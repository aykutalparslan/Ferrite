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

using Ferrite.TL.slim;
using Ferrite.TL.slim.baseLayer;
using Ferrite.TL.slim.baseLayer.account;
using Ferrite.TL.slim.baseLayer.auth;

namespace Ferrite.Services;

public interface IAccountService
{
    public ValueTask<TLBool> RegisterDevice(long authKeyId, TLBytes q);
    public ValueTask<TLBool> RegisterDeviceL57(long authKeyId, TLBytes q);
    public ValueTask<TLBool> UnregisterDevice(long authKeyId, TLBytes q);
    public ValueTask<TLBool> UpdateNotifySettings(long authKeyId, TLBytes q);
    public ValueTask<TLPeerNotifySettings> GetNotifySettings(long authKeyId, TLBytes q);
    public ValueTask<TLBool> ResetNotifySettings(long authKeyId);
    public ValueTask<TLUser> UpdateProfile(long authKeyId, string? firstName, string? lastName, string? about);
    public ValueTask<TLBool> UpdateStatus(long authKeyId, bool status);
    public ValueTask<TLBool> ReportPeer(long authKeyId, TLBytes q);
    public ValueTask<TLBool> CheckUsername(string username);
    public ValueTask<TLUser> UpdateUsername(long authKeyId, string username);
    public ValueTask<TLPrivacyRules> SetPrivacy(long authKeyId, TLBytes q);
    public ValueTask<TLPrivacyRules> GetPrivacy(long authKeyId, TLBytes q);
    public ValueTask<TLBool> DeleteAccount(long authKeyId);
    public ValueTask<TLBool> SetAccountTTL(long authKeyId, int accountDaysTTL);
    public ValueTask<TLAccountDaysTTL> GetAccountTTL(long authKeyId);
    public ValueTask<TLSentCode> SendChangePhoneCode(long authKeyId, TLBytes q);
    public ValueTask<TLUser> ChangePhone(long authKeyId, TLBytes q);
    public ValueTask<TLBool> UpdateDeviceLocked(long authKeyId, int period);
    public ValueTask<TLAuthorizations> GetAuthorizations(long authKeyId);
    public ValueTask<TLBool> ResetAuthorization(long authKeyId, long hash);
    public ValueTask<TLBool> SetContactSignUpNotification(long authKeyId, bool silent);
    public ValueTask<TLBool> GetContactSignUpNotification(long authKeyId);
    public ValueTask<TLBool> ChangeAuthorizationSettings(long authKeyId, long hash, 
        bool encryptedRequestsDisabled,bool callRequestsDisabled);
}