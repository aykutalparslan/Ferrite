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
public class InputGroupCallStreamImpl : InputFileLocation
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public InputGroupCallStreamImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 93890858;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.Write(_call.TLBytes, false);
            writer.WriteInt64(_timeMs, true);
            writer.WriteInt32(_scale, true);
            if (_flags[0])
            {
                writer.WriteInt32(_videoChannel, true);
            }

            if (_flags[0])
            {
                writer.WriteInt32(_videoQuality, true);
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

    private InputGroupCall _call;
    public InputGroupCall Call
    {
        get => _call;
        set
        {
            serialized = false;
            _call = value;
        }
    }

    private long _timeMs;
    public long TimeMs
    {
        get => _timeMs;
        set
        {
            serialized = false;
            _timeMs = value;
        }
    }

    private int _scale;
    public int Scale
    {
        get => _scale;
        set
        {
            serialized = false;
            _scale = value;
        }
    }

    private int _videoChannel;
    public int VideoChannel
    {
        get => _videoChannel;
        set
        {
            serialized = false;
            _flags[0] = true;
            _videoChannel = value;
        }
    }

    private int _videoQuality;
    public int VideoQuality
    {
        get => _videoQuality;
        set
        {
            serialized = false;
            _flags[0] = true;
            _videoQuality = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        buff.Skip(4); _call  =  factory . Read < InputGroupCall > ( ref  buff ) ; 
        _timeMs = buff.ReadInt64(true);
        _scale = buff.ReadInt32(true);
        if (_flags[0])
        {
            _videoChannel = buff.ReadInt32(true);
        }

        if (_flags[0])
        {
            _videoQuality = buff.ReadInt32(true);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}