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

using Autofac.Extras.Moq;
using Ferrite.Data.Repositories;
using Ferrite.TL.slim;
using Ferrite.TL.slim.dto;
using Ferrite.TL.slim.layer150;
using Moq;
using Xunit;

namespace Ferrite.Tests.Data;

public class ContactsRepositoryTests
{
    [Fact]
    public void Puts_Contact()
    {
        using TLContactInfo savedContact = ContactInfo.Builder()
            .UserId(222)
            .ClientId(333)
            .Date(555)
            .Phone("aaa"u8)
            .FirstName("a"u8)
            .LastName("b"u8)
            .Build();
        var savedContactBytes = savedContact.AsSpan().ToArray();
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Put(savedContactBytes, 
            (long)111, (long)222)).Verifiable();

        var repo = mock.Create<ContactsRepository>();
        repo.PutContact(111, 222, savedContact);
        store.VerifyAll();
    }

    [Fact]
    public void Deletes_Contact()
    {
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Delete((long)111, (long)222)).Verifiable();

        var repo = mock.Create<ContactsRepository>();
        repo.DeleteContact(111, 222);
        store.VerifyAll();
    }
    
    [Fact]
    public void Deletes_Contacts()
    {
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Delete((long)111)).Verifiable();

        var repo = mock.Create<ContactsRepository>();
        repo.DeleteContacts(111);
        store.VerifyAll();
    }
    
    [Fact]
    public void Gets_SavedContacts()
    {
        List<byte[]> saved = new();
        List<byte[]> result = new();
        for (int i = 0; i < 2; i++)
        {
            using TLContactInfo savedContact = ContactInfo.Builder()
                .UserId(222+i)
                .ClientId(333 + i)
                .Date(555 + i)
                .Phone("aaa"u8)
                .FirstName("a"u8)
                .LastName("b"u8)
                .Build();
            saved.Add(savedContact.AsSpan().ToArray());
            using TLSavedContact c = SavedPhoneContact.Builder()
                .Date(555 + i)
                .Phone("aaa"u8)
                .FirstName("a"u8)
                .LastName("b"u8)
                .Build();
            result.Add(c.AsSpan().ToArray());
        }
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Iterate((long)123)).Returns(saved);

        var repo = mock.Create<ContactsRepository>();
        var contacts = repo.GetSavedContacts(123);
        for (int i = 0; i < 2; i++)
        {
            Assert.Equal(result[i], 
                contacts[i].AsSpan().ToArray());
        }
        store.VerifyAll();
    }
    
    [Fact]
    public void Gets_Contacts()
    {
        List<byte[]> saved = new();
        List<byte[]> contacts = new();
        using var contact1 = Contact.Builder()
            .UserId(222)
            .Mutual(false)
            .Build().TLBytes!.Value;
        contacts.Add(contact1.AsSpan().ToArray());
        using var contact2 = Contact.Builder()
            .UserId(223)
            .Mutual(true)
            .Build().TLBytes!.Value;
        contacts.Add(contact2.AsSpan().ToArray());
        List<byte[]> mutualContacts = new (){BitConverter.GetBytes((long)223)};
        for (int i = 0; i < 2; i++)
        {
            using TLContactInfo savedContact = ContactInfo.Builder()
                .UserId(222+i)
                .ClientId(333 + i)
                .Date(555 + i)
                .Phone("aaa"u8)
                .FirstName("a"u8)
                .LastName("b"u8)
                .Build();
            saved.Add(savedContact.AsSpan().ToArray());
        }
        var mock = AutoMock.GetLoose();
        var store = new Mock<IKVStore>();
        store.Setup(x => x.Iterate((long)123)).Returns(saved);
        var store2 = new Mock<IKVStore>();
        store2.Setup(x => x.Iterate((long)123)).Returns(mutualContacts);

        var repo = new ContactsRepository(store.Object, store2.Object);
        var result = repo.GetContacts(123);
        for (int i = 0; i < 2; i++)
        {
            Assert.Equal(contacts[i], 
                result[i].AsSpan().ToArray());
        }
        store.VerifyAll();
    }
}