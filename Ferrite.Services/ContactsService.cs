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

public class ContactsService : IContactsService
{
    public ICollection<long> GetContactIds(long authKeyId, long hash)
    {
        throw new NotImplementedException();
    }

    public ICollection<ContactStatus> GetStatuses(long authKeyId)
    {
        throw new NotImplementedException();
    }

    public Contacts GetContacts(long authKeyId, long hash)
    {
        throw new NotImplementedException();
    }

    public ImportedContacts ImportedContacts(long authKeyId, ICollection<InputContact> contacts)
    {
        throw new NotImplementedException();
    }

    public UpdatesBase DeleteContacts(long authKeyId, ICollection<InputUser> id)
    {
        throw new NotImplementedException();
    }

    public bool DeleteByPhones(long authKeyId, ICollection<string> phones)
    {
        throw new NotImplementedException();
    }

    public bool Block(long authKeyId, InputUser id)
    {
        throw new NotImplementedException();
    }

    public bool Unblock(long authKeyId, InputUser id)
    {
        throw new NotImplementedException();
    }

    public Blocked GetBlocked(long authKeyId, int offset, int limit)
    {
        throw new NotImplementedException();
    }

    public Found Search(long authKeyId, string q, int limit)
    {
        throw new NotImplementedException();
    }

    public ServiceResult<ResolvedPeer> ResolveUsername(long authKeyId, string username)
    {
        throw new NotImplementedException();
    }

    public TopPeers GetTopPeers(long authKeyId, bool correspondents, bool botsPm, bool botsInline, bool phoneCalls,
        bool forwardUsers, bool forwardChats, bool groups, bool channels, int offset, int limit, long hash)
    {
        throw new NotImplementedException();
    }

    public ServiceResult<bool> ResetTopPeerRating(long authKeyId, TopPeerCategory category, Peer peer)
    {
        throw new NotImplementedException();
    }

    public ServiceResult<ICollection<SavedContact>> GetSaved(long authKeyId)
    {
        throw new NotImplementedException();
    }

    public bool ToggleTopPeers(long authKeyId, bool enabled)
    {
        throw new NotImplementedException();
    }

    public ServiceResult<UpdatesBase> AddContact(long authKeyId, bool AddPhonePrivacyException, InputUser id, string firstname, string lastname,
        string phone)
    {
        throw new NotImplementedException();
    }

    public ServiceResult<UpdatesBase> AcceptContact(long authKeyId, InputUser id)
    {
        throw new NotImplementedException();
    }

    public ServiceResult<UpdatesBase> GetLocated(long authKeyId, bool background, InputGeoPoint geoPoint, int? selfExpires)
    {
        throw new NotImplementedException();
    }

    public UpdatesBase BlockFromReplies(long authKeyId, bool deleteMessage, bool deleteHistory, bool reportSpam, int messageId)
    {
        throw new NotImplementedException();
    }

    public ServiceResult<ResolvedPeer> ResolvePhone(long authKeyId, string phone)
    {
        throw new NotImplementedException();
    }
}