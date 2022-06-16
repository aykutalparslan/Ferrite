/*
 *   Project Ferrite is an Implementation Telegram Server API
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
using System.Buffers;
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Services;
using Ferrite.TL.layer139.account;
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.TL.layer139.contacts;
public class GetContacts : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IContactsService _contacts;
    private bool serialized = false;
    public GetContacts(ITLObjectFactory objectFactory, IContactsService contacts)
    {
        factory = objectFactory;
        _contacts = contacts;
    }

    public int Constructor => 1574346258;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(_hash, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private long _hash;
    public long Hash
    {
        get => _hash;
        set
        {
            serialized = false;
            _hash = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var serviceResult = await _contacts.GetContacts(ctx.AuthKeyId, _hash);
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        var contacts = factory.Resolve<ContactsImpl>();
        contacts.SavedCount = serviceResult.SavedCount;
        var contactsList = factory.Resolve<Vector<Contact>>();
        var usersList = factory.Resolve<Vector<User>>();
        foreach (var c in serviceResult.ContactList)
        {
            var contact = factory.Resolve<ContactImpl>();
            contact.Mutual = c.Mutual;
            contact.UserId = c.UserId;
            contactsList.Add(contact);
        }
        foreach (var u in serviceResult.Users)
        {
            var userImpl = factory.Resolve<UserImpl>();
            userImpl.Id = u.Id;
            userImpl.FirstName = u.FirstName;
            userImpl.LastName = u.LastName;
            userImpl.Phone = u.Phone;
            userImpl.Self = u.Self;
            if(u.Status == Data.UserStatus.Empty)
            {
                userImpl.Status = factory.Resolve<UserStatusEmptyImpl>();
            }
            if (u.Photo.Empty)
            {
                userImpl.Photo = factory.Resolve<UserProfilePhotoEmptyImpl>();
            }
            usersList.Add(userImpl);
        }
        contacts.Contacts = contactsList;
        contacts.Users = usersList;
        result.Result = contacts;
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _hash = buff.ReadInt64(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}