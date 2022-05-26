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

namespace Ferrite.Services;

public interface IAccountService
{
    public Task<bool> RegisterDevice(DeviceInfo deviceInfo);
    public Task<bool> UnregisterDevice(long authKeyId, string token, ICollection<long> otherUserIds);
    public Task<bool> UpdateNotifySettings(long authKeyId, InputNotifyPeer peer, InputPeerNotifySettings settings);
    public Task<InputPeerNotifySettings> GetNotifySettings(long authKeyId, InputNotifyPeer peer);
    public Task<bool> ResetNotifySettings(long authKeyId);
    public Task<User?> UpdateProfile(long authKeyId, string? firstName, string? lastName, string? about);
    public Task<bool> UpdateStatus(long authKeyId, bool status);
    public Task<bool> ReportPeer(long authKeyId, InputPeer peer, ReportReason reason);
    public Task<bool> CheckUsername(string username);
    public Task<User?> UpdateUsername(long authKeyId, string username);
    public Task<PrivacyRules?> SetPrivacy(long authKeyId, InputPrivacyKey key, ICollection<PrivacyRule> rules);
    public Task<PrivacyRules?> GetPrivacy(long authKeyId, InputPrivacyKey key);
    public Task<bool> DeleteAccount(long authKeyId);
    public Task<bool> SetAccountTTL(long authKeyId, int accountDaysTTL);
    public Task<int> GetAccountTTL(long authKeyId);
    public Task<ServiceResult<SentCode>> SendChangePhoneCode(long authKeyId, string phoneNumber, CodeSettings settings);
    public Task<ServiceResult<User>> ChangePhone(long authKeyId, string phoneNumber, string phoneCodeHash, string phoneCode);
    public Task<bool> UpdateDeviceLocked(long authKeyId, int period);
}