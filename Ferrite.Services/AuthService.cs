﻿//
//  Project Ferrite is an Implementation Telegram Server API
//  Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Affero General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Affero General Public License for more details.
//
//  You should have received a copy of the GNU Affero General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using System.Text;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Data.Account;
using Ferrite.Data.Auth;
using Ferrite.Data.Repositories;
using Ferrite.Services.Gateway;
using Ferrite.TL.slim;
using Ferrite.TL.slim.layer150;
using Ferrite.TL.slim.layer150.auth;
using Ferrite.TL.slim.mtproto;
using Ferrite.Utils;
using xxHash;

namespace Ferrite.Services;

public class AuthService : IAuthService
{
    private readonly IRandomGenerator _random;
    private readonly ISearchEngine _search;
    private readonly IAtomicCounter _userIdCnt;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVerificationGateway _verificationGateway;
    private readonly ILogger _log;
    private const int PhoneCodeTimeout = 60;//seconds

    public AuthService(IRandomGenerator random, ISearchEngine search,
        IUnitOfWork unitOfWork, ICounterFactory counterFactory,
        IVerificationGateway verificationGateway,
        ILogger log)
    {
        _random = random;
        _search = search;
        _userIdCnt = counterFactory.GetCounter("counter_user_id");
        _unitOfWork = unitOfWork;
        _verificationGateway = verificationGateway;
        _log = log;
    }

