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
public class InputMediaVenueImpl : InputMedia
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public InputMediaVenueImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1052959727;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_geoPoint.TLBytes, false);
            writer.WriteTLString(_title);
            writer.WriteTLString(_address);
            writer.WriteTLString(_provider);
            writer.WriteTLString(_venueId);
            writer.WriteTLString(_venueType);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private InputGeoPoint _geoPoint;
    public InputGeoPoint GeoPoint
    {
        get => _geoPoint;
        set
        {
            serialized = false;
            _geoPoint = value;
        }
    }

    private string _title;
    public string Title
    {
        get => _title;
        set
        {
            serialized = false;
            _title = value;
        }
    }

    private string _address;
    public string Address
    {
        get => _address;
        set
        {
            serialized = false;
            _address = value;
        }
    }

    private string _provider;
    public string Provider
    {
        get => _provider;
        set
        {
            serialized = false;
            _provider = value;
        }
    }

    private string _venueId;
    public string VenueId
    {
        get => _venueId;
        set
        {
            serialized = false;
            _venueId = value;
        }
    }

    private string _venueType;
    public string VenueType
    {
        get => _venueType;
        set
        {
            serialized = false;
            _venueType = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _geoPoint = (InputGeoPoint)factory.Read(buff.ReadInt32(true), ref buff);
        _title = buff.ReadTLString();
        _address = buff.ReadTLString();
        _provider = buff.ReadTLString();
        _venueId = buff.ReadTLString();
        _venueType = buff.ReadTLString();
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}