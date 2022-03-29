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
public class GeoPointImpl : GeoPoint
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public GeoPointImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1297942941;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt64((long)_long, true);
            writer.WriteInt64((long)_lat, true);
            writer.WriteInt64(_accessHash, true);
            if (_flags[0])
            {
                writer.WriteInt32(_accuracyRadius, true);
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

    private double _long;
    public double Long
    {
        get => _long;
        set
        {
            serialized = false;
            _long = value;
        }
    }

    private double _lat;
    public double Lat
    {
        get => _lat;
        set
        {
            serialized = false;
            _lat = value;
        }
    }

    private long _accessHash;
    public long AccessHash
    {
        get => _accessHash;
        set
        {
            serialized = false;
            _accessHash = value;
        }
    }

    private int _accuracyRadius;
    public int AccuracyRadius
    {
        get => _accuracyRadius;
        set
        {
            serialized = false;
            _flags[0] = true;
            _accuracyRadius = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _long = buff.ReadInt64(true);
        _lat = buff.ReadInt64(true);
        _accessHash = buff.ReadInt64(true);
        if (_flags[0])
        {
            _accuracyRadius = buff.ReadInt32(true);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}