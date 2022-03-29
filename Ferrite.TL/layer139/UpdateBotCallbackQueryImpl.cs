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
using Ferrite.Utils;

namespace Ferrite.TL.layer139;
public class UpdateBotCallbackQueryImpl : Update
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public UpdateBotCallbackQueryImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1177566067;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt64(_queryId, true);
            writer.WriteInt64(_userId, true);
            writer.Write(_peer.TLBytes, false);
            writer.WriteInt32(_msgId, true);
            writer.WriteInt64(_chatInstance, true);
            if (_flags[0])
            {
                writer.WriteTLBytes(_data);
            }

            if (_flags[1])
            {
                writer.WriteTLString(_gameShortName);
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

    private long _queryId;
    public long QueryId
    {
        get => _queryId;
        set
        {
            serialized = false;
            _queryId = value;
        }
    }

    private long _userId;
    public long UserId
    {
        get => _userId;
        set
        {
            serialized = false;
            _userId = value;
        }
    }

    private Peer _peer;
    public Peer Peer
    {
        get => _peer;
        set
        {
            serialized = false;
            _peer = value;
        }
    }

    private int _msgId;
    public int MsgId
    {
        get => _msgId;
        set
        {
            serialized = false;
            _msgId = value;
        }
    }

    private long _chatInstance;
    public long ChatInstance
    {
        get => _chatInstance;
        set
        {
            serialized = false;
            _chatInstance = value;
        }
    }

    private byte[] _data;
    public byte[] Data
    {
        get => _data;
        set
        {
            serialized = false;
            _flags[0] = true;
            _data = value;
        }
    }

    private string _gameShortName;
    public string GameShortName
    {
        get => _gameShortName;
        set
        {
            serialized = false;
            _flags[1] = true;
            _gameShortName = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _queryId = buff.ReadInt64(true);
        _userId = buff.ReadInt64(true);
        buff.Skip(4); _peer  =  factory . Read < Peer > ( ref  buff ) ; 
        _msgId = buff.ReadInt32(true);
        _chatInstance = buff.ReadInt64(true);
        if (_flags[0])
        {
            _data = buff.ReadTLBytes().ToArray();
        }

        if (_flags[1])
        {
            _gameShortName = buff.ReadTLString();
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}