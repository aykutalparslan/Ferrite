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
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer.contacts;
public class ImportContacts : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IContactsService _contactsService;
    private bool serialized = false;
    public ImportContacts(ITLObjectFactory objectFactory, IContactsService contactsService)
    {
        factory = objectFactory;
        _contactsService = contactsService;
    }

    public int Constructor => 746589157;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_contacts.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private Vector<InputContact> _contacts;
    public Vector<InputContact> Contacts
    {
        get => _contacts;
        set
        {
            serialized = false;
            _contacts = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        List<Data.InputContact> contacts = new();
        foreach (var c in _contacts)
        {
            if (c is InputPhoneContactImpl phoneContact)
            {
                contacts.Add(new Data.InputContact(phoneContact.ClientId,
                    phoneContact.Phone.Replace("+",""), phoneContact.FirstName, phoneContact.LastName));
            }
        }

        var serviceResult = await _contactsService.ImportContacts(ctx.PermAuthKeyId!= 0 ? ctx.PermAuthKeyId : ctx.AuthKeyId, contacts);
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        var imported = factory.Resolve<ImportedContactsImpl>();
        var importedList = factory.Resolve<Vector<ImportedContact>>();
        var usersList = factory.Resolve<Vector<User>>();
        VectorOfLong retry = new VectorOfLong();
        var popularList = factory.Resolve<Vector<PopularContact>>();
        foreach (var i in serviceResult.Imported)
        {
            var ic = factory.Resolve<ImportedContactImpl>();
            ic.ClientId = i.ClientId;
            ic.UserId = i.UserId;
            importedList.Add(ic);
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
        foreach (var r in serviceResult.RetryContact)
        {
            retry.Add(r);
        }

        foreach (var p in serviceResult.PopularInvites)
        {
            var pop = factory.Resolve<PopularContactImpl>();
            pop.Importers = p.Importers;
            pop.ClientId = p.ClientId;
            popularList.Add(pop);
        }
        imported.Imported = importedList;
        imported.Users = usersList;
        imported.RetryContacts = retry;
        imported.PopularInvites = popularList;
        result.Result = imported;
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        buff.Skip(4); _contacts  =  factory . Read < Vector < InputContact > > ( ref  buff ) ; 
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}