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
public class DeleteHistory : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IMessagesService _messages;
    private readonly IMapperContext _mapper;
    private bool serialized = false;
    public DeleteHistory(ITLObjectFactory objectFactory, IMessagesService messages,
        IMapperContext mapper)
    {
        factory = objectFactory;
        _messages = messages;
        _mapper = mapper;
    }

    public int Constructor => -1332768214;
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
            writer.WriteInt32(_maxId, true);
            if (_flags[2])
            {
                writer.WriteInt32(_minDate, true);
            }

            if (_flags[3])
            {
                writer.WriteInt32(_maxDate, true);
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

    public bool JustClear
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool Revoke
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
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

    private int _minDate;
    public int MinDate
    {
        get => _minDate;
        set
        {
            serialized = false;
            _flags[2] = true;
            _minDate = value;
        }
    }

    private int _maxDate;
    public int MaxDate
    {
        get => _maxDate;
        set
        {
            serialized = false;
            _flags[3] = true;
            _maxDate = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var serviceResult = await _messages.DeleteHistory(ctx.CurrentAuthKeyId,
            _mapper.MapToDTO<InputPeer, InputPeerDTO>(_peer), _maxId,
            _flags[2] ? _minDate : null,
            _flags[3] ? _maxDate : null,
            JustClear, Revoke);
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
            var affected = factory.Resolve<AffectedHistoryImpl>();
            affected.Pts = serviceResult.Result.Pts;
            affected.PtsCount = serviceResult.Result.PtsCount;
            affected.Offset = serviceResult.Result.Offset;
            rpcResult.Result = affected;
        }

        return rpcResult;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _peer = (InputPeer)factory.Read(buff.ReadInt32(true), ref buff);
        _maxId = buff.ReadInt32(true);
        if (_flags[2])
        {
            _minDate = buff.ReadInt32(true);
        }

        if (_flags[3])
        {
            _maxDate = buff.ReadInt32(true);
        }
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}