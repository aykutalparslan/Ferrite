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
public class UpdateReadChannelDiscussionInboxImpl : Update
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public UpdateReadChannelDiscussionInboxImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -693004986;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt64(_channelId, true);
            writer.WriteInt32(_topMsgId, true);
            writer.WriteInt32(_readMaxId, true);
            if (_flags[0])
            {
                writer.WriteInt64(_broadcastId, true);
            }

            if (_flags[0])
            {
                writer.WriteInt32(_broadcastPost, true);
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

    private long _channelId;
    public long ChannelId
    {
        get => _channelId;
        set
        {
            serialized = false;
            _channelId = value;
        }
    }

    private int _topMsgId;
    public int TopMsgId
    {
        get => _topMsgId;
        set
        {
            serialized = false;
            _topMsgId = value;
        }
    }

    private int _readMaxId;
    public int ReadMaxId
    {
        get => _readMaxId;
        set
        {
            serialized = false;
            _readMaxId = value;
        }
    }

    private long _broadcastId;
    public long BroadcastId
    {
        get => _broadcastId;
        set
        {
            serialized = false;
            _flags[0] = true;
            _broadcastId = value;
        }
    }

    private int _broadcastPost;
    public int BroadcastPost
    {
        get => _broadcastPost;
        set
        {
            serialized = false;
            _flags[0] = true;
            _broadcastPost = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _channelId = buff.ReadInt64(true);
        _topMsgId = buff.ReadInt32(true);
        _readMaxId = buff.ReadInt32(true);
        if (_flags[0])
        {
            _broadcastId = buff.ReadInt64(true);
        }

        if (_flags[0])
        {
            _broadcastPost = buff.ReadInt32(true);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}