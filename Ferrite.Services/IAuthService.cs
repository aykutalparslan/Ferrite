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
using Ferrite.Data;
using Ferrite.TL.slim;

namespace Ferrite.Services;

public interface IAuthService
{
    public ValueTask<bool> SaveAppInfo(TLBytes info);
    public ValueTask<TLBytes?> GetAppInfo(long authKeyId);
    public ValueTask<bool> IsAuthorized(long authKeyId);
    public ValueTask<TLBytes> SendCode(TLBytes q);
    public ValueTask<TLBytes> SignUp(long authKeyId, TLBytes q);
    public ValueTask<TLBytes> SignIn(long authKeyId, TLBytes q);
    public ValueTask<TLBytes> LogOut(long authKeyId);
    public ValueTask<TLBytes> ResetAuthorizations(long authKeyId);
    public ValueTask<TLBytes> ExportAuthorization(long authKeyId, int currentDc, TLBytes q);
    public ValueTask<TLBytes> ImportAuthorization(long authKeyId, TLBytes q);
    public ValueTask<TLBytes> BindTempAuthKey(long sessionId, TLBytes q);
    public Task<AuthorizationDTO> ImportBotAuthorization(int apiId, string apiHash, string botAuthToken);
    public Task<AuthorizationDTO> CheckPassword(bool empty, long srpId, byte[] A, byte[] M1);
    public Task<string> RequestPasswordRecovery();
    public Task<AuthorizationDTO> RecoverPassword(string code, PasswordInputSettingsDTO newSettings);
    public ValueTask<TLBytes> ResendCode(TLBytes q);
    public ValueTask<TLBytes> CancelCode(TLBytes q);
    public ValueTask<TLBytes> DropTempAuthKeys(long authKeyId, ICollection<long> exceptAuthKeys);
    public ValueTask<TLBytes> ExportLoginToken(long authKeyId, long sessionId, TLBytes q);
    public Task<LoginTokenDTO> ImportLoginToken(byte[] token);
    public Task<TLBytes?> AcceptLoginToken(long authKeyId, byte[] token);
    public Task<bool> CheckRecoveryPassword(string code);
}

