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
public class UpdateBotInlineQueryImpl : Update
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public UpdateBotInlineQueryImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 1232025500;
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
            writer.WriteInt64(_userId, true);
            writer.WriteTLString(_query);
            if (_flags[0])
            {
                writer.Write(_geo.TLBytes, false);
            }

            if (_flags[1])
            {
                writer.Write(_peerType.TLBytes, false);
            }

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

    private long _userId;
    public long UserId
    {
        get => _userId;
        set
        {
            serialized = false;
            _userId = value;
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

    private GeoPoint _geo;
    public GeoPoint Geo
    {
        get => _geo;
        set
        {
            serialized = false;
            _flags[0] = true;
            _geo = value;
        }
    }

    private InlineQueryPeerType _peerType;
    public InlineQueryPeerType PeerType
    {
        get => _peerType;
        set
        {
            serialized = false;
            _flags[1] = true;
            _peerType = value;
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

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _queryId = buff.ReadInt64(true);
        _userId = buff.ReadInt64(true);
        _query = buff.ReadTLString();
        if (_flags[0])
        {
            _geo = (GeoPoint)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[1])
        {
            _peerType = (InlineQueryPeerType)factory.Read(buff.ReadInt32(true), ref buff);
        }

        _offset = buff.ReadTLString();
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}