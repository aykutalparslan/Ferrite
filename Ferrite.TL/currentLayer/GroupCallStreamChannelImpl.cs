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

namespace Ferrite.TL.currentLayer;
public class GroupCallStreamChannelImpl : GroupCallStreamChannel
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public GroupCallStreamChannelImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -2132064081;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt32(_channel, true);
            writer.WriteInt32(_scale, true);
            writer.WriteInt64(_lastTimestampMs, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private int _channel;
    public int Channel
    {
        get => _channel;
        set
        {
            serialized = false;
            _channel = value;
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

    private long _lastTimestampMs;
    public long LastTimestampMs
    {
        get => _lastTimestampMs;
        set
        {
            serialized = false;
            _lastTimestampMs = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _channel = buff.ReadInt32(true);
        _scale = buff.ReadInt32(true);
        _lastTimestampMs = buff.ReadInt64(true);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}