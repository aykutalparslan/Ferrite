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
public class DiscussionMessageImpl : DiscussionMessage
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public DiscussionMessageImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1506535550;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.Write(_messages.TLBytes, false);
            if (_flags[0])
            {
                writer.WriteInt32(_maxId, true);
            }

            if (_flags[1])
            {
                writer.WriteInt32(_readInboxMaxId, true);
            }

            if (_flags[2])
            {
                writer.WriteInt32(_readOutboxMaxId, true);
            }

            writer.WriteInt32(_unreadCount, true);
            writer.Write(_chats.TLBytes, false);
            writer.Write(_users.TLBytes, false);
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

    private Vector<Message> _messages;
    public Vector<Message> Messages
    {
        get => _messages;
        set
        {
            serialized = false;
            _messages = value;
        }
    }

    private int _maxId;
    public int MaxId
    {
        get => _maxId;
        set
        {
            serialized = false;
            _flags[0] = true;
            _maxId = value;
        }
    }

    private int _readInboxMaxId;
    public int ReadInboxMaxId
    {
        get => _readInboxMaxId;
        set
        {
            serialized = false;
            _flags[1] = true;
            _readInboxMaxId = value;
        }
    }

    private int _readOutboxMaxId;
    public int ReadOutboxMaxId
    {
        get => _readOutboxMaxId;
        set
        {
            serialized = false;
            _flags[2] = true;
            _readOutboxMaxId = value;
        }
    }

    private int _unreadCount;
    public int UnreadCount
    {
        get => _unreadCount;
        set
        {
            serialized = false;
            _unreadCount = value;
        }
    }

    private Vector<Chat> _chats;
    public Vector<Chat> Chats
    {
        get => _chats;
        set
        {
            serialized = false;
            _chats = value;
        }
    }

    private Vector<User> _users;
    public Vector<User> Users
    {
        get => _users;
        set
        {
            serialized = false;
            _users = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        buff.Skip(4); _messages  =  factory . Read < Vector < Message > > ( ref  buff ) ; 
        if (_flags[0])
        {
            _maxId = buff.ReadInt32(true);
        }

        if (_flags[1])
        {
            _readInboxMaxId = buff.ReadInt32(true);
        }

        if (_flags[2])
        {
            _readOutboxMaxId = buff.ReadInt32(true);
        }

        _unreadCount = buff.ReadInt32(true);
        buff.Skip(4); _chats  =  factory . Read < Vector < Chat > > ( ref  buff ) ; 
        buff.Skip(4); _users  =  factory . Read < Vector < User > > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}