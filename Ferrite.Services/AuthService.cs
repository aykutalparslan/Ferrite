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
using System.Threading.Tasks.Sources;
using DotNext.IO;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Data.Account;
using Ferrite.Data.Auth;
using Ferrite.Data.Repositories;
using xxHash;

namespace Ferrite.Services;

public class AuthService : IAuthService
{
    private readonly IRandomGenerator _random;
    private readonly IDistributedCache _cache;
    private readonly IPersistentStore _store;
    private readonly ISearchEngine _search;
    private readonly IAtomicCounter _userIdCnt;
    private readonly IUnitOfWork _unitOfWork;

    private const int PhoneCodeTimeout = 60;//seconds

    public AuthService(IRandomGenerator random, IDistributedCache cache, IPersistentStore store, ISearchEngine search,
        IUnitOfWork unitOfWork)
    {
        _random = random;
        _cache = cache;
        _store = store;
        _search = search;
        _userIdCnt = _cache.GetCounter("counter_user_id");
        _unitOfWork = unitOfWork;
    }

    public async Task<AppInfoDTO?> AcceptLoginToken(long authKeyId, byte[] token)
    {
        var t = await _cache.GetLoginTokenAsync(token);
        var auth = await _store.GetAuthorizationAsync(authKeyId);
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
            _ = _cache.PutLoginTokenAsync(login, new TimeSpan(0, 0, 60));
            await _store.SaveAuthorizationAsync(new AuthInfoDTO()
            {
                AuthKeyId = t.AuthKeyId,
                Phone = auth.Phone,
                UserId = auth.UserId,
                ApiLayer = -1,
                LoggedIn = true
            });
            var app = await _store.GetAppInfoAsync(t.AuthKeyId);
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
        return await _cache.DeletePhoneCodeAsync(phoneNumber, phoneCodeHash);
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
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        var data = _random.GetRandomBytes(128);
        //TODO: get current dc id
        await _store.SaveExportedAuthorizationAsync(auth, 1, dcId, data);
        return new ExportedAuthorizationDTO()
        {
            Id = auth.UserId,
            Bytes = data
        };
    }

