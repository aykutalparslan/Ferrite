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
public class MessageMediaGeoLiveImpl : MessageMedia
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public MessageMediaGeoLiveImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1186937242;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.Write(_geo.TLBytes, false);
            if (_flags[0])
            {
                writer.WriteInt32(_heading, true);
            }

            writer.WriteInt32(_period, true);
            if (_flags[1])
            {
                writer.WriteInt32(_proximityNotificationRadius, true);
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

    private GeoPoint _geo;
    public GeoPoint Geo
    {
        get => _geo;
        set
        {
            serialized = false;
            _geo = value;
        }
    }

    private int _heading;
    public int Heading
    {
        get => _heading;
        set
        {
            serialized = false;
            _flags[0] = true;
            _heading = value;
        }
    }

    private int _period;
    public int Period
    {
        get => _period;
        set
        {
            serialized = false;
            _period = value;
        }
    }

    private int _proximityNotificationRadius;
    public int ProximityNotificationRadius
    {
        get => _proximityNotificationRadius;
        set
        {
            serialized = false;
            _flags[1] = true;
            _proximityNotificationRadius = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _geo = (GeoPoint)factory.Read(buff.ReadInt32(true), ref buff);
        if (_flags[0])
        {
            _heading = buff.ReadInt32(true);
        }

        _period = buff.ReadInt32(true);
        if (_flags[1])
        {
            _proximityNotificationRadius = buff.ReadInt32(true);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}