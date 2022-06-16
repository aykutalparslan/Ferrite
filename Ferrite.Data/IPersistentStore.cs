/*
 *   Project Ferrite is an Implementation of the Telegram Server API
 *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Affero General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Affero General Public License for more details.
 *
 *   You should have received a copy of the GNU Affero General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using Ferrite.Data.Account;

namespace Ferrite.Data;
public interface IPersistentStore
{
    public Task<bool> SaveAuthKeyAsync(long authKeyId, byte[] authKey);
    public Task<byte[]?> GetAuthKeyAsync(long authKeyId);
    public byte[]? GetAuthKey(long authKeyId);
    public Task<bool> SaveExportedAuthorizationAsync(AuthInfo info, int previousDc, int nextDc, byte[] data);
    public Task<ExportedAuthInfo?> GetExportedAuthorizationAsync(long user_id, long auth_key_id);
    public Task<bool> SaveAuthorizationAsync(AuthInfo info);
    public Task<AuthInfo?> GetAuthorizationAsync(long authKeyId);
    public Task<ICollection<AuthInfo>> GetAuthorizationsAsync(string phone);
    public Task<bool> DeleteAuthorizationAsync(long authKeyId);
    public Task<bool> DeleteAuthKeyAsync(long authKeyId);
    public Task<bool> SaveServerSaltAsync(long authKeyId, long serverSalt, long validSince, int TTL);
    public Task<ICollection<ServerSalt>> GetServerSaltsAsync(long authKeyId, int count);
    public Task<bool> SaveUserAsync(User user);
    public Task<bool> UpdateUserAsync(User user);
    public Task<bool> UpdateUsernameAsync(long userId, string username);
    /// <summary>
    /// Must be called only after phone fields in the other tables are updated.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="phone"></param>
    /// <returns></returns>
    public Task<bool> UpdateUserPhoneAsync(long userId, string phone);
    public Task<User?> GetUserAsync(long userId);
    public Task<User?> GetUserAsync(string phone);
    public Task<long> GetUserIdAsync(string phone);
    public Task<User?> GetUserByUsernameAsync(string username);
    /// <summary>
    /// Deletes a user. This method does not check if the related user data is deleted
    /// therefore must be called only after all user related data has been deleted.
    /// Otherwise it will leave the database in an inconsistent state.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public Task<bool> DeleteUserAsync(User user);
    public Task<bool> SaveAppInfoAsync(AppInfo appInfo);
    public Task<AppInfo?> GetAppInfoAsync(long authKeyId);
    public Task<long?> GetAuthKeyIdByAppHashAsync(long hash);
    public Task<bool> SaveDeviceInfoAsync(DeviceInfo deviceInfo);
    public Task<DeviceInfo?> GetDeviceInfoAsync(long authKeyId);
    public Task<bool> DeleteDeviceInfoAsync(long authKeyId, string token, ICollection<long> otherUserIds);
    public Task<bool> SaveNotifySettingsAsync(long authKeyId, InputNotifyPeer peer, PeerNotifySettings settings);
    public Task<bool> SavePeerReportReasonAsync(long reportedByUser, InputPeer peer, ReportReason reason);
    public Task<PeerNotifySettings?> GetNotifySettingsAsync(long authKeyId, InputNotifyPeer peer);
    public Task<bool> DeleteNotifySettingsAsync(long authKeyId);
    public Task<bool> SavePrivacyRulesAsync(long userId, InputPrivacyKey key, ICollection<PrivacyRule> rules);
    public Task<bool> DeletePrivacyRulesAsync(long userId);
    public Task<ICollection<PrivacyRule>> GetPrivacyRulesAsync(long userId, InputPrivacyKey key);
    public Task<bool> SaveChatAsync(Chat chat);
    public Task<Chat?> GetChatAsync(long chatId);
    public Task<bool> UpdateAccountTTLAsync(long userId, int accountDaysTTL);
    public Task<int> GetAccountTTLAsync(long userId);
    public Task<ImportedContact?> SaveContactAsync(long userId, InputContact contact);
    public Task<bool> DeleteContactAsync(long userId, long contactUserId);
    public Task<ICollection<SavedContact>> GetSavedContactsAsync(long userId);
    public Task<ICollection<Contact>> GetContactsAsync(long userId);
    public Task<bool> SaveBlockedUserAsync(long userId, long peerId, PeerType peerType);
    public Task<bool> DeleteBlockedUserAsync(long userId, long peerId, PeerType peerType);
    public Task<ICollection<PeerBlocked>> GetBlockedPeersAsync(long userId);
}

