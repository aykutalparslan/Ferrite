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
public class InputAppEventImpl : InputAppEvent
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public InputAppEventImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 488313413;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64((long)_time, true);
            writer.WriteTLString(_type);
            writer.WriteInt64(_peer, true);
            writer.Write(_data.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private double _time;
    public double Time
    {
        get => _time;
        set
        {
            serialized = false;
            _time = value;
        }
    }

    private string _type;
    public string Type
    {
        get => _type;
        set
        {
            serialized = false;
            _type = value;
        }
    }

    private long _peer;
    public long Peer
    {
        get => _peer;
        set
        {
            serialized = false;
            _peer = value;
        }
    }

    private JSONValue _data;
    public JSONValue Data
    {
        get => _data;
        set
        {
            serialized = false;
            _data = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _time = buff.ReadInt64(true);
        _type = buff.ReadTLString();
        _peer = buff.ReadInt64(true);
        buff.Skip(4); _data  =  factory . Read < JSONValue > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}