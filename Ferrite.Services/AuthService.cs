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
using System;
using System.Text;
using System.Threading.Tasks.Sources;
using DotNext.IO;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Data.Account;
using Ferrite.Data.Auth;
using Ferrite.Data.Repositories;
using Ferrite.TL.slim;
using Ferrite.TL.slim.layer148.auth;
using Ferrite.Utils;
using xxHash;

namespace Ferrite.Services;

public class AuthService : IAuthService
{
    private readonly IRandomGenerator _random;
    private readonly ISearchEngine _search;
    private readonly IAtomicCounter _userIdCnt;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsersService _users;
    private readonly ILogger _log;

    private const int PhoneCodeTimeout = 60;//seconds

    public AuthService(IRandomGenerator random, ISearchEngine search,
        IUnitOfWork unitOfWork, ICounterFactory counterFactory, 
        IUsersService users, ILogger log)
    {
        _random = random;
        _search = search;
        _userIdCnt = counterFactory.GetCounter("counter_user_id");
        _unitOfWork = unitOfWork;
        _users = users;
        _log = log;
    }

    public async Task<AppInfoDTO?> AcceptLoginToken(long authKeyId, byte[] token)
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

    public async Task<bool> BindTempAuthKey(long tempAuthKeyId, long permAuthKeyId, int expiresAt)
    {
        _unitOfWork.BoundAuthKeyRepository.PutBoundAuthKey(tempAuthKeyId, 
            permAuthKeyId, new TimeSpan(0, 0, expiresAt));
        return await _unitOfWork.SaveAsync();
    }

    public async Task<bool> CancelCode(string phoneNumber, string phoneCodeHash)
    {
        _unitOfWork.PhoneCodeRepository.DeletePhoneCode(phoneNumber, phoneCodeHash);
        return await _unitOfWork.SaveAsync();
    }

    public Task<AuthorizationDTO> CheckPassword(bool empty, long srpId, byte[] A, byte[] M1)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CheckRecoveryPassword(string code)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DropTempAuthKeys(long authKeyId, ICollection<long> exceptAuthKeys)
    {
        var tempKeys = _unitOfWork.BoundAuthKeyRepository.GetTempAuthKeys(authKeyId);
        foreach (var key in tempKeys)
        {
            if (!exceptAuthKeys.Contains(key))
            {
                _unitOfWork.TempAuthKeyRepository.DeleteTempAuthKey(key);
            }
        }

        return await _unitOfWork.SaveAsync();
    }

