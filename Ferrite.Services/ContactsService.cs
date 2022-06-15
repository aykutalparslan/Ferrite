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
    private readonly IPersistentStore _store;
    public ContactsService(IPersistentStore store)
    {
        _store = store;
    }

    public async Task<ICollection<long>> GetContactIds(long authKeyId, long hash)
    {
        return new List<long>();
    }

    public async Task<ICollection<ContactStatus>> GetStatuses(long authKeyId)
    {
        return new List<ContactStatus>();
    }

    public async Task<Contacts> GetContacts(long authKeyId, long hash)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        var contactList = await _store.GetContactsAsync(auth.UserId);
        List<User> userList = new List<User>();
        foreach (var c in contactList)
        {
            userList.Add(await _store.GetUserAsync(c.UserId));
        }

        return new Contacts(contactList, contactList.Count, userList);
    }

    public async Task<ImportedContacts> ImportContacts(long authKeyId, ICollection<InputContact> contacts)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        List<ImportedContact> importedContacts = new();
        List<User> users = new();
        foreach (var c in contacts)
        {
            var imported = await _store.SaveContactAsync(auth.UserId, c);
            users.Add(await  _store.GetUserAsync(imported.UserId));
            importedContacts.Add(imported);
        }

        return new ImportedContacts(importedContacts, new List<PopularContact>(), 
            new List<long>(), users);
    }

    public async Task<UpdatesBase?> DeleteContacts(long authKeyId, ICollection<InputUser> id)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        foreach (var c in id)
        {
            await _store.DeleteContactAsync(auth.UserId, c.UserId);
        }
        
        return null;
    }

    public async Task<bool> DeleteByPhones(long authKeyId, ICollection<string> phones)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        foreach (var p in phones)
        {
            var userId = await _store.GetUserIdAsync(p);
            await _store.DeleteContactAsync(auth.UserId, userId);
        }

        return true;
    }

    public async Task<bool> Block(long authKeyId, InputUser id)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        return await _store.SaveBlockedUserAsync(auth.UserId, id.UserId);
    }

    public async Task<bool> Unblock(long authKeyId, InputUser id)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        return await _store.DeleteBlockedUserAsync(auth.UserId, id.UserId);
    }

    public async Task<Blocked> GetBlocked(long authKeyId, int offset, int limit)
    {
        throw new NotImplementedException();
    }

    public async Task<Found> Search(long authKeyId, string q, int limit)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<ResolvedPeer>> ResolveUsername(long authKeyId, string username)
    {
        throw new NotImplementedException();
    }

    public async Task<TopPeers> GetTopPeers(long authKeyId, bool correspondents, bool botsPm, bool botsInline, bool phoneCalls, bool forwardUsers,
        bool forwardChats, bool groups, bool channels, int offset, int limit, long hash)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<bool>> ResetTopPeerRating(long authKeyId, TopPeerCategory category, Peer peer)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<ICollection<SavedContact>>> GetSaved(long authKeyId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ToggleTopPeers(long authKeyId, bool enabled)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<UpdatesBase>> AddContact(long authKeyId, bool AddPhonePrivacyException, InputUser id, string firstname, string lastname,
        string phone)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<UpdatesBase>> AcceptContact(long authKeyId, InputUser id)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<UpdatesBase>> GetLocated(long authKeyId, bool background, InputGeoPoint geoPoint, int? selfExpires)
    {
        throw new NotImplementedException();
    }

    public async Task<UpdatesBase?> BlockFromReplies(long authKeyId, bool deleteMessage, bool deleteHistory, bool reportSpam, int messageId)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<ResolvedPeer>> ResolvePhone(long authKeyId, string phone)
    {
        throw new NotImplementedException();
    }
}