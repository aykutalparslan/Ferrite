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
    public Task<bool> ReportPeer(long authKeyId, InputPeerDTO peer, ReportReason reason);
    public Task<bool> CheckUsername(string username);
    public Task<UserDTO?> UpdateUsername(long authKeyId, string username);
    public Task<PrivacyRulesDTO?> SetPrivacy(long authKeyId, InputPrivacyKey key, ICollection<PrivacyRuleDTO> rules);
    public Task<PrivacyRulesDTO?> GetPrivacy(long authKeyId, InputPrivacyKey key);
    public Task<bool> DeleteAccount(long authKeyId);
    public Task<bool> SetAccountTTL(long authKeyId, int accountDaysTTL);
    public Task<int> GetAccountTTL(long authKeyId);
    public Task<ServiceResult<SentCodeDTO>> SendChangePhoneCode(long authKeyId, string phoneNumber, CodeSettingsDTO settings);
    public Task<ServiceResult<UserDTO>> ChangePhone(long authKeyId, string phoneNumber, string phoneCodeHash, string phoneCode);
    public Task<bool> UpdateDeviceLocked(long authKeyId, int period);
    public Task<AuthorizationsDTO> GetAuthorizations(long authKeyId);
    public Task<ServiceResult<bool>> ResetAuthorization(long authKeyId, long hash);
    public Task<bool> SetContactSignUpNotification(long authKeyId, bool silent);
    public Task<bool> GetContactSignUpNotification(long authKeyId);
    public Task<ServiceResult<bool>> ChangeAuthorizationSettings(long authKeyId, long hash, 
        bool encryptedRequestsDisabled,bool callRequestsDisabled);
}