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
using Ferrite.TL.slim;
using Ferrite.TL.slim.baseLayer.auth;
using Ferrite.TL.slim.baseLayer.dto;

namespace Ferrite.Services;

public interface IAuthService
{
    public ValueTask<bool> SaveAppInfo(TLAppInfo info);
    public ValueTask<TLAppInfo?> GetAppInfo(long authKeyId);
    public ValueTask<bool> IsAuthorized(long authKeyId);
    public ValueTask<TLSentCode> SendCode(TLBytes q);
    public ValueTask<TLAuthorization> SignUp(long authKeyId, TLBytes q);
    public ValueTask<TLAuthorization> SignIn(long authKeyId, TLBytes q);
    public ValueTask<TLLoggedOut> LogOut(long authKeyId);
    public ValueTask<TLBool> ResetAuthorizations(long authKeyId);
    public ValueTask<TLExportedAuthorization> ExportAuthorization(long authKeyId, int currentDc, TLBytes q);
    public ValueTask<TLAuthorization> ImportAuthorization(long authKeyId, TLBytes q);
    public ValueTask<TLBool> BindTempAuthKey(long sessionId, TLBytes q);
    public ValueTask<TLSentCode> ResendCode(TLBytes q);
    public ValueTask<TLBool> CancelCode(TLBytes q);
    public ValueTask<TLBool> DropTempAuthKeys(long authKeyId, ICollection<long> exceptAuthKeys);
    public ValueTask<TLLoginToken> ExportLoginToken(long authKeyId, long sessionId, TLBytes q);
}