    public async Task<ExportedAuthorizationDTO> ExportAuthorization(long authKeyId, int dcId)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var data = _random.GetRandomBytes(128);
        //TODO: get current dc id
        //auth, 1, dcId, data
        _unitOfWork.AuthorizationRepository.PutExportedAuthorization(new ExportedAuthInfoDTO
        {
            Data = data,
            UserId = auth.UserId,
            Phone = auth.Phone,
            AuthKeyId = auth.AuthKeyId,
            NextDcId = dcId,
            PreviousDcId = 1,
        });
        await _unitOfWork.SaveAsync();
        return new ExportedAuthorizationDTO()
        {
            Id = auth.UserId,
            Bytes = data
        };
    }

    public async Task<LoginTokenDTO> ExportLoginToken(long authKeyId, long sessionId, int apiId, string apiHash, ICollection<long> exceptIds)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth!= null && auth.LoggedIn &&
            _users.GetUser(auth.UserId) is UserDTO user)
        {
            return new LoginTokenDTO()
            {
                LoginTokenType = LoginTokenType.TokenSuccess,
                Authorization = new AuthorizationDTO()
                {
                    AuthorizationType = AuthorizationType.Authorization,
                    User = new UserDTO()
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Phone = user.Phone,
                        Status = UserStatusDTO.Empty,
                        Self = true,
                        Photo = user.Photo
                    }
                }
        };
        }
        var token = _random.GetRandomBytes(16);
        LoginViaQRDTO login = new LoginViaQRDTO()
        {
            Token = token,
            AuthKeyId = authKeyId,
            SessionId = sessionId,
            Status = false,
            ExceptUserIds = exceptIds
        };
        _unitOfWork.LoginTokenRepository.PutLoginToken(login, new TimeSpan(0, 0, 30));
        await _unitOfWork.SaveAsync();
        return new LoginTokenDTO()
        {
            LoginTokenType = LoginTokenType.Token,
            Token = token,
            Expires = 30
        };
    }

    public async Task<AuthorizationDTO> ImportAuthorization(long userId, long authKeyId, byte[] bytes)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var exported = await _unitOfWork.AuthorizationRepository.GetExportedAuthorizationAsync(userId, bytes);
        
        if (auth != null && exported != null &&
            auth.Phone == exported.Phone && bytes.SequenceEqual(exported.Data))
        {
            var user = _users.GetUser(auth.UserId);
            if(user == null)
            {
                return new AuthorizationDTO()
                {
                    AuthorizationType = AuthorizationType.UserIdInvalid
                };
            }
            return new AuthorizationDTO()
            {
                AuthorizationType = AuthorizationType.Authorization,
                User = new UserDTO()
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Phone = user.Phone,
                    Status = UserStatusDTO.Empty,
                    Self = true,
                    Photo = user.Photo
                }
            };
        }
        return new AuthorizationDTO()
        {
            AuthorizationType = AuthorizationType.AuthBytesInvalid
        };
    }

    public Task<AuthorizationDTO> ImportBotAuthorization(int apiId, string apiHash, string botAuthToken)
    {
        throw new NotImplementedException();
    }

    public Task<LoginTokenDTO> ImportLoginToken(byte[] token)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> IsAuthorized(long authKeyId)
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

    public async Task<LoggedOutDTO?> LogOut(long authKeyId)
    {
        var futureAuthToken = _random.GetRandomBytes(32);
        var info = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if(info == null)
        {
            return null;
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
        return new LoggedOutDTO()
        {
            FutureAuthToken = futureAuthToken
        };
    }

    public Task<AuthorizationDTO> RecoverPassword(string code, PasswordInputSettingsDTO newSettings)
    {
        throw new NotImplementedException();
    }

    public Task<string> RequestPasswordRecovery()
    {
        throw new NotImplementedException();
    }

    public async Task<SentCodeDTO> ResendCode(string phoneNumber, string phoneCodeHash)
    {
        var phoneCode = _unitOfWork.PhoneCodeRepository.GetPhoneCode(phoneNumber, phoneCodeHash);
        if (phoneCode != null)
        {
            Console.WriteLine("auth.sentCode=>" + phoneCode);
            _unitOfWork.PhoneCodeRepository.PutPhoneCode(phoneNumber, phoneCodeHash, phoneCode,
                new TimeSpan(0, 0, PhoneCodeTimeout * 2));
            await _unitOfWork.SaveAsync();
            return new SentCodeDTO()
            {
                CodeType = SentCodeType.Sms,
                CodeLength = 5,
                Timeout = PhoneCodeTimeout,
                PhoneCodeHash = phoneCodeHash
            };
        }
        return null;//this is tested
    }

    public async Task<bool> ResetAuthorizations(long authKeyId)
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
            return true;
        }
        return false;
    }

    public async ValueTask<TLBytes> SendCode(TLBytes q)
    {
#if DEBUG
        var code = 12345;
#else
		var code = _random.GetNext(10000, 99999);
#endif
        _log.Information("auth.sentCode=>" + code);
        var codeBytes = BitConverter.GetBytes(code);
        var hash = codeBytes.GetXxHash64(1071).ToString("x");
        var (phoneNumber, apiId, apiHash) = GetSendCodeParameters(q);
        _unitOfWork.PhoneCodeRepository.PutPhoneCode(phoneNumber, hash, code.ToString(),
            new TimeSpan(0, 0, PhoneCodeTimeout*2));
        await _unitOfWork.SaveAsync();
        return GenerateSentCode(Encoding.UTF8.GetBytes(hash));
    }
    private static ValueTuple<string, int, string> GetSendCodeParameters(TLBytes q)
    {
        var sendCode = new SendCode(q.AsSpan());
        var phoneNumber = Encoding.UTF8.GetString(sendCode.PhoneNumber);
        var apiHash = Encoding.UTF8.GetString(sendCode.ApiHash);
        return (phoneNumber, sendCode.ApiId, apiHash);
    }
    private static TLBytes GenerateSentCode(ReadOnlySpan<byte> phoneCodeHash)
    {
        return ((TLBytes)SentCode.Builder()
            .Type(new CodeTypeSms().ToReadOnlySpan())
            .Timeout(PhoneCodeTimeout)
            .PhoneCodeHash(phoneCodeHash)
            .Build().TLBytes!);
    }

    public async Task<AuthorizationDTO> SignIn(long authKeyId, string phoneNumber, string phoneCodeHash, string phoneCode)
    {
        _log.Debug($"*** Sign In for authKey with Id: {authKeyId} ***");
        var code = _unitOfWork.PhoneCodeRepository.GetPhoneCode(phoneNumber, phoneCodeHash);
        if (code != phoneCode)
        {
            return new AuthorizationDTO()
            {
                AuthorizationType = AuthorizationType.PhoneCodeInvalid,
            };
        }
        _unitOfWork.SignInRepository.PutSignIn(authKeyId, phoneNumber, phoneCodeHash);
        await _unitOfWork.SaveAsync();
        var userId = _unitOfWork.UserRepository.GetUserId(phoneNumber);
        if(userId == null)
        {
            return new AuthorizationDTO()
            {
                AuthorizationType = AuthorizationType.SignUpRequired,
            };
        }
        var user = _users.GetUser((long)userId);
        if(user == null)
        {
            return new AuthorizationDTO()
            {
                AuthorizationType = AuthorizationType.SignUpRequired,
            };
        }
        user.Self = true;
        user.Min = true;
        user.ApplyMinPhoto = true;
        var authKeyDetails = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        _unitOfWork.AuthorizationRepository.PutAuthorization(new AuthInfoDTO()
        {
            AuthKeyId = authKeyId,
            Phone = phoneNumber,
            UserId = user.Id,
            ApiLayer = authKeyDetails?.ApiLayer ?? -1,
            LoggedIn = true
        });
        await _unitOfWork.SaveAsync();
        return new AuthorizationDTO()
        {
            AuthorizationType = AuthorizationType.Authorization,
            User = user,
        };
    }

    public async Task<AuthorizationDTO> SignUp(long authKeyId, string phoneNumber,
        string phoneCodeHash, string firstName, string lastName)
    {
        _log.Debug($"*** Sign Up for authKey with Id: {authKeyId} ***");
        long userId = await _userIdCnt.IncrementAndGet();
        if(userId == 0)
        {
            userId = await _userIdCnt.IncrementAndGet();
        }
        var phoneCode = _unitOfWork.PhoneCodeRepository.GetPhoneCode(phoneNumber, phoneCodeHash);
        var signedInAuthKeyId = _unitOfWork.SignInRepository.GetSignIn(phoneNumber, phoneCodeHash);
        if(phoneCode == null || signedInAuthKeyId != authKeyId)
        {
            return new AuthorizationDTO()
            {
                AuthorizationType = AuthorizationType.PhoneCodeInvalid,
            };
        } 
        var codeBytes = BitConverter.GetBytes(int.Parse(phoneCode));
        var hash = codeBytes.GetXxHash64(1071).ToString("x");
        if (phoneCodeHash != hash)
        {
            return new AuthorizationDTO()
            {
                AuthorizationType = AuthorizationType.PhoneCodeInvalid,
            };
        }
        var user = new UserDTO()
        {
            Id = userId,
            Phone = phoneNumber,
            FirstName = firstName,
            LastName = lastName,
            AccessHash = _random.NextLong(),
            Photo = new UserProfilePhotoDTO()
            {
                Empty = true
            }
        };
        _unitOfWork.UserRepository.PutUser(user);
        await _search.IndexUser(new Data.Search.UserSearchModel(user.Id, user.Username, 
            user.FirstName, user.LastName, user.Phone));
        var authKeyDetails = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        _unitOfWork.AuthorizationRepository.PutAuthorization(new AuthInfoDTO()
        {
            AuthKeyId = authKeyId,
            Phone = phoneNumber,
            UserId = user.Id,
            ApiLayer = authKeyDetails?.ApiLayer ?? -1,
            LoggedIn = true
        });
        await _unitOfWork.SaveAsync();
        return new AuthorizationDTO()
        {
            AuthorizationType = AuthorizationType.Authorization,
            User = new UserDTO()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                Status = UserStatusDTO.Empty,
                Self = true,
                Photo = new UserProfilePhotoDTO()
                {
                    Empty = true
                }
            }
        };
    }

    public async Task<bool> SaveAppInfo(AppInfoDTO info)
    {
        _unitOfWork.AppInfoRepository.PutAppInfo(info);
        _log.Debug($"=== Save App Info for authKey with Id: {info.AuthKeyId}");
        return await _unitOfWork.SaveAsync();
    }

    public async Task<AppInfoDTO?> GetAppInfo(long authKeyId)
    {
        _log.Debug($"=== Get App Info for authKey with Id: {authKeyId}");
        return _unitOfWork.AppInfoRepository.GetAppInfo(authKeyId);
    }
}

