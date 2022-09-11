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

using MessagePack;

namespace Ferrite.Data.Repositories;

public class ContactsRepository : IContactsRepository
{
    private readonly IKVStore _store;
    public ContactsRepository(IKVStore store)
    {
        _store = store;
        store.SetSchema(new TableDefinition("ferrite", "contacts",
            new KeyDefinition("pk",
                new DataColumn { Name = "user_id", Type = DataType.Long },
                new DataColumn { Name = "contact_user_id", Type = DataType.Long })));
    }
    public ImportedContactDTO? PutContact(long userId, long contactUserId, InputContactDTO contact)
    {
        var savedContact = new SavedContactDTO(contact.Phone, contact.FirstName, contact.LastName,
            (int)DateTimeOffset.Now.ToUnixTimeSeconds(), contact.ClientId, contactUserId);
        var savedContactBytes = MessagePackSerializer.Serialize(savedContact);
        _store.Put(savedContactBytes, userId, contactUserId);
        return new ImportedContactDTO(contactUserId, contact.ClientId);
    }

    public bool DeleteContact(long userId, long contactUserId)
    {
        return _store.Delete(userId, contactUserId);
    }

    public bool DeleteContacts(long userId)
    {
        return _store.Delete(userId);
    }

    public ICollection<SavedContactDTO> GetSavedContacts(long userId)
    {
        List<SavedContactDTO> savedContacts = new();
        var iter = _store.Iterate(userId);
        foreach (var savedBytes in iter)
        {
            var savedContact = MessagePackSerializer.Deserialize<SavedContactDTO>(savedBytes);
            savedContacts.Add(savedContact);
        }

        return savedContacts;
    }

    public ICollection<ContactDTO> GetContacts(long userId)
    {
        List<ContactDTO> contacts = new();
        var iter = _store.Iterate(userId);
        foreach (var savedBytes in iter)
        {
            var savedContact = MessagePackSerializer.Deserialize<SavedContactDTO>(savedBytes);
            var mutual = _store.Get(savedContact.UserId, userId) != null;
            contacts.Add(new ContactDTO(savedContact.UserId, mutual));
        }

        return contacts;
    }
}