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
public class MessageRepliesImpl : MessageReplies
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public MessageRepliesImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -2083123262;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt32(_replies, true);
            writer.WriteInt32(_repliesPts, true);
            if (_flags[1])
            {
                writer.Write(_recentRepliers.TLBytes, false);
            }

            if (_flags[0])
            {
                writer.WriteInt64(_channelId, true);
            }

            if (_flags[2])
            {
                writer.WriteInt32(_maxId, true);
            }

            if (_flags[3])
            {
                writer.WriteInt32(_readMaxId, true);
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

    public bool Comments
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    private int _replies;
    public int Replies
    {
        get => _replies;
        set
        {
            serialized = false;
            _replies = value;
        }
    }

    private int _repliesPts;
    public int RepliesPts
    {
        get => _repliesPts;
        set
        {
            serialized = false;
            _repliesPts = value;
        }
    }

    private Vector<Peer> _recentRepliers;
    public Vector<Peer> RecentRepliers
    {
        get => _recentRepliers;
        set
        {
            serialized = false;
            _flags[1] = true;
            _recentRepliers = value;
        }
    }

    private long _channelId;
    public long ChannelId
    {
        get => _channelId;
        set
        {
            serialized = false;
            _flags[0] = true;
            _channelId = value;
        }
    }

    private int _maxId;
    public int MaxId
    {
        get => _maxId;
        set
        {
            serialized = false;
            _flags[2] = true;
            _maxId = value;
        }
    }

    private int _readMaxId;
    public int ReadMaxId
    {
        get => _readMaxId;
        set
        {
            serialized = false;
            _flags[3] = true;
            _readMaxId = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _replies = buff.ReadInt32(true);
        _repliesPts = buff.ReadInt32(true);
        if (_flags[1])
        {
            buff.Skip(4);
            _recentRepliers = factory.Read<Vector<Peer>>(ref buff);
        }

        if (_flags[0])
        {
            _channelId = buff.ReadInt64(true);
        }

        if (_flags[2])
        {
            _maxId = buff.ReadInt32(true);
        }

        if (_flags[3])
        {
            _readMaxId = buff.ReadInt32(true);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}