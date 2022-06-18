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
public class BotResultsImpl : BotResults
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public BotResultsImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1803769784;
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
            if (_flags[1])
            {
                writer.WriteTLString(_nextOffset);
            }

            if (_flags[2])
            {
                writer.Write(_switchPm.TLBytes, false);
            }

            writer.Write(_results.TLBytes, false);
            writer.WriteInt32(_cacheTime, true);
            writer.Write(_users.TLBytes, false);
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

    public bool Gallery
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
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

    private string _nextOffset;
    public string NextOffset
    {
        get => _nextOffset;
        set
        {
            serialized = false;
            _flags[1] = true;
            _nextOffset = value;
        }
    }

    private InlineBotSwitchPM _switchPm;
    public InlineBotSwitchPM SwitchPm
    {
        get => _switchPm;
        set
        {
            serialized = false;
            _flags[2] = true;
            _switchPm = value;
        }
    }

    private Vector<BotInlineResult> _results;
    public Vector<BotInlineResult> Results
    {
        get => _results;
        set
        {
            serialized = false;
            _results = value;
        }
    }

    private int _cacheTime;
    public int CacheTime
    {
        get => _cacheTime;
        set
        {
            serialized = false;
            _cacheTime = value;
        }
    }

    private Vector<User> _users;
    public Vector<User> Users
    {
        get => _users;
        set
        {
            serialized = false;
            _users = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _queryId = buff.ReadInt64(true);
        if (_flags[1])
        {
            _nextOffset = buff.ReadTLString();
        }

        if (_flags[2])
        {
            _switchPm = (InlineBotSwitchPM)factory.Read(buff.ReadInt32(true), ref buff);
        }

        buff.Skip(4); _results  =  factory . Read < Vector < BotInlineResult > > ( ref  buff ) ; 
        _cacheTime = buff.ReadInt32(true);
        buff.Skip(4); _users  =  factory . Read < Vector < User > > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}