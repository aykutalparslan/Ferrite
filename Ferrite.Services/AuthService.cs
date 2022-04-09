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
using Ferrite.Services.Account;
using Ferrite.Services.Auth;

namespace Ferrite.Services;

public class AuthService : IAuthService
{
    private static ReadOnlySpan<int> UnauthorizedMethods => new int[]
    {
        -1502141361, 1056025023, 1418342645, -779399914, -2131827673,
        -1126886015, -1518699091, -990308245, 531836966, 1378703997,
        1375900482, -219008246, -269862909, -845657435, 1120311183,
        1784243458
    };

    public Authorization AcceptLoginToken(byte[] token)
    {
        throw new NotImplementedException();
    }

    public bool BindTempAuthKey(long permAuthKeyId, long nonce, int expiresAt, byte[] encryptedMessage)
    {
        throw new NotImplementedException();
    }

    public bool CancelCode(string phoneNumber, string phoneCodeHash)
    {
        throw new NotImplementedException();
    }

    public Authorization CheckPassword(bool empty, long srpId, byte[] A, byte[] M1)
    {
        throw new NotImplementedException();
    }

    public bool CheckRecoveryPassword(string code)
    {
        throw new NotImplementedException();
    }

    public bool DropTempAuthKeys(ICollection<long> exceptAuthKeys)
    {
        throw new NotImplementedException();
    }

    public (long id, byte[] bytes) ExportAuthorization(int dcId)
    {
        throw new NotImplementedException();
    }

    public LoginToken ExportLoginToken(int apiId, string apiHash, ICollection<long> exceptIds)
    {
        throw new NotImplementedException();
    }

    public Authorization ImportAuthorization(long id, byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public Authorization ImportBotAuthorization(int apiId, string apiHash, string botAuthToken)
    {
        throw new NotImplementedException();
    }

    public LoginToken ImportLoginToken(byte[] token)
    {
        throw new NotImplementedException();
    }

    public bool LogOut(out byte[] futureAuthToken)
    {
        throw new NotImplementedException();
    }

    public Authorization RecoverPassword(string code, PasswordInputSettings newSettings)
    {
        throw new NotImplementedException();
    }

    public string RequestPasswordRecovery()
    {
        throw new NotImplementedException();
    }

    public SentCode ResendCode(string phoneNumber, string phoneCodeHash)
    {
        throw new NotImplementedException();
    }

    public bool ResetAuthorizations()
    {
        throw new NotImplementedException();
    }

    public SentCode SendCode(string phoneNumber, int apiId, string apiHash, CodeSettings settings)
    {
        return new SentCode()
        {
            CodeType = SentCodeType.Sms,
            CodeLength = 5,
            Timeout = 60,
            PhoneCodeHash = "12345"
        };
    }

    public Authorization SignIn(string phoneNumber, string phoneCodeHash, string phoneCode)
    {
        throw new NotImplementedException();
    }

    public Authorization SignUp(string phoneNumber, string phoneCodeHash, string firstName, string lastName)
    {
        throw new NotImplementedException();
    }
}

