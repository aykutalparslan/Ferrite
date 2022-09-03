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
public class SetTyping : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IMessagesService _messages;
    private readonly IMapperContext _mapper;
    private bool serialized = false;
    public SetTyping(ITLObjectFactory objectFactory, IMessagesService messages,
        IMapperContext mapper)
    {
        factory = objectFactory;
        _messages = messages;
        _mapper = mapper;
    }

    public int Constructor => 1486110434;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.Write(_peer.TLBytes, false);
            if (_flags[0])
            {
                writer.WriteInt32(_topMsgId, true);
            }

            writer.Write(_action.TLBytes, false);
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

    private int _topMsgId;
    public int TopMsgId
    {
        get => _topMsgId;
        set
        {
            serialized = false;
            _flags[0] = true;
            _topMsgId = value;
        }
    }

    private SendMessageAction _action;
    public SendMessageAction Action
    {
        get => _action;
        set
        {
            serialized = false;
            _action = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var serviceResult = await _messages.SetTyping(ctx.CurrentAuthKeyId,
            _mapper.MapToDTO<InputPeer, InputPeerDTO>(_peer),
            _mapper.MapToDTO<SendMessageAction, SendMessageActionDTO>(_action), _flags[0] ? _topMsgId : null);
        var rpcResult = factory.Resolve<RpcResult>();
        rpcResult.ReqMsgId = ctx.MessageId;
        if (!serviceResult.Success)
        {
            var err = factory.Resolve<RpcError>();
            err.ErrorCode = serviceResult.ErrorMessage.Code;
            err.ErrorMessage = serviceResult.ErrorMessage.Message;
            rpcResult.Result = err;
        }
        else
        {
            rpcResult.Result = serviceResult.Result ? new BoolTrue() : new BoolFalse();
        }
        
        return rpcResult;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _peer = (InputPeer)factory.Read(buff.ReadInt32(true), ref buff);
        if (_flags[0])
        {
            _topMsgId = buff.ReadInt32(true);
        }

        _action = (SendMessageAction)factory.Read(buff.ReadInt32(true), ref buff);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}