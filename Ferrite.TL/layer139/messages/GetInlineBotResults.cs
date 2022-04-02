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
public class GetInlineBotResults : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public GetInlineBotResults(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => 1364105629;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.Write(_bot.TLBytes, false);
            writer.Write(_peer.TLBytes, false);
            if (_flags[0])
            {
                writer.Write(_geoPoint.TLBytes, false);
            }

            writer.WriteTLString(_query);
            writer.WriteTLString(_offset);
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

    private InputUser _bot;
    public InputUser Bot
    {
        get => _bot;
        set
        {
            serialized = false;
            _bot = value;
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

    private InputGeoPoint _geoPoint;
    public InputGeoPoint GeoPoint
    {
        get => _geoPoint;
        set
        {
            serialized = false;
            _flags[0] = true;
            _geoPoint = value;
        }
    }

    private string _query;
    public string Query
    {
        get => _query;
        set
        {
            serialized = false;
            _query = value;
        }
    }

    private string _offset;
    public string Offset
    {
        get => _offset;
        set
        {
            serialized = false;
            _offset = value;
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
        _bot = (InputUser)factory.Read(buff.ReadInt32(true), ref buff);
        _peer = (InputPeer)factory.Read(buff.ReadInt32(true), ref buff);
        if (_flags[0])
        {
            _geoPoint = (InputGeoPoint)factory.Read(buff.ReadInt32(true), ref buff);
        }

        _query = buff.ReadTLString();
        _offset = buff.ReadTLString();
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}