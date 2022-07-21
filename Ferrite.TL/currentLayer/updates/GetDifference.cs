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

namespace Ferrite.TL.currentLayer.updates;
public class GetDifference : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IUpdatesService _updates;
    private readonly IMapperContext _mapper;
    private bool serialized = false;
    public GetDifference(ITLObjectFactory objectFactory, IUpdatesService updates, IMapperContext mapper)
    {
        factory = objectFactory;
        _updates = updates;
        _mapper = mapper;
    }

    public int Constructor => 630429265;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt32(_pts, true);
            if (_flags[0])
            {
                writer.WriteInt32(_ptsTotalLimit, true);
            }

            writer.WriteInt32(_date, true);
            writer.WriteInt32(_qts, true);
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

    private int _pts;
    public int Pts
    {
        get => _pts;
        set
        {
            serialized = false;
            _pts = value;
        }
    }

    private int _ptsTotalLimit;
    public int PtsTotalLimit
    {
        get => _ptsTotalLimit;
        set
        {
            serialized = false;
            _flags[0] = true;
            _ptsTotalLimit = value;
        }
    }

    private int _date;
    public int Date
    {
        get => _date;
        set
        {
            serialized = false;
            _date = value;
        }
    }

    private int _qts;
    public int Qts
    {
        get => _qts;
        set
        {
            serialized = false;
            _qts = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var serviceResult = await _updates.GetDifference(ctx.CurrentAuthKeyId,
            _pts, _date, _qts, _flags[0] ? _ptsTotalLimit : null);
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        if (!serviceResult.Success)
        {
            var err = factory.Resolve<RpcError>();
            err.ErrorCode = serviceResult.ErrorMessage.Code;
            err.ErrorMessage = serviceResult.ErrorMessage.Message;
            result.Result = err;
        }
        else
        {
            var diff = factory.Resolve<DifferenceImpl>();
            diff.NewMessages = factory.Resolve<Vector<Message>>();
            diff.NewEncryptedMessages = factory.Resolve<Vector<EncryptedMessage>>();
            diff.OtherUpdates = factory.Resolve<Vector<Update>>();
            diff.Chats = factory.Resolve<Vector<Chat>>();
            diff.Users = factory.Resolve<Vector<User>>();
            foreach (var c in serviceResult.Result.Chats)
            {
                diff.Chats.Add(_mapper.MapToTLObject<Chat, ChatDTO>(c));
            }
            foreach (var m in serviceResult.Result.NewMessages)
            {
                diff.NewMessages.Add(_mapper.MapToTLObject<Message, MessageDTO>(m));
            }
            foreach (var u in serviceResult.Result.Users)
            {
                diff.Users.Add(_mapper.MapToTLObject<User, UserDTO>(u));
            }
            var state = factory.Resolve<StateImpl>();
            state.Date = state.Date;
            state.Pts = state.Pts;
            state.Qts = state.Qts;
            state.Seq = state.Seq;
            diff.State = state;
            result.Result = diff;
        }
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _pts = buff.ReadInt32(true);
        if (_flags[0])
        {
            _ptsTotalLimit = buff.ReadInt32(true);
        }

        _date = buff.ReadInt32(true);
        _qts = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}