    public async Task<LoginTokenDTO> ExportLoginToken(long authKeyId, long sessionId, int apiId, string apiHash, ICollection<long> exceptIds)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        if (auth!= null && auth.LoggedIn &&
            await _store.GetUserAsync(auth.UserId) is UserDTO user)
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
                        Photo = new UserProfilePhotoDTO()
                        {
                            Empty = true
                        }
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
        await _cache.PutLoginTokenAsync(login, new TimeSpan(0, 0, 30));
        return new LoginTokenDTO()
        {
            LoginTokenType = LoginTokenType.Token,
            Token = token,
            Expires = 30
        };
    }

    public async Task<AuthorizationDTO> ImportAuthorization(long userId, long authKeyId, byte[] bytes)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        var exported = await _store.GetExportedAuthorizationAsync(userId, bytes);
        
        if (auth != null && exported != null &&
            auth.Phone == exported.Phone && bytes.SequenceEqual(exported.Data))
        {
            var user = await _store.GetUserAsync(auth.UserId);
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
                    Photo = new UserProfilePhotoDTO()
                    {
                        Empty = true
                    }
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
        var authKeyDetails = await _store.GetAuthorizationAsync(authKeyId);
        return authKeyDetails?.LoggedIn ?? false;
    }

    public async Task<LoggedOutDTO?> LogOut(long authKeyId)
    {
        var futureAuthToken = _random.GetRandomBytes(32);
        var info = await _store.GetAuthorizationAsync(authKeyId);
        if(info == null)
        {
            return null;
        }
        await _store.SaveAuthorizationAsync(info with
        {
            FutureAuthToken = futureAuthToken,
            Phone = "",
            UserId = 0,
            LoggedIn = false
        });
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
        var phoneCode = await _cache.GetPhoneCodeAsync(phoneNumber, phoneCodeHash);
        if (phoneCode != null)
        {
            Console.WriteLine("auth.sentCode=>" + phoneCode);
            await _cache.PutPhoneCodeAsync(phoneNumber, phoneCodeHash, phoneCode,
                new TimeSpan(0, 0, PhoneCodeTimeout * 2));

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
        var currentAuth = await _store.GetAuthorizationAsync(authKeyId);
        if (currentAuth != null)
        {
            var authorizations = await _store.GetAuthorizationsAsync(currentAuth.Phone);
            foreach (var auth in authorizations)
            {
                if(auth.AuthKeyId != authKeyId)
                {
                    await _store.DeleteAuthorizationAsync(authKeyId);
                }
            }
            return true;
        }
        return false;
    }

    public async Task<SentCodeDTO> SendCode(string phoneNumber, int apiId, string apiHash, CodeSettingsDTO settings)
    {
        var code = _random.GetNext(10000, 99999);
        Console.WriteLine("auth.sentCode=>" + code.ToString());
        var codeBytes = BitConverter.GetBytes(code);
        var hash = codeBytes.GetXxHash64(1071).ToString("x");
        await _cache.PutPhoneCodeAsync(phoneNumber, hash, code.ToString(),
            new TimeSpan(0, 0, PhoneCodeTimeout*2));
        
        return new SentCodeDTO()
        {
            CodeType = SentCodeType.Sms,
            CodeLength = 5,
            Timeout = PhoneCodeTimeout,
            PhoneCodeHash = hash
        };
    }

    public async Task<AuthorizationDTO> SignIn(long authKeyId, string phoneNumber, string phoneCodeHash, string phoneCode)
    {
        var code = await _cache.GetPhoneCodeAsync(phoneNumber, phoneCodeHash);
        if (code != phoneCode)
        {
            return new AuthorizationDTO()
            {
                AuthorizationType = AuthorizationType.PhoneCodeInvalid,
            };
        }
        await _cache.PutSignInAsync(authKeyId, phoneNumber, phoneCodeHash);
        var user = await _store.GetUserAsync(phoneNumber);
        if(user == null)
        {
            return new AuthorizationDTO()
            {
                AuthorizationType = AuthorizationType.SignUpRequired,
            };
        }
        var authKeyDetails = await _store.GetAuthorizationAsync(authKeyId);
        await _store.SaveAuthorizationAsync(new AuthInfoDTO()
        {
            AuthKeyId = authKeyId,
            Phone = phoneNumber,
            UserId = user.Id,
            ApiLayer = authKeyDetails == null ? -1 : authKeyDetails.ApiLayer,
            LoggedIn = true
        });
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

    public async Task<AuthorizationDTO> SignUp(long authKeyId, string phoneNumber,
        string phoneCodeHash, string firstName, string lastName)
    {
        long userId = await _userIdCnt.IncrementAndGet();
        if(userId == 0)
        {
            userId = await _userIdCnt.IncrementAndGet();
        }
        var phoneCode = await _cache.GetPhoneCodeAsync(phoneNumber, phoneCodeHash);
        var signedInAuthKeyId = await _cache.GetSignInAsync(phoneNumber, phoneCodeHash);
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
        await _store.SaveUserAsync(user);
        await _search.IndexUser(new Data.Search.UserSearchModel(user.Id, user.Username, 
            user.FirstName, user.LastName, user.Phone));
        var authKeyDetails = await _store.GetAuthorizationAsync(authKeyId);
        await _store.SaveAuthorizationAsync(new AuthInfoDTO()
        {
            AuthKeyId = authKeyId,
            Phone = phoneNumber,
            UserId = user.Id,
            ApiLayer = authKeyDetails == null ? -1 : authKeyDetails.ApiLayer,
            LoggedIn = true
        });
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
        return await _store.SaveAppInfoAsync(info);
    }

    public async Task<AppInfoDTO?> GetAppInfo(long authKeyId)
    {
        return await _store.GetAppInfoAsync(authKeyId);
    }
}