    public async Task<TLBytes?> AcceptLoginToken(long authKeyId, byte[] token)
    {
        var t = _unitOfWork.LoginTokenRepository.GetLoginToken(token);
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth != null && t != null&& t.ExceptUserIds.Contains(auth.UserId))
        {
            var login = new LoginViaQRDTO()
            {
                AuthKeyId = t.AuthKeyId,
                SessionId = t.SessionId,
                Token = t.Token,
                ExceptUserIds = t.ExceptUserIds,
                AcceptedByUserId = auth.UserId,
                Status = true
            };
            _unitOfWork.LoginTokenRepository.PutLoginToken(login, new TimeSpan(0, 0, 60));
            _unitOfWork.AuthorizationRepository.PutAuthorization(new AuthInfoDTO()
            {
                AuthKeyId = t.AuthKeyId,
                Phone = auth.Phone,
                UserId = auth.UserId,
                ApiLayer = -1,
                LoggedIn = true
            });
            await _unitOfWork.SaveAsync();
            var app = _unitOfWork.AppInfoRepository.GetAppInfo(t.AuthKeyId);
            return app;
        }
        return null;
    }

    public async ValueTask<TLBytes> BindTempAuthKey(long sessionId, TLBytes q)
    {
        var bindParameters = GetBindTempAuthKeyParameters(sessionId, q);
        if (bindParameters == null)
        {
            return RpcErrorGenerator.GenerateError(400, 
                "ENCRYPTED_MESSAGE_INVALID"u8);
        }

        if (await _unitOfWork.BoundAuthKeyRepository.GetBoundAuthKeyAsync(bindParameters.Value.TempAuthKeyId) != null)
        {
            return RpcErrorGenerator.GenerateError(400, 
                "TEMP_AUTH_KEY_ALREADY_BOUND"u8);
        }
        
        _unitOfWork.BoundAuthKeyRepository.PutBoundAuthKey(bindParameters.Value.TempAuthKeyId, 
            bindParameters.Value.PermAuthKeyId, 
            new TimeSpan(0, 0, bindParameters.Value.ExpiresAt));
        var result = await _unitOfWork.SaveAsync();
        return result ? BoolTrue.Builder().Build().TLBytes!.Value : 
            BoolFalse.Builder().Build().TLBytes!.Value;
    }

    private readonly record struct BindTempAuthKeyParameters(long TempAuthKeyId, long PermAuthKeyId, int ExpiresAt);

    private BindTempAuthKeyParameters? GetBindTempAuthKeyParameters(long sessionId, TLBytes q)
    {
        using var bindRequest = new BindTempAuthKey(q.AsSpan());
        var authKey = _unitOfWork.AuthKeyRepository.GetAuthKey(bindRequest.PermAuthKeyId);
        if (authKey == null) return null;
        Span<byte> encrypted = stackalloc byte[bindRequest.EncryptedMessage.Length];
        bindRequest.EncryptedMessage.CopyTo(encrypted);
        using var bindDataInner = DecryptBindingMessage(authKey, encrypted);
        if (bindDataInner.PermAuthKeyId != bindRequest.PermAuthKeyId ||
            bindDataInner.Nonce != bindRequest.Nonce ||
            bindDataInner.TempSessionId != sessionId) return null;
        return new BindTempAuthKeyParameters(bindDataInner.TempAuthKeyId, bindDataInner.PermAuthKeyId,
            bindRequest.ExpiresAt);
    }
    
    private BindAuthKeyInner DecryptBindingMessage(Span<byte> authKey, Span<byte> encrypted)
    {
        Span<byte> messageKey = encrypted.Slice(8, 16);
        AesIgeV1 aesIge = new AesIgeV1(authKey, messageKey);
        aesIge.Decrypt(encrypted[24..]);
        return new BindAuthKeyInner(encrypted[(24 + 32)..]);
    }

    public async ValueTask<TLBytes> CancelCode(TLBytes q)
    {
        var (phoneNumber, phoneCodeHash) = GetCancelCodeParameters(q);
        var result = _unitOfWork.PhoneCodeRepository.DeletePhoneCode(phoneNumber, phoneCodeHash);
        result = result && await _unitOfWork.SaveAsync();
        return result ? (TLBytes)BoolTrue.Builder().Build().TLBytes! : 
            (TLBytes)BoolFalse.Builder().Build().TLBytes!;
    }
    
    private static CancelCodeParameters GetCancelCodeParameters(TLBytes q)
    {
        var cancelCode = new CancelCode(q.AsSpan());
        var phoneNumber = Encoding.UTF8.GetString(cancelCode.PhoneNumber);
        var phoneCodeHash = Encoding.UTF8.GetString(cancelCode.PhoneCodeHash);
        return new CancelCodeParameters(phoneNumber, phoneCodeHash);
    }

    private readonly record struct CancelCodeParameters(string PhoneNumber, string PhoneCodeHash);

    public Task<AuthorizationDTO> CheckPassword(bool empty, long srpId, byte[] A, byte[] M1)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CheckRecoveryPassword(string code)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<TLBytes> DropTempAuthKeys(long authKeyId, ICollection<long> exceptAuthKeys)
    {
        var tempKeys = _unitOfWork.BoundAuthKeyRepository.GetTempAuthKeys(authKeyId);
        foreach (var key in tempKeys)
        {
            if (!exceptAuthKeys.Contains(key))
            {
                _unitOfWork.TempAuthKeyRepository.DeleteTempAuthKey(key);
            }
        }

        var result = await _unitOfWork.SaveAsync();
        return result ? BoolTrue.Builder().Build().TLBytes!.Value : 
            BoolFalse.Builder().Build().TLBytes!.Value;
    }

    public async ValueTask<TLBytes> ExportAuthorization(long authKeyId, int currentDc, TLBytes q)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return RpcErrorGenerator.GenerateError(400, 
                "AUTH_KEY_UNREGISTERED"u8);
        }
        var data = _random.GetRandomBytes(128);
        var dcId = new ExportAuthorization(q.AsSpan()).DcId;
        _unitOfWork.AuthorizationRepository.PutExportedAuthorization(new ExportedAuthInfoDTO
        {
            Data = data,
            UserId = auth.UserId,
            Phone = auth.Phone,
            AuthKeyId = auth.AuthKeyId,
            NextDcId = dcId,
            PreviousDcId = currentDc,
        });
        await _unitOfWork.SaveAsync();
        return ExportedAuthorization
            .Builder()
            .Id(auth.UserId)
            .Bytes(data).Build().TLBytes!.Value;
    }

    public async ValueTask<TLBytes> ExportLoginToken(long authKeyId, long sessionId, TLBytes q)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth is { LoggedIn: true })
        {
            using var user = _unitOfWork.UserRepository.GetUser(auth.UserId);
            if (user != null)
            {
                using var authorization = GenerateAuthorization(user.Value);
                return LoginTokenSuccess
                    .Builder()
                    .Authorization(authorization.AsSpan())
                    .Build().TLBytes!.Value;
            }
        }
        var token = _random.GetRandomBytes(16);
        var tokenParameters = GetExportLoginTokenParameters(q);
        LoginViaQRDTO login = new LoginViaQRDTO()
        {
            Token = token,
            AuthKeyId = authKeyId,
            SessionId = sessionId,
            Status = false,
            ExceptUserIds = tokenParameters.ExceptIds
        };
        _unitOfWork.LoginTokenRepository.PutLoginToken(login, new TimeSpan(0, 0, 30));
        await _unitOfWork.SaveAsync();
        return LoginToken
            .Builder()
            .Token(token)
            .Expires(30)
            .Build().TLBytes!.Value;
    }

    private readonly record struct ExportLoginTokenParameters(int ApiId, string ApiHash, ICollection<long> ExceptIds);

    private static ExportLoginTokenParameters GetExportLoginTokenParameters(TLBytes q)
    {
        var exportRequest = new ExportLoginToken(q.AsSpan());
        var apiHash = Encoding.UTF8.GetString(exportRequest.ApiHash);
        var ids = new long[exportRequest.ExceptIds.Count];
        for (int i = 0; i < exportRequest.ExceptIds.Count; i++)
        {
            ids[i] = exportRequest.ExceptIds[i];
        }
        return new ExportLoginTokenParameters(exportRequest.ApiId, apiHash, ids);
    }
    
    public async ValueTask<TLBytes> ImportAuthorization(long authKeyId, TLBytes q)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var importParameters = GetImportAuthorizationParameters(q);
        var exported = await _unitOfWork.AuthorizationRepository
            .GetExportedAuthorizationAsync(importParameters.UserId, importParameters.Bytes);
        
        if (auth != null && exported != null &&
            auth.Phone == exported.Phone && importParameters.Bytes.SequenceEqual(exported.Data))
        {
            var user = _unitOfWork.UserRepository.GetUser(auth.UserId);
            if(user == null)
            {
                return RpcErrorGenerator.GenerateError(400, "USER_ID_INVALID"u8);
            }

            return GenerateAuthorization(user.Value);
        }
        return RpcErrorGenerator.GenerateError(400, "AUTH_BYTES_INVALID"u8);
    }

    private readonly record struct ImportAuthorizationParameters(long UserId, byte[] Bytes);

    private static ImportAuthorizationParameters GetImportAuthorizationParameters(TLBytes q)
    {
        var importAuthorization = new ImportAuthorization(q.AsSpan());
        return new ImportAuthorizationParameters(importAuthorization.Id, importAuthorization.Bytes.ToArray());
    }

    public Task<AuthorizationDTO> ImportBotAuthorization(int apiId, string apiHash, string botAuthToken)
    {
        throw new NotImplementedException();
    }

    public Task<LoginTokenDTO> ImportLoginToken(byte[] token)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<bool> IsAuthorized(long authKeyId)
    {
        if (authKeyId == 0)
        {
            return false;
        }
        var authKeyDetails = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (authKeyDetails == null)
        {
            var permAuthKey = await _unitOfWork.BoundAuthKeyRepository.GetBoundAuthKeyAsync(authKeyId);
            if (permAuthKey != null)
            {
                authKeyDetails = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync((long)permAuthKey);
            }
        }
        return authKeyDetails?.LoggedIn ?? false;
    }

    public async ValueTask<TLBytes> LogOut(long authKeyId)
    {
        var futureAuthToken = _random.GetRandomBytes(32);
        var info = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if(info == null)
        {
            return LoggedOut
                .Builder()
                .Build().TLBytes!.Value;
        }
        _unitOfWork.AuthorizationRepository.PutAuthorization(info with
        {
            FutureAuthToken = futureAuthToken,
            Phone = "",
            UserId = 0,
            LoggedIn = false
        });
        _log.Debug($"Log Out for authKey with Id: {authKeyId}");
        await _unitOfWork.SaveAsync();
        return LoggedOut
            .Builder()
            .FutureAuthToken(futureAuthToken)
            .Build().TLBytes!.Value;
    }

    public Task<AuthorizationDTO> RecoverPassword(string code, PasswordInputSettingsDTO newSettings)
    {
        throw new NotImplementedException();
    }

    public Task<string> RequestPasswordRecovery()
    {
        throw new NotImplementedException();
    }

    public async ValueTask<TLBytes> ResendCode(TLBytes q)
    {
        var (phoneNumber, phoneCodeHash) = GetResendCodeParameters(q);
    
        var phoneCode = _unitOfWork.PhoneCodeRepository.GetPhoneCode(phoneNumber, phoneCodeHash);
        if (phoneCode != null)
        {
            _unitOfWork.PhoneCodeRepository.PutPhoneCode(phoneNumber, phoneCodeHash, phoneCode,
                new TimeSpan(0, 0, PhoneCodeTimeout * 2));
            await _unitOfWork.SaveAsync();
            await _verificationGateway.Resend(phoneNumber, phoneCode);

            return GenerateSentCode(Encoding.UTF8.GetBytes(phoneCodeHash));
        }

        return RpcErrorGenerator.GenerateError(400, "PHONE_CODE_EXPIRED"u8);
    }
    
    private static ResendCodeParameters GetResendCodeParameters(TLBytes q)
    {
        var sendCode = new ResendCode(q.AsSpan());
        var phoneNumber = Encoding.UTF8.GetString(sendCode.PhoneNumber);
        var phoneCodeHash = Encoding.UTF8.GetString(sendCode.PhoneCodeHash);
        return new ResendCodeParameters(phoneNumber, phoneCodeHash);
    }
    
    private readonly record struct ResendCodeParameters(string PhoneNumber, string PhoneCodeHash);

    public async ValueTask<TLBytes> ResetAuthorizations(long authKeyId)
    {
        var currentAuth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (currentAuth != null)
        {
            var authorizations = await _unitOfWork.AuthorizationRepository.GetAuthorizationsAsync(currentAuth.Phone);
            foreach (var auth in authorizations)
            {
                if(auth.AuthKeyId != authKeyId)
                {
                    _unitOfWork.AuthorizationRepository.DeleteAuthorization(authKeyId);
                }
            }

            await _unitOfWork.SaveAsync();
            return BoolTrue.Builder().Build().TLBytes!.Value;
        }
        return BoolFalse.Builder().Build().TLBytes!.Value;
    }

    public async ValueTask<TLBytes> SendCode(TLBytes q)
    {
        var (phoneNumber, apiId, apiHash) = GetSendCodeParameters(q);
        var code = await _verificationGateway.SendSms(phoneNumber);
        var hash = GeneratePhoneCodeHash(code);
        
        _unitOfWork.PhoneCodeRepository.PutPhoneCode(phoneNumber, hash, code,
            new TimeSpan(0, 0, PhoneCodeTimeout*2));
        await _unitOfWork.SaveAsync();
        return GenerateSentCode(Encoding.UTF8.GetBytes(hash));
    }

    private string GeneratePhoneCodeHash(string code)
    {
        var codeBytes = Encoding.UTF8.GetBytes(code);
        return codeBytes.GetXxHash64(1071).ToString("x");
    }

    private static SendCodeParameters GetSendCodeParameters(TLBytes q)
    {
        var sendCode = new SendCode(q.AsSpan());
        var phoneNumber = Encoding.UTF8.GetString(sendCode.PhoneNumber);
        var apiHash = Encoding.UTF8.GetString(sendCode.ApiHash);
        return new SendCodeParameters(phoneNumber, sendCode.ApiId, apiHash);
    }
    
    private readonly record struct SendCodeParameters(string PhoneNumber, int ApiId, string PhoneCodeHash);
    
    private static TLBytes GenerateSentCode(ReadOnlySpan<byte> phoneCodeHash)
    {
        return ((TLBytes)SentCode.Builder()
            .Type(new SentCodeTypeSms(5).ToReadOnlySpan())
            .Timeout(PhoneCodeTimeout)
            .PhoneCodeHash(phoneCodeHash)
            .Build().TLBytes!);
    }

    public async ValueTask<TLBytes> SignIn(long authKeyId, TLBytes q)
    {
        var signInParameters = GetSignInParameters(q);
        var (phoneNumber, phoneCodeHash, phoneCode) = signInParameters;
        _log.Debug($"*** Sign In for authKey with Id: {authKeyId} ***");
        var code = _unitOfWork.PhoneCodeRepository.GetPhoneCode(phoneNumber, phoneCodeHash);
        if (code != phoneCode)
        {
            return RpcErrorGenerator.GenerateError(400, "PHONE_CODE_INVALID"u8);
        }
        _unitOfWork.SignInRepository.PutSignIn(authKeyId, phoneNumber, phoneCodeHash);
        await _unitOfWork.SaveAsync();
        var userId = _unitOfWork.UserRepository.GetUserId(phoneNumber);
        if(userId == null)
        {
            return GenerateSignUpRequired();
        }
        var user = _unitOfWork.UserRepository.GetUser(userId.Value);
        if(user == null)
        {
            return GenerateSignUpRequired();
        }

        user = SetUserAttributes(user.Value);
        var authKeyDetails = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        _unitOfWork.AuthorizationRepository.PutAuthorization(new AuthInfoDTO()
        {
            AuthKeyId = authKeyId,
            Phone = phoneNumber,
            UserId = userId.Value,
            ApiLayer = authKeyDetails?.ApiLayer ?? -1,
            LoggedIn = true
        });
        await _unitOfWork.SaveAsync();
        return GenerateAuthorization(user.Value);
    }

    private TLBytes SetUserAttributes(TLBytes user)
    {
        using (user)
        {
            var u = new User(user.AsSpan());
            var userModified = u.Clone()
                .Self(true).Build();
            return userModified.TLBytes!.Value;
        }
    }

    private static SignInParameters GetSignInParameters(TLBytes q)
    {
        var signIn = new SignIn(q.AsSpan());
        var phoneNumber = Encoding.UTF8.GetString(signIn.PhoneNumber);
        var phoneCodeHash = Encoding.UTF8.GetString(signIn.PhoneCodeHash);
        var phoneCode = Encoding.UTF8.GetString(signIn.PhoneCode);
        return new SignInParameters(phoneNumber, phoneCodeHash, phoneCode);
    }

    private readonly record struct SignInParameters(string PhoneNumber, string PhoneCodeHash, string PhoneCode);

    private TLBytes GenerateSignUpRequired()
    {
        var signupRequired = AuthorizationSignUpRequired.Builder().Build();
        return signupRequired.TLBytes!.Value;
    }

    public async ValueTask<TLBytes> SignUp(long authKeyId, TLBytes q)
    {
        _log.Debug($"*** Sign Up for authKey with Id: {authKeyId} ***");
        long userId = await _userIdCnt.IncrementAndGet();
        var signUpParameters = GetSignUpParameters(q);
        if(userId == 0)
        {
            userId = await _userIdCnt.IncrementAndGet();
        }
        var phoneCode = _unitOfWork.PhoneCodeRepository.GetPhoneCode(signUpParameters.PhoneNumber, 
            signUpParameters.PhoneCodeHash);
        var signedInAuthKeyId = _unitOfWork.SignInRepository.GetSignIn(signUpParameters.PhoneNumber, 
            signUpParameters.PhoneCodeHash);
        if(phoneCode == null || signedInAuthKeyId != authKeyId)
        {
            return RpcErrorGenerator.GenerateError(400, "PHONE_CODE_INVALID"u8);
        }
        var hash = GeneratePhoneCodeHash(phoneCode);
        if (signUpParameters.PhoneCodeHash != hash)
        {
            return RpcErrorGenerator.GenerateError(400, "PHONE_CODE_INVALID"u8);
        }
        
        using var user = SaveUser(userId, signUpParameters.PhoneNumber, 
            signUpParameters.FirstName, signUpParameters.LastName);
        await _search.IndexUser(new Data.Search.UserSearchModel(userId, "", 
            signUpParameters.FirstName, signUpParameters.LastName, signUpParameters.PhoneNumber));
        var authKeyDetails = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        _unitOfWork.AuthorizationRepository.PutAuthorization(new AuthInfoDTO()
        {
            AuthKeyId = authKeyId,
            Phone = signUpParameters.PhoneNumber,
            UserId = userId,
            ApiLayer = authKeyDetails?.ApiLayer ?? -1,
            LoggedIn = true
        });
        await _unitOfWork.SaveAsync();
        return GenerateAuthorization(user);
    }
    
    private static SignUpParameters GetSignUpParameters(TLBytes q)
    {
        var signUp = new SignUp(q.AsSpan());
        var phoneNumber = Encoding.UTF8.GetString(signUp.PhoneNumber);
        var phoneCodeHash = Encoding.UTF8.GetString(signUp.PhoneCodeHash);
        var firstName = Encoding.UTF8.GetString(signUp.FirstName);
        var lastName = Encoding.UTF8.GetString(signUp.LastName);
        return new SignUpParameters(phoneNumber, phoneCodeHash, firstName, lastName);
    }

    private readonly record struct SignUpParameters(string PhoneNumber, 
        string PhoneCodeHash, string FirstName, string LastName);

    private TLBytes GenerateAuthorization(TLBytes user)
    {
        using var userModified = new User(user.AsSpan())
            .Clone()
            .Self(true)
            .Build();
        var authorization = AuthAuthorization.Builder()
            .User(userModified.ToReadOnlySpan()).Build();
        return authorization.TLBytes!.Value;
    }

    private TLBytes SaveUser(long userId ,string phoneNumber, string firstName, string lastName)
    {
        using var photo = UserProfilePhotoEmpty.Builder().Build();
        var user = User.Builder()
            .Id(userId)
            .Phone(Encoding.UTF8.GetBytes(phoneNumber))
            .FirstName(Encoding.UTF8.GetBytes(firstName))
            .LastName(Encoding.UTF8.GetBytes(lastName))
            .AccessHash(_random.NextLong())
            .Photo(photo.TLBytes!.Value.AsSpan())
            .Build();
        _unitOfWork.UserRepository.PutUser(user.TLBytes!.Value);
        return user.TLBytes!.Value;
    }

    public async ValueTask<bool> SaveAppInfo(TLBytes info)
    {
        _unitOfWork.AppInfoRepository.PutAppInfo(info);
        return await _unitOfWork.SaveAsync();
    }

    public async ValueTask<TLBytes?> GetAppInfo(long authKeyId)
    {
        return _unitOfWork.AppInfoRepository.GetAppInfo(authKeyId);
    }
}

