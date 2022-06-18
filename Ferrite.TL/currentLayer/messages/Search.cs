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

namespace Ferrite.TL.currentLayer.messages;
public class Search : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public Search(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -1593989278;
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
            writer.WriteTLString(_q);
            if (_flags[0])
            {
                writer.Write(_fromId.TLBytes, false);
            }

            if (_flags[1])
            {
                writer.WriteInt32(_topMsgId, true);
            }

            writer.Write(_filter.TLBytes, false);
            writer.WriteInt32(_minDate, true);
            writer.WriteInt32(_maxDate, true);
            writer.WriteInt32(_offsetId, true);
            writer.WriteInt32(_addOffset, true);
            writer.WriteInt32(_limit, true);
            writer.WriteInt32(_maxId, true);
            writer.WriteInt32(_minId, true);
            writer.WriteInt64(_hash, true);
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

    private string _q;
    public string Q
    {
        get => _q;
        set
        {
            serialized = false;
            _q = value;
        }
    }

    private InputPeer _fromId;
    public InputPeer FromId
    {
        get => _fromId;
        set
        {
            serialized = false;
            _flags[0] = true;
            _fromId = value;
        }
    }

    private int _topMsgId;
    public int TopMsgId
    {
        get => _topMsgId;
        set
        {
            serialized = false;
            _flags[1] = true;
            _topMsgId = value;
        }
    }

    private MessagesFilter _filter;
    public MessagesFilter Filter
    {
        get => _filter;
        set
        {
            serialized = false;
            _filter = value;
        }
    }

    private int _minDate;
    public int MinDate
    {
        get => _minDate;
        set
        {
            serialized = false;
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
            _maxDate = value;
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
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _peer = (InputPeer)factory.Read(buff.ReadInt32(true), ref buff);
        _q = buff.ReadTLString();
        if (_flags[0])
        {
            _fromId = (InputPeer)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[1])
        {
            _topMsgId = buff.ReadInt32(true);
        }

        _filter = (MessagesFilter)factory.Read(buff.ReadInt32(true), ref buff);
        _minDate = buff.ReadInt32(true);
        _maxDate = buff.ReadInt32(true);
        _offsetId = buff.ReadInt32(true);
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