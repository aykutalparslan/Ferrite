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
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL.mtproto;
using Ferrite.TL.ObjectMapper;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer.users;
public class GetUsers : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IUsersService _users;
    private readonly IMapperContext _mapper;
    private bool serialized = false;
    public GetUsers(ITLObjectFactory objectFactory, IUsersService users, IMapperContext mapper)
    {
        factory = objectFactory;
        _users = users;
        _mapper = mapper;
    }

    public int Constructor => 227648840;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_id.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private Vector<InputUser> _id;
    public Vector<InputUser> Id
    {
        get => _id;
        set
        {
            serialized = false;
            _id = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        /*var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        var inputUsers = new List<InputUserDTO>();
        foreach (var u in _id)
        {
            inputUsers.Add(_mapper.MapToDTO<InputUser, InputUserDTO>(u));
        }

        var serviceResult = await _users.GetUsers(ctx.PermAuthKeyId != 0 ? ctx.PermAuthKeyId : ctx.AuthKeyId,
            inputUsers);
        if (serviceResult.Success)
        {
            var users = factory.Resolve<Vector<User>>();
            foreach (var u in serviceResult.Result)
            {
                users.Add(_mapper.MapToTLObject<User, UserDTO>(u));
            }

            result.Result = users;
        }
        else
        {
            var err = factory.Resolve<RpcError>();
            err.ErrorCode = serviceResult.ErrorMessage.Code;
            err.ErrorMessage = serviceResult.ErrorMessage.Message;
            result.Result = err;
        }
        return result;*/
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        buff.Skip(4); _id  =  factory . Read < Vector < InputUser > > ( ref  buff ) ; 
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}