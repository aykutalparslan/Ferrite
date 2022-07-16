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
using Ferrite.Data.Repositories;
using Ferrite.Services;
using Ferrite.TL.mtproto;
using Ferrite.TL.ObjectMapper;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer.messages;
public class GetMessages : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IMessagesService _messages;
    private readonly IMapperContext _mapper;
    private bool serialized = false;
    public GetMessages(ITLObjectFactory objectFactory, IMessagesService messages, IMapperContext mapper)
    {
        factory = objectFactory;
        _messages = messages;
        _mapper = mapper;
    }

    public int Constructor => 1673946374;
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

    private Vector<InputMessage> _id;
    public Vector<InputMessage> Id
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
        List<InputMessageDTO> ids = new List<InputMessageDTO>();
        foreach (var m in _id)
        {
            ids.Add(_mapper.MapToDTO<InputMessage, InputMessageDTO>(m));
        }

        var serviceResult = await _messages.GetMessagesAsync(ctx.CurrentAuthKeyId, ids);
        var result = factory.Resolve<RpcResult>();
        if (!serviceResult.Success)
        {
            var err = factory.Resolve<RpcError>();
            err.ErrorCode = serviceResult.ErrorMessage.Code;
            err.ErrorMessage = serviceResult.ErrorMessage.Message;
            result.Result = err;
        }
        else
        {
            var messages = factory.Resolve<MessagesImpl>();
            messages.Chats = factory.Resolve<Vector<Chat>>();
            foreach (var c in serviceResult.Result.Chats)
            {
                messages.Chats.Add(_mapper.MapToTLObject<Chat, ChatDTO>(c));
            }
            messages.Messages = factory.Resolve<Vector<Message>>();
            foreach (var m in serviceResult.Result.Messages)
            {
                messages.Messages.Add(_mapper.MapToTLObject<Message, MessageDTO>(m));
            }
            messages.Users = factory.Resolve<Vector<User>>();
            foreach (var u in serviceResult.Result.Users)
            {
                messages.Users.Add(_mapper.MapToTLObject<User, UserDTO>(u));
            }
        }

        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        buff.Skip(4); _id  =  factory . Read < Vector < InputMessage > > ( ref  buff ) ; 
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}