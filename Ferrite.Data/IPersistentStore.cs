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
    //public Task<bool> SaveAuthKeyAsync(long authKeyId, byte[] authKey);
    //public Task<byte[]?> GetAuthKeyAsync(long authKeyId);
    //public byte[]? GetAuthKey(long authKeyId);
    //public Task<bool> SaveExportedAuthorizationAsync(AuthInfoDTO info, int previousDc, int nextDc, byte[] data);
    //public Task<ExportedAuthInfoDTO?> GetExportedAuthorizationAsync(long user_id, byte[] data);
    //public Task<bool> SaveAuthorizationAsync(AuthInfoDTO info);
    //public Task<AuthInfoDTO?> GetAuthorizationAsync(long authKeyId);
    //public Task<ICollection<AuthInfoDTO>> GetAuthorizationsAsync(string phone);
    //public Task<bool> DeleteAuthorizationAsync(long authKeyId);
    //public Task<bool> DeleteAuthKeyAsync(long authKeyId);
    //public Task<bool> SaveServerSaltAsync(long authKeyId, long serverSalt, long validSince, int TTL);
    //public Task<ICollection<ServerSaltDTO>> GetServerSaltsAsync(long authKeyId, int count);
    //public Task<bool> SaveUserAsync(UserDTO user);
    //public Task<bool> UpdateUserAsync(UserDTO user);
    //public Task<bool> UpdateUsernameAsync(long userId, string username);
    /// <summary>
    /// Must be called only after phone fields in the other tables are updated.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="phone"></param>
    /// <returns></returns>
    //public Task<bool> UpdateUserPhoneAsync(long userId, string phone);
    //public Task<UserDTO?> GetUserAsync(long userId);
    //public Task<UserDTO?> GetUserAsync(string phone);
    //public Task<long> GetUserIdAsync(string phone);
    //public Task<UserDTO?> GetUserByUsernameAsync(string username);
    /// <summary>
    /// Deletes a user. This method does not check if the related user data is deleted
    /// therefore must be called only after all user related data has been deleted.
    /// Otherwise it will leave the database in an inconsistent state.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    //public Task<bool> DeleteUserAsync(UserDTO user);
    //public Task<bool> SaveAppInfoAsync(AppInfoDTO appInfo);
    //public Task<AppInfoDTO?> GetAppInfoAsync(long authKeyId);
    //public Task<long?> GetAuthKeyIdByAppHashAsync(long hash);
    //public Task<bool> SaveDeviceInfoAsync(DeviceInfoDTO deviceInfo);
    //public Task<DeviceInfoDTO?> GetDeviceInfoAsync(long authKeyId);
    //public Task<bool> DeleteDeviceInfoAsync(long authKeyId, string token, ICollection<long> otherUserIds);
    //public Task<bool> SaveNotifySettingsAsync(long authKeyId, InputNotifyPeerDTO peer, PeerNotifySettingsDTO settings);
    //public Task<bool> SavePeerReportReasonAsync(long reportedByUser, InputPeerDTO peer, ReportReason reason);
    //public Task<IReadOnlyCollection<PeerNotifySettingsDTO>> GetNotifySettingsAsync(long authKeyId, InputNotifyPeerDTO peer);
    //public Task<bool> DeleteNotifySettingsAsync(long authKeyId);
    //public Task<bool> SavePrivacyRulesAsync(long userId, InputPrivacyKey key, ICollection<PrivacyRuleDTO> rules);
    //public Task<bool> DeletePrivacyRulesAsync(long userId);
    //public Task<ICollection<PrivacyRuleDTO>> GetPrivacyRulesAsync(long userId, InputPrivacyKey key);
    public Task<bool> SaveChatAsync(ChatDTO chat);
    public Task<ChatDTO?> GetChatAsync(long chatId);
    //public Task<bool> UpdateAccountTTLAsync(long userId, int accountDaysTTL);
    //public Task<int> GetAccountTTLAsync(long userId);
    public Task<ImportedContactDTO?> SaveContactAsync(long userId, InputContactDTO contact);
    public Task<bool> DeleteContactAsync(long userId, long contactUserId);
    public Task<bool> DeleteContactsAsync(long userId);
    public Task<ICollection<SavedContactDTO>> GetSavedContactsAsync(long userId);
    public Task<ICollection<ContactDTO>> GetContactsAsync(long userId);
    public Task<bool> SaveBlockedUserAsync(long userId, long peerId, PeerType peerType);
    public Task<bool> DeleteBlockedPeerAsync(long userId, long peerId, PeerType peerType);
    public Task<ICollection<PeerBlocked>> GetBlockedPeersAsync(long userId);
    public Task<bool> SaveFileInfoAsync(UploadedFileInfoDTO uploadedFile);
    public Task<UploadedFileInfoDTO?> GetFileInfoAsync(long fileId);
    public Task<bool> SaveBigFileInfoAsync(UploadedFileInfoDTO uploadedFile);
    public Task<UploadedFileInfoDTO?> GetBigFileInfoAsync(long fileId);
    public Task<bool> SaveFilePartAsync(FilePartDTO part);
    public Task<IReadOnlyCollection<FilePartDTO>> GetFilePartsAsync(long fileId);
    public Task<bool> SaveBigFilePartAsync(FilePartDTO part);
    public Task<IReadOnlyCollection<FilePartDTO>> GetBigFilePartsAsync(long fileId);
    public Task<bool> SaveFileReferenceAsync(FileReferenceDTO reference);
    public Task<FileReferenceDTO?> GetFileReferenceAsync(byte[] referenceBytes);
    public Task<bool> SaveProfilePhotoAsync(long userId, long fileId, long accessHash,
        byte[] referenceBytes, DateTime date);
    public Task<bool> DeleteProfilePhotoAsync(long userId, long fileId);
    public Task<IReadOnlyCollection<PhotoDTO>> GetProfilePhotosAsync(long userId);
    public Task<PhotoDTO?> GetProfilePhotoAsync(long userId, long fileId);
    public Task<bool> SaveThumbnailAsync(ThumbnailDTO thumbnail);
    public Task<IReadOnlyCollection<ThumbnailDTO>> GetThumbnailsAsync(long photoId);
    public Task<bool> SaveSignUoNotificationAsync(long userId, bool silent);
    public Task<bool> GetSignUoNotificationAsync(long userId);
}

