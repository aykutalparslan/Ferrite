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
    ICollection<long> GetContactIds(long authKeyId, long hash);
    ICollection<ContactStatus> GetStatuses(long authKeyId);
    Data.Contacts.Contacts GetContacts(long authKeyId, long hash);
    Data.Contacts.ImportedContacts ImportedContacts(long authKeyId, ICollection<InputContact> contacts);
    UpdatesBase DeleteContacts(long authKeyId, ICollection<InputUser> id);
    bool DeleteByPhones(long authKeyId, ICollection<string> phones);
    bool Block(long authKeyId, InputUser id);
    bool Unblock(long authKeyId, InputUser id);
    Data.Contacts.Blocked GetBlocked(long authKeyId, int offset, int limit);
    Data.Contacts.Found Search(long authKeyId, string q, int limit);
    ServiceResult<ResolvedPeer> ResolveUsername(long authKeyId, string username);
    Data.Contacts.TopPeers GetTopPeers(long authKeyId, bool correspondents, bool botsPm, bool botsInline,
        bool phoneCalls, bool forwardUsers, bool forwardChats, bool groups, bool channels, 
        int offset, int limit, long hash);

    ServiceResult<bool> ResetTopPeerRating(long authKeyId, TopPeerCategory category, Peer peer);
    ServiceResult<ICollection<SavedContact>> GetSaved(long authKeyId);
    bool ToggleTopPeers(long authKeyId, bool enabled);
    ServiceResult<UpdatesBase> AddContact(long authKeyId, bool AddPhonePrivacyException, InputUser id,
        string firstname, string lastname, string phone);
    ServiceResult<UpdatesBase> AcceptContact(long authKeyId, InputUser id);
    ServiceResult<UpdatesBase> GetLocated(long authKeyId, bool background, InputGeoPoint geoPoint, int? selfExpires);
    UpdatesBase BlockFromReplies(long authKeyId, bool deleteMessage, bool deleteHistory, bool reportSpam, int messageId);
    ServiceResult<ResolvedPeer> ResolvePhone(long authKeyId, string phone);
}