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

using Ferrite.TL.slim;
using Ferrite.TL.slim.dto;
using Ferrite.TL.slim.layer150;

namespace Ferrite.Data.Repositories;

public class ContactsRepository : IContactsRepository
{
    private readonly IKVStore _store;
    private readonly IKVStore _storeMutual;
    public ContactsRepository(IKVStore store, IKVStore storeMutual)
    {
        _store = store;
        store.SetSchema(new TableDefinition("ferrite", "contacts",
            new KeyDefinition("pk",
                new DataColumn { Name = "user_id", Type = DataType.Long },
                new DataColumn { Name = "contact_user_id", Type = DataType.Long })));
        _storeMutual = storeMutual;
        _storeMutual.SetSchema(new TableDefinition("ferrite", "contacts_mutual",
            new KeyDefinition("pk",
                new DataColumn { Name = "user_id", Type = DataType.Long },
                new DataColumn { Name = "contact_user_id", Type = DataType.Long })));
    }
    public TLImportedContact PutContact(long userId, long contactUserId, TLContactInfo contact)
    {
        _store.Put(contact.AsSpan().ToArray(), userId, contactUserId);
        _storeMutual.Put(BitConverter.GetBytes(userId), contactUserId, userId);
        var c = contact.AsContactInfo();
        return ImportedContact.Builder()
            .ClientId(c.ClientId)
            .UserId(c.UserId)
            .Build();
    }

    public bool DeleteContact(long userId, long contactUserId)
    {
        return _store.Delete(userId, contactUserId);
    }

    public bool DeleteContacts(long userId)
    {
        return _store.Delete(userId);
    }

    public IReadOnlyList<TLSavedContact> GetSavedContacts(long userId)
    {
        List<TLSavedContact> savedContacts = new();
        var iter = _store.Iterate(userId);
        foreach (var savedBytes in iter)
        {
            var contactInfo = new TLContactInfo(savedBytes, 0, savedBytes.Length)
                .AsContactInfo();
            savedContacts.Add(SavedPhoneContact.Builder()
                .Phone(contactInfo.Phone)
                .FirstName(contactInfo.FirstName)
                .LastName(contactInfo.LastName)
                .Date(contactInfo.Date)
                .Build());
        }

        return savedContacts;
    }

    public IReadOnlyList<TLContact> GetContacts(long userId)
    {
        List<TLContact> contacts = new();
        var contactsIterator = _store.Iterate(userId);
        List<long> mutualContacts = new ();
        var mutualIterator = _storeMutual.Iterate(userId);
        foreach (var c in mutualIterator)
        {
            mutualContacts.Add(BitConverter.ToInt64(c));
        }
        foreach (var savedBytes in contactsIterator)
        {
            var contact = new TLContactInfo(savedBytes, 0, savedBytes.Length).AsContactInfo();
            var mutual = mutualContacts.Contains(contact.UserId);
            contacts.Add(Contact.Builder()
                .UserId(contact.UserId)
                .Mutual(mutual)
                .Build());
        }

        return contacts;
    }
}