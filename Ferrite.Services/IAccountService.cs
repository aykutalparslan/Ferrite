//
//  Project Ferrite is an Implementation of the Telegram Server API
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

using Ferrite.Data;
using Ferrite.Data.Account;
using Ferrite.Data.Auth;
using Ferrite.TL.slim;

namespace Ferrite.Services;

public interface IAccountService
{
    public ValueTask<TLBytes> RegisterDevice(long authKeyId, TLBytes q);
    public ValueTask<TLBytes> RegisterDeviceL57(long authKeyId, TLBytes q);
    public ValueTask<TLBytes> UnregisterDevice(long authKeyId, TLBytes q);
    public ValueTask<TLBytes> UpdateNotifySettings(long authKeyId, TLBytes q);
    public ValueTask<TLBytes> GetNotifySettings(long authKeyId, TLBytes q);
    public ValueTask<TLBytes> ResetNotifySettings(long authKeyId);
    public ValueTask<TLBytes> UpdateProfile(long authKeyId, string? firstName, string? lastName, string? about);
    public ValueTask<TLBytes> UpdateStatus(long authKeyId, bool status);
    public ValueTask<TLBytes> ReportPeer(long authKeyId, TLBytes q);
    public ValueTask<TLBytes> CheckUsername(string username);
    public ValueTask<TLBytes> UpdateUsername(long authKeyId, string username);
    public ValueTask<TLBytes> SetPrivacy(long authKeyId, TLBytes q);
    public ValueTask<TLBytes> GetPrivacy(long authKeyId, TLBytes q);
    public ValueTask<TLBytes> DeleteAccount(long authKeyId);
    public ValueTask<TLBytes> SetAccountTTL(long authKeyId, int accountDaysTTL);
    public ValueTask<TLBytes> GetAccountTTL(long authKeyId);
    public ValueTask<TLBytes> SendChangePhoneCode(long authKeyId, TLBytes q);
    public ValueTask<TLBytes> ChangePhone(long authKeyId, TLBytes q);
    public ValueTask<TLBytes> UpdateDeviceLocked(long authKeyId, int period);
    public ValueTask<TLBytes> GetAuthorizations(long authKeyId);
    public ValueTask<TLBytes> ResetAuthorization(long authKeyId, long hash);
    public ValueTask<TLBytes> SetContactSignUpNotification(long authKeyId, bool silent);
    public Task<bool> GetContactSignUpNotification(long authKeyId);
    public Task<ServiceResult<bool>> ChangeAuthorizationSettings(long authKeyId, long hash, 
        bool encryptedRequestsDisabled,bool callRequestsDisabled);
}