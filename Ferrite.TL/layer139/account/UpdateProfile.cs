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
using StackExchange.Redis;

namespace Ferrite.TL.layer139.account;
public class UpdateProfile : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IAccountService _account;
    private bool serialized = false;
    public UpdateProfile(ITLObjectFactory objectFactory, IAccountService account)
    {
        factory = objectFactory;
        _account = account;
    }

    public int Constructor => 2018596725;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            if (_flags[0])
            {
                writer.WriteTLString(_firstName);
            }

            if (_flags[1])
            {
                writer.WriteTLString(_lastName);
            }

            if (_flags[2])
            {
                writer.WriteTLString(_about);
            }

            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private Flags _flags;
    public Flags Flags
    {
        get => _flags;
        set
        {
            serialized = false;
            _flags = value;
        }
    }

    private string _firstName;
    public string FirstName
    {
        get => _firstName;
        set
        {
            serialized = false;
            _flags[0] = true;
            _firstName = value;
        }
    }

    private string _lastName;
    public string LastName
    {
        get => _lastName;
        set
        {
            serialized = false;
            _flags[1] = true;
            _lastName = value;
        }
    }

    private string _about;
    public string About
    {
        get => _about;
        set
        {
            serialized = false;
            _flags[2] = true;
            _about = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        var userNew = await _account.UpdateProfile(ctx.PermAuthKeyId!=0 ? ctx.PermAuthKeyId : ctx.AuthKeyId, _firstName, _lastName, _about);
        if (userNew == null)
        {
            var userEmpty = factory.Resolve<UserEmptyImpl>();
            result.Result = userEmpty;
        }
        else
        {
            var user = factory.Resolve<UserImpl>();
            user.FirstName = userNew.FirstName;
            user.LastName = userNew.LastName;
            user.Phone = userNew.Phone;
            user.Self = true;
            //TODO: get user status
            user.Status = factory.Resolve<UserStatusEmptyImpl>();
            //TODO: get user photo
            user.Photo = factory.Resolve<UserProfilePhotoEmptyImpl>();
        }
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        if (_flags[0])
        {
            _firstName = buff.ReadTLString();
        }

        if (_flags[1])
        {
            _lastName = buff.ReadTLString();
        }

        if (_flags[2])
        {
            _about = buff.ReadTLString();
        }
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}