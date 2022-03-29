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

namespace Ferrite.TL.layer139.messages;
public class SetInlineBotResults : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public SetInlineBotResults(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -346119674;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt64(_queryId, true);
            writer.Write(_results.TLBytes, false);
            writer.WriteInt32(_cacheTime, true);
            if (_flags[2])
            {
                writer.WriteTLString(_nextOffset);
            }

            if (_flags[3])
            {
                writer.Write(_switchPm.TLBytes, false);
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

    public bool Gallery
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool Private
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
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

    private Vector<InputBotInlineResult> _results;
    public Vector<InputBotInlineResult> Results
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

    private string _nextOffset;
    public string NextOffset
    {
        get => _nextOffset;
        set
        {
            serialized = false;
            _flags[2] = true;
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
            _flags[3] = true;
            _switchPm = value;
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
        _queryId = buff.ReadInt64(true);
        buff.Skip(4); _results  =  factory . Read < Vector < InputBotInlineResult > > ( ref  buff ) ; 
        _cacheTime = buff.ReadInt32(true);
        if (_flags[2])
        {
            _nextOffset = buff.ReadTLString();
        }

        if (_flags[3])
        {
            buff.Skip(4);
            _switchPm = factory.Read<InlineBotSwitchPM>(ref buff);
        }
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}