//
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
using Ferrite.TL.slim.dto;
using Ferrite.TL.slim.layer150;
using Ferrite.TL.slim.layer150.auth;
using Ferrite.TL.slim.mtproto;
using Ferrite.Utils;
using xxHash;
using TLAuthorization = Ferrite.TL.slim.layer150.auth.TLAuthorization;

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

    public async ValueTask<TLBool> BindTempAuthKey(long sessionId, TLBytes q)
    {
        var bindParameters = GetBindTempAuthKeyParameters(sessionId, q);
        if (bindParameters == null)
        {
            return (TLBool)RpcErrorGenerator.GenerateError(400, 
                "ENCRYPTED_MESSAGE_INVALID"u8);
        }

        if (await _unitOfWork.BoundAuthKeyRepository.GetBoundAuthKeyAsync(bindParameters.Value.TempAuthKeyId) != null)
        {
            return (TLBool)RpcErrorGenerator.GenerateError(400, 
                "TEMP_AUTH_KEY_ALREADY_BOUND"u8);
        }
        
        _unitOfWork.BoundAuthKeyRepository.PutBoundAuthKey(bindParameters.Value.TempAuthKeyId, 
            bindParameters.Value.PermAuthKeyId, 
            new TimeSpan(0, 0, bindParameters.Value.ExpiresAt));
        var result = await _unitOfWork.SaveAsync();
        return result ? new BoolTrue() : new BoolFalse();
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

    public async ValueTask<TLBool> CancelCode(TLBytes q)
    {
        var (phoneNumber, phoneCodeHash) = GetCancelCodeParameters(q);
        var result = _unitOfWork.PhoneCodeRepository.DeletePhoneCode(phoneNumber, phoneCodeHash);
        result = result && await _unitOfWork.SaveAsync();
        return result ? new BoolTrue() : new BoolFalse();
    }
    
    private static CancelCodeParameters GetCancelCodeParameters(TLBytes q)
    {
        var cancelCode = new CancelCode(q.AsSpan());
        var phoneNumber = Encoding.UTF8.GetString(cancelCode.PhoneNumber);
        var phoneCodeHash = Encoding.UTF8.GetString(cancelCode.PhoneCodeHash);
        return new CancelCodeParameters(phoneNumber, phoneCodeHash);
    }

    private readonly record struct CancelCodeParameters(string PhoneNumber, string PhoneCodeHash);

    public async ValueTask<TLBool> DropTempAuthKeys(long authKeyId, ICollection<long> exceptAuthKeys)
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
        return result ? new BoolTrue() : new BoolFalse();
    }

    public async ValueTask<TLExportedAuthorization> ExportAuthorization(long authKeyId, int currentDc, TLBytes q)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return (TLExportedAuthorization)RpcErrorGenerator.GenerateError(400, 
                "AUTH_KEY_UNREGISTERED"u8);
        }
        var data = _random.GetRandomBytes(128);
        var dcId = new ExportAuthorization(q.AsSpan()).DcId;
        _unitOfWork.AuthorizationRepository.PutExportedAuthorization(ExportedAuthInfo.Builder()
            .Data(data)
            .UserId(auth.Value.AsAuthInfo().UserId)
            .Phone(auth.Value.AsAuthInfo().Phone)
            .AuthKeyId(auth.Value.AsAuthInfo().AuthKeyId)
            .NextDcId(dcId)
            .PreviousDcId(currentDc)
            .Build());
        await _unitOfWork.SaveAsync();
        return ExportedAuthorization
            .Builder()
            .Id(auth.Value.AsAuthInfo().UserId)
            .Bytes(data).Build();
    }

    public async ValueTask<TLLoginToken> ExportLoginToken(long authKeyId, long sessionId, TLBytes q)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth != null && auth.Value.AsAuthInfo().LoggedIn)
        {
            using var user = _unitOfWork.UserRepository.GetUser(auth.Value.AsAuthInfo().UserId);
            if (user != null)
            {
                using var authorization = GenerateAuthorization(user.Value);
                return LoginTokenSuccess
                    .Builder()
                    .Authorization(authorization.AsSpan())
                    .Build();
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
            .Build();
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
    
    public async ValueTask<TLAuthorization> ImportAuthorization(long authKeyId, TLBytes q)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var importParameters = GetImportAuthorizationParameters(q);
        var exported = await _unitOfWork.AuthorizationRepository
            .GetExportedAuthorizationAsync(importParameters.UserId, importParameters.Bytes);
        
        if (auth != null && exported != null &&
            auth.Value.AsAuthInfo().Phone.SequenceEqual(exported.Value.AsExportedAuthInfo().Phone) && 
            importParameters.Bytes.AsSpan().SequenceEqual(exported.Value.AsExportedAuthInfo().Data))
        {
            var user = _unitOfWork.UserRepository.GetUser(auth.Value.AsAuthInfo().UserId);
            if(user == null)
            {
                return (TLAuthorization)RpcErrorGenerator.GenerateError(400, "USER_ID_INVALID"u8);
            }

            return GenerateAuthorization(user.Value);
        }
        return (TLAuthorization)RpcErrorGenerator.GenerateError(400, "AUTH_BYTES_INVALID"u8);
    }

    private readonly record struct ImportAuthorizationParameters(long UserId, byte[] Bytes);

    private static ImportAuthorizationParameters GetImportAuthorizationParameters(TLBytes q)
    {
        var importAuthorization = new ImportAuthorization(q.AsSpan());
        return new ImportAuthorizationParameters(importAuthorization.Id, importAuthorization.Bytes.ToArray());
    }
    
    public async ValueTask<bool> IsAuthorized(long authKeyId)
    {
        if (authKeyId == 0)
        {
            return false;
        }

        var authKeyDetails = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (authKeyDetails != null) return authKeyDetails.Value.AsAuthInfo().LoggedIn;
        var permAuthKey = await _unitOfWork.BoundAuthKeyRepository.GetBoundAuthKeyAsync(authKeyId);
        if (permAuthKey != null)
        {
            authKeyDetails = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync((long)permAuthKey);
        }

        return authKeyDetails != null && authKeyDetails.Value.AsAuthInfo().LoggedIn;
    }

    public async ValueTask<TLLoggedOut> LogOut(long authKeyId)
    {
        var futureAuthToken = _random.GetRandomBytes(32);
        var info = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if(info == null)
        {
            return LoggedOut
                .Builder()
                .Build();
        }

        _unitOfWork.AuthorizationRepository.DeleteAuthorization(info.Value.AsAuthInfo().AuthKeyId);
        _log.Debug($"Log Out for authKey with Id: {authKeyId}");
        await _unitOfWork.SaveAsync();
        return LoggedOut
            .Builder()
            .FutureAuthToken(futureAuthToken)
            .Build();
    }
    
    public async ValueTask<TLSentCode> ResendCode(TLBytes q)
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

        return (TLSentCode)RpcErrorGenerator.GenerateError(400, "PHONE_CODE_EXPIRED"u8);
    }
    
    private static ResendCodeParameters GetResendCodeParameters(TLBytes q)
    {
        var sendCode = new ResendCode(q.AsSpan());
        var phoneNumber = Encoding.UTF8.GetString(sendCode.PhoneNumber);
        var phoneCodeHash = Encoding.UTF8.GetString(sendCode.PhoneCodeHash);
        return new ResendCodeParameters(phoneNumber, phoneCodeHash);
    }
    
    private readonly record struct ResendCodeParameters(string PhoneNumber, string PhoneCodeHash);

    public async ValueTask<TLBool> ResetAuthorizations(long authKeyId)
    {
        var currentAuth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (currentAuth != null)
        {
            var authorizations = await _unitOfWork
                .AuthorizationRepository.GetAuthorizationsAsync(
                    Encoding.UTF8.GetString(currentAuth.Value.AsAuthInfo().Phone));
            foreach (var auth in authorizations)
            {
                if(auth.AsAuthInfo().AuthKeyId != authKeyId)
                {
                    _unitOfWork.AuthorizationRepository.DeleteAuthorization(authKeyId);
                }
            }

            await _unitOfWork.SaveAsync();
            return new BoolTrue();
        }

        return new BoolFalse();
    }

    public async ValueTask<TLSentCode> SendCode(TLBytes q)
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
    
    private static TLSentCode GenerateSentCode(ReadOnlySpan<byte> phoneCodeHash)
    {
        using var codeType = new SentCodeTypeSms(5);
        return SentCode.Builder()
            .Type(codeType.ToReadOnlySpan())
            .Timeout(PhoneCodeTimeout)
            .PhoneCodeHash(phoneCodeHash)
            .Build();
    }

    public async ValueTask<TLAuthorization> SignIn(long authKeyId, TLBytes q)
    {
        var signInParameters = GetSignInParameters(q);
        var (phoneNumber, phoneCodeHash, phoneCode) = signInParameters;
        _log.Debug($"*** Sign In for authKey with Id: {authKeyId} ***");
        var code = _unitOfWork.PhoneCodeRepository.GetPhoneCode(phoneNumber, phoneCodeHash);
        if (code != phoneCode)
        {
            return (TLAuthorization)RpcErrorGenerator.GenerateError(400, "PHONE_CODE_INVALID"u8);
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
        var apiLayer = authKeyDetails != null ? authKeyDetails.Value.AsAuthInfo().ApiLayer : -1;
        _unitOfWork.AuthorizationRepository.PutAuthorization(AuthInfo.Builder()
            .AuthKeyId(authKeyId)
            .Phone(Encoding.UTF8.GetBytes(phoneNumber))
            .UserId(userId.Value)
            .ApiLayer(apiLayer)
            .LoggedIn(true)
            .Build());
        await _unitOfWork.SaveAsync();
        return GenerateAuthorization(user.Value);
    }

    private TLUser SetUserAttributes(TLUser user)
    {
        using (user)
        {
            var u = user.AsUser();
            var userModified = u.Clone()
                .Self(true).Build();
            return userModified;
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

    private TLAuthorization GenerateSignUpRequired()
    {
        var signupRequired = AuthorizationSignUpRequired.Builder().Build();
        return signupRequired;
    }

    public async ValueTask<TLAuthorization> SignUp(long authKeyId, TLBytes q)
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
            return (TLAuthorization)RpcErrorGenerator.GenerateError(400, "PHONE_CODE_INVALID"u8);
        }
        var hash = GeneratePhoneCodeHash(phoneCode);
        if (signUpParameters.PhoneCodeHash != hash)
        {
            return (TLAuthorization)RpcErrorGenerator.GenerateError(400, "PHONE_CODE_INVALID"u8);
        }
        
        using var user = SaveUser(userId, signUpParameters.PhoneNumber, 
            signUpParameters.FirstName, signUpParameters.LastName);
        await _search.IndexUser(new Data.Search.UserSearchModel(userId, "", 
            signUpParameters.FirstName, signUpParameters.LastName, signUpParameters.PhoneNumber));
        var authKeyDetails = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var apiLayer = authKeyDetails != null ? authKeyDetails.Value.AsAuthInfo().ApiLayer : -1;
        _unitOfWork.AuthorizationRepository.PutAuthorization(AuthInfo.Builder()
            .AuthKeyId(authKeyId)
            .Phone(Encoding.UTF8.GetBytes(signUpParameters.PhoneNumber))
            .UserId(userId)
            .ApiLayer(apiLayer)
            .LoggedIn(true)
            .Build());
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

    private TLAuthorization GenerateAuthorization(TLBytes user)
    {
        using var userModified = new User(user.AsSpan())
            .Clone()
            .Self(true)
            .Build();
        return AuthAuthorization.Builder()
            .User(userModified.ToReadOnlySpan()).Build();
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
        _unitOfWork.UserRepository.PutUser(user);
        return user.TLBytes!.Value;
    }

    public async ValueTask<bool> SaveAppInfo(TLAppInfo info)
    {
        _unitOfWork.AppInfoRepository.PutAppInfo(info);
        return await _unitOfWork.SaveAsync();
    }

    public async ValueTask<TLAppInfo?> GetAppInfo(long authKeyId)
    {
        return _unitOfWork.AppInfoRepository.GetAppInfo(authKeyId);
    }
}

