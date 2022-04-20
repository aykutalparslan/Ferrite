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
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Data.Account;
using Ferrite.Data.Auth;
using xxHash;

namespace Ferrite.Services;

public class AuthService : IAuthService
{
    private readonly IRandomGenerator _random;
    private readonly IDistributedStore _cache;
    private readonly IPersistentStore _store;
    private readonly IAtomicCounter _userIdCnt;

    private const int PhoneCodeTimeout = 60;//seconds

    public AuthService(IRandomGenerator random, IDistributedStore cache, IPersistentStore store)
    {
        _random = random;
        _cache = cache;
        _store = store;
        _userIdCnt = _cache.GetCounter("counter_user_id");
    }

    public Task<Authorization> AcceptLoginToken(byte[] token)
    {
        throw new NotImplementedException();
    }

    public Task<bool> BindTempAuthKey(long permAuthKeyId, long nonce, int expiresAt, byte[] encryptedMessage)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> CancelCode(string phoneNumber, string phoneCodeHash)
    {
        return await _cache.DeletePhoneCodeAsync(phoneNumber, phoneCodeHash);
    }

    public Task<Authorization> CheckPassword(bool empty, long srpId, byte[] A, byte[] M1)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CheckRecoveryPassword(string code)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DropTempAuthKeys(ICollection<long> exceptAuthKeys)
    {
        throw new NotImplementedException();
    }

    public Task<ExportedAuthorization> ExportAuthorization(long authKeyId, int dcId)
    {
        throw new NotImplementedException();
    }

    public Task<LoginToken> ExportLoginToken(int apiId, string apiHash, ICollection<long> exceptIds)
    {
        throw new NotImplementedException();
    }

    public Task<Authorization> ImportAuthorization(long id, byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public Task<Authorization> ImportBotAuthorization(int apiId, string apiHash, string botAuthToken)
    {
        throw new NotImplementedException();
    }

    public Task<LoginToken> ImportLoginToken(byte[] token)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> IsAuthorized(long authKeyId)
    {
        var authKeyDetails = await _store.GetAuthorizationAsync(authKeyId);
        return authKeyDetails == null ? false : authKeyDetails.LoggedIn;
    }

    public async Task<LoggedOut?> LogOut(long authKeyId)
    {
        var futureAuthToken = _random.GetRandomBytes(32);
        var info = await _store.GetAuthorizationAsync(authKeyId);
        if(info == null)
        {
            return null;
        }
        await _store.SaveAuthorizationAsync(new AuthInfo()
        {
            AuthKeyId = info.AuthKeyId,
            ApiLayer = info.ApiLayer,
            FutureAuthToken = futureAuthToken,
            Phone = "",
            UserId = 0,
            LoggedIn = false
        });
        return new LoggedOut()
        {
            FutureAuthToken = futureAuthToken
        };
    }

    public Task<Authorization> RecoverPassword(string code, PasswordInputSettings newSettings)
    {
        throw new NotImplementedException();
    }

    public Task<string> RequestPasswordRecovery()
    {
        throw new NotImplementedException();
    }

    public async Task<SentCode> ResendCode(string phoneNumber, string phoneCodeHash)
    {
        var phoneCode = await _cache.GetPhoneCodeAsync(phoneNumber, phoneCodeHash);
        if (phoneCode != null)
        {
            Console.WriteLine("auth.sentCode=>" + phoneCode);
            await _cache.PutPhoneCodeAsync(phoneNumber, phoneCodeHash, phoneCode,
                new TimeSpan(0, 0, PhoneCodeTimeout * 2));

            return new SentCode()
            {
                CodeType = SentCodeType.Sms,
                CodeLength = 5,
                Timeout = PhoneCodeTimeout,
                PhoneCodeHash = phoneCodeHash
            };
        }
        return null;//this is tested
    }

    public Task<bool> ResetAuthorizations()
    {
        throw new NotImplementedException();
    }

    public async Task<SentCode> SendCode(string phoneNumber, int apiId, string apiHash, CodeSettings settings)
    {
        var code = _random.GetNext(10000, 99999);
        Console.WriteLine("auth.sentCode=>" + code.ToString());
        var codeBytes = BitConverter.GetBytes(code);
        var hash = codeBytes.GetXxHash64(1071).ToString("x");
        await _cache.PutPhoneCodeAsync(phoneNumber, hash, code.ToString(),
            new TimeSpan(0, 0, PhoneCodeTimeout*2));
        
        return new SentCode()
        {
            CodeType = SentCodeType.Sms,
            CodeLength = 5,
            Timeout = PhoneCodeTimeout,
            PhoneCodeHash = hash
        };
    }

    public async Task<Authorization> SignIn(long authKeyId, string phoneNumber, string phoneCodeHash, string phoneCode)
    {
        var user = await _store.GetUserAsync(phoneNumber);
        if(user == null)
        {
            return new Authorization()
            {
                AuthorizationType = AuthorizationType.SignUpRequired,
            };
        }
        var code = await _cache.GetPhoneCodeAsync(phoneNumber, phoneCodeHash);
        if (code != phoneCode)
        {
            return new Authorization()
            {
                AuthorizationType = AuthorizationType.PhoneCodeInvalid,
            };
        }
        var authKeyDetails = await _store.GetAuthorizationAsync(authKeyId);
        await _store.SaveAuthorizationAsync(new AuthInfo()
        {
            AuthKeyId = authKeyId,
            Phone = phoneNumber,
            UserId = user.Id,
            ApiLayer = authKeyDetails == null ? -1 : authKeyDetails.ApiLayer,
            LoggedIn = true
        });
        return new Authorization()
        {
            AuthorizationType = AuthorizationType.Authorization,
            User = new User()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                Status = UserStatus.Empty,
                Self = true,
                Photo = new UserProfilePhoto()
                {
                    Empty = true
                }
            }
        };
    }

    public async Task<Authorization> SignUp(long authKeyId, string phoneNumber,
        string phoneCodeHash, string firstName, string lastName)
    {
        long userId = await _userIdCnt.IncrementAndGet();
        if(userId == 0)
        {
            userId = await _userIdCnt.IncrementAndGet();
        }
        var phoneCode = await _cache.GetPhoneCodeAsync(phoneNumber, phoneCodeHash);
        if(phoneCode == null)
        {
            return new Authorization()
            {
                AuthorizationType = AuthorizationType.SignUpRequired,
            };
        }
        var user = new User()
        {
            Id = userId,
            Phone = phoneNumber,
            FirstName = firstName,
            LastName = lastName,
            AccessHash = _random.NextLong()
        };
        await _store.SaveUserAsync(user);
        var authKeyDetails = await _store.GetAuthorizationAsync(authKeyId);
        await _store.SaveAuthorizationAsync(new AuthInfo()
        {
            AuthKeyId = authKeyId,
            Phone = phoneNumber,
            UserId = user.Id,
            ApiLayer = authKeyDetails == null ? -1 : authKeyDetails.ApiLayer
        });
        return new Authorization()
        {
            AuthorizationType = AuthorizationType.Authorization,
            User = new User()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                Status = UserStatus.Empty,
                Self = true,
                Photo = new UserProfilePhoto()
                {
                    Empty = true
                }
            }
        };
    }
}

