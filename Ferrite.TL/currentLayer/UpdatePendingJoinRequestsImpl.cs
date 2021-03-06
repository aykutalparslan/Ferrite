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
public class UpdatePendingJoinRequestsImpl : Update
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public UpdatePendingJoinRequestsImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 1885586395;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_peer.TLBytes, false);
            writer.WriteInt32(_requestsPending, true);
            writer.Write(_recentRequesters.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private Peer _peer;
    public Peer Peer
    {
        get => _peer;
        set
        {
            serialized = false;
            _peer = value;
        }
    }

    private int _requestsPending;
    public int RequestsPending
    {
        get => _requestsPending;
        set
        {
            serialized = false;
            _requestsPending = value;
        }
    }

    private VectorOfLong _recentRequesters;
    public VectorOfLong RecentRequesters
    {
        get => _recentRequesters;
        set
        {
            serialized = false;
            _recentRequesters = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _peer = (Peer)factory.Read(buff.ReadInt32(true), ref buff);
        _requestsPending = buff.ReadInt32(true);
        buff.Skip(4); _recentRequesters  =  factory . Read < VectorOfLong > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}