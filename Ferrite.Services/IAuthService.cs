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
using Ferrite.Data.Auth;
using Ferrite.Data.Account;

namespace Ferrite.Services;

public interface IAuthService
{
    public Task<bool> IsAuthorized(long authKeyId);
    /// <summary>
	/// Send the verification code for login
	/// </summary>
	/// <param name="phoneNumber">Phone number in international format</param>
	/// <param name="apiId">Application identifier</param>
	/// <param name="apiHash">Application secret hash</param>
	/// <param name="settings">Settings for the code type to send</param>
	/// <returns>The method returns an auth.SentCode object with information on the message sent.</returns>
    public Task<SentCode> SendCode(string phoneNumber, int apiId, string apiHash, CodeSettings settings);
    public Task<Authorization> SignUp(long authKeyId, string phoneNumber, string phoneCodeHash, string firstName, string lastName);
    public Task<Authorization> SignIn(long authKeyId, string phoneNumber, string phoneCodeHash, string phoneCode);
    public Task<LoggedOut?> LogOut(long authKeyId);
    public Task<bool> ResetAuthorizations();
    public Task<ExportedAuthorization> ExportAuthorization(long authKeyId, int dcId);
    public Task<Authorization> ImportAuthorization(long id, byte[] bytes);
    public Task<bool> BindTempAuthKey(long permAuthKeyId, long nonce, int expiresAt, byte[] encryptedMessage);
    public Task<Authorization> ImportBotAuthorization(int apiId, string apiHash, string botAuthToken);
    public Task<Authorization> CheckPassword(bool empty, long srpId, byte[] A, byte[] M1);
    public Task<string> RequestPasswordRecovery();
    public Task<Authorization> RecoverPassword(string code, PasswordInputSettings newSettings);
    public Task<SentCode> ResendCode(string phoneNumber, string phoneCodeHash);
    public Task<bool> CancelCode(string phoneNumber, string phoneCodeHash);
    public Task<bool> DropTempAuthKeys(ICollection<long> exceptAuthKeys);
    public Task<LoginToken> ExportLoginToken(int apiId, string apiHash, ICollection<long> exceptIds);
    public Task<LoginToken> ImportLoginToken(byte[] token);
    public Task<Authorization> AcceptLoginToken(byte[] token);
    public Task<bool> CheckRecoveryPassword(string code);
}

