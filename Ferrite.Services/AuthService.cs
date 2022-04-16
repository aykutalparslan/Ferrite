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

    private const int PhoneCodeTimeout = 60;//seconds

    public AuthService(IRandomGenerator random, IDistributedStore cache, IPersistentStore store)
    {
        _random = random;
        _cache = cache;
        _store = store;
    }

    public Task<Authorization> AcceptLoginToken(byte[] token)
    {
        throw new NotImplementedException();
    }

    public Task<bool> BindTempAuthKey(long permAuthKeyId, long nonce, int expiresAt, byte[] encryptedMessage)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CancelCode(string phoneNumber, string phoneCodeHash)
    {
        throw new NotImplementedException();
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

    public Task<bool> IsAuthorized(long authKeyId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> LogOut(long authKeyId, out byte[] futureAuthToken)
    {
        throw new NotImplementedException();
    }

    public Task<Authorization> RecoverPassword(string code, PasswordInputSettings newSettings)
    {
        throw new NotImplementedException();
    }

    public Task<string> RequestPasswordRecovery()
    {
        throw new NotImplementedException();
    }

    public Task<SentCode> ResendCode(string phoneNumber, string phoneCodeHash)
    {
        throw new NotImplementedException();
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

    public Task<Authorization> SignIn(long authKeyId, string phoneNumber, string phoneCodeHash, string phoneCode)
    {
        throw new NotImplementedException();
    }

    public Task<Authorization> SignUp(long authKeyId, string phoneNumber, string phoneCodeHash, string firstName, string lastName)
    {
        throw new NotImplementedException();
    }
}

