// 
// Project Ferrite is an Implementation of the Telegram Server API
// Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 

using Ferrite.Data;
using Ferrite.Data.Contacts;

namespace Ferrite.Services;

public interface IContactsService
{
    Task<ICollection<long>> GetContactIds(long authKeyId, long hash);
    Task<ICollection<ContactStatus>> GetStatuses(long authKeyId);
    Task<Data.Contacts.Contacts> GetContacts(long authKeyId, long hash);
    Task<Data.Contacts.ImportedContacts> ImportContacts(long authKeyId, ICollection<InputContact> contacts);
    Task<UpdatesBase?> DeleteContacts(long authKeyId, ICollection<InputUser> id);
    Task<bool> DeleteByPhones(long authKeyId, ICollection<string> phones);
    Task<bool> Block(long authKeyId, InputPeer id);
    Task<bool> Unblock(long authKeyId, InputPeer id);
    Task<Data.Contacts.Blocked> GetBlocked(long authKeyId, int offset, int limit);
    Task<Data.Contacts.Found> Search(long authKeyId, string q, int limit);
    Task<ServiceResult<ResolvedPeer>> ResolveUsername(long authKeyId, string username);
    Task<Data.Contacts.TopPeers> GetTopPeers(long authKeyId, bool correspondents, bool botsPm, bool botsInline,
        bool phoneCalls, bool forwardUsers, bool forwardChats, bool groups, bool channels, 
        int offset, int limit, long hash);

    Task<ServiceResult<bool>> ResetTopPeerRating(long authKeyId, TopPeerCategory category, Peer peer);
    Task<bool> ResetSaved(long authKeyId);
    Task<ServiceResult<ICollection<SavedContact>>> GetSaved(long authKeyId);
    Task<bool> ToggleTopPeers(long authKeyId, bool enabled);
    Task<ServiceResult<UpdatesBase>> AddContact(long authKeyId, bool AddPhonePrivacyException, InputUser id,
        string firstname, string lastname, string phone);
    Task<ServiceResult<UpdatesBase>> AcceptContact(long authKeyId, InputUser id);
    Task<ServiceResult<UpdatesBase>> GetLocated(long authKeyId, bool background, InputGeoPoint geoPoint, int? selfExpires);
    Task<UpdatesBase?> BlockFromReplies(long authKeyId, bool deleteMessage, bool deleteHistory, bool reportSpam, int messageId);
    Task<ServiceResult<ResolvedPeer>> ResolvePhone(long authKeyId, string phone);
}