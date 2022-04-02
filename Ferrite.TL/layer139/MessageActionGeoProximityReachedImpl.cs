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
public class MessageActionGeoProximityReachedImpl : MessageAction
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public MessageActionGeoProximityReachedImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1730095465;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_fromId.TLBytes, false);
            writer.Write(_toId.TLBytes, false);
            writer.WriteInt32(_distance, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private Peer _fromId;
    public Peer FromId
    {
        get => _fromId;
        set
        {
            serialized = false;
            _fromId = value;
        }
    }

    private Peer _toId;
    public Peer ToId
    {
        get => _toId;
        set
        {
            serialized = false;
            _toId = value;
        }
    }

    private int _distance;
    public int Distance
    {
        get => _distance;
        set
        {
            serialized = false;
            _distance = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _fromId = (Peer)factory.Read(buff.ReadInt32(true), ref buff);
        _toId = (Peer)factory.Read(buff.ReadInt32(true), ref buff);
        _distance = buff.ReadInt32(true);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}