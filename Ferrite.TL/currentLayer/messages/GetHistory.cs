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

namespace Ferrite.TL.currentLayer.messages;
public class GetHistory : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IMessagesService _messages;
    private readonly IMapperContext _mapper;
    private bool serialized = false;
    public GetHistory(ITLObjectFactory objectFactory, IMessagesService messages, IMapperContext mapper)
    {
        factory = objectFactory;
        _messages = messages;
        _mapper = mapper;
    }

    public int Constructor => 1143203525;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_peer.TLBytes, false);
            writer.WriteInt32(_offsetId, true);
            writer.WriteInt32(_offsetDate, true);
            writer.WriteInt32(_addOffset, true);
            writer.WriteInt32(_limit, true);
            writer.WriteInt32(_maxId, true);
            writer.WriteInt32(_minId, true);
            writer.WriteInt64(_hash, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private InputPeer _peer;
    public InputPeer Peer
    {
        get => _peer;
        set
        {
            serialized = false;
            _peer = value;
        }
    }

    private int _offsetId;
    public int OffsetId
    {
        get => _offsetId;
        set
        {
            serialized = false;
            _offsetId = value;
        }
    }

    private int _offsetDate;
    public int OffsetDate
    {
        get => _offsetDate;
        set
        {
            serialized = false;
            _offsetDate = value;
        }
    }

    private int _addOffset;
    public int AddOffset
    {
        get => _addOffset;
        set
        {
            serialized = false;
            _addOffset = value;
        }
    }

    private int _limit;
    public int Limit
    {
        get => _limit;
        set
        {
            serialized = false;
            _limit = value;
        }
    }

    private int _maxId;
    public int MaxId
    {
        get => _maxId;
        set
        {
            serialized = false;
            _maxId = value;
        }
    }

    private int _minId;
    public int MinId
    {
        get => _minId;
        set
        {
            serialized = false;
            _minId = value;
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
        var rpcResult = factory.Resolve<RpcResult>();
        rpcResult.ReqMsgId = ctx.MessageId;
        var serviceResult = await _messages.GetHistory(ctx.CurrentAuthKeyId,
            _mapper.MapToDTO<InputPeer, InputPeerDTO>(_peer),
            _offsetId, _offsetDate, _addOffset, _limit,
            _maxId, _minId, _hash);
        if (!serviceResult.Success)
        {
            var err = factory.Resolve<RpcError>();
            err.ErrorCode = serviceResult.ErrorMessage.Code;
            err.ErrorMessage = serviceResult.ErrorMessage.Message;
            rpcResult.Result = err;
        }
        else if (serviceResult.Result.Messages.Count == 0)
        {
            var messages = factory.Resolve<MessagesNotModifiedImpl>();
            rpcResult.Result = messages;
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
            rpcResult.Result = messages;
        }
        return rpcResult;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _peer = (InputPeer)factory.Read(buff.ReadInt32(true), ref buff);
        _offsetId = buff.ReadInt32(true);
        _offsetDate = buff.ReadInt32(true);
        _addOffset = buff.ReadInt32(true);
        _limit = buff.ReadInt32(true);
        _maxId = buff.ReadInt32(true);
        _minId = buff.ReadInt32(true);
        _hash = buff.ReadInt64(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}