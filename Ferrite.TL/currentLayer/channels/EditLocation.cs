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

namespace Ferrite.TL.currentLayer.channels;
public class EditLocation : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public EditLocation(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => 1491484525;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_channel.TLBytes, false);
            writer.Write(_geoPoint.TLBytes, false);
            writer.WriteTLString(_address);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private InputChannel _channel;
    public InputChannel Channel
    {
        get => _channel;
        set
        {
            serialized = false;
            _channel = value;
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

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _channel = (InputChannel)factory.Read(buff.ReadInt32(true), ref buff);
        _geoPoint = (InputGeoPoint)factory.Read(buff.ReadInt32(true), ref buff);
        _address = buff.ReadTLString();
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}