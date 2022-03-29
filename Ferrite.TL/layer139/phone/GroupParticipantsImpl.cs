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

namespace Ferrite.TL.layer139.phone;
public class GroupParticipantsImpl : GroupParticipants
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public GroupParticipantsImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -193506890;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt32(_count, true);
            writer.Write(_participants.TLBytes, false);
            writer.WriteTLString(_nextOffset);
            writer.Write(_chats.TLBytes, false);
            writer.Write(_users.TLBytes, false);
            writer.WriteInt32(_version, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private int _count;
    public int Count
    {
        get => _count;
        set
        {
            serialized = false;
            _count = value;
        }
    }

    private Vector<GroupCallParticipant> _participants;
    public Vector<GroupCallParticipant> Participants
    {
        get => _participants;
        set
        {
            serialized = false;
            _participants = value;
        }
    }

    private string _nextOffset;
    public string NextOffset
    {
        get => _nextOffset;
        set
        {
            serialized = false;
            _nextOffset = value;
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

    private int _version;
    public int Version
    {
        get => _version;
        set
        {
            serialized = false;
            _version = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _count = buff.ReadInt32(true);
        buff.Skip(4); _participants  =  factory . Read < Vector < GroupCallParticipant > > ( ref  buff ) ; 
        _nextOffset = buff.ReadTLString();
        buff.Skip(4); _chats  =  factory . Read < Vector < Chat > > ( ref  buff ) ; 
        buff.Skip(4); _users  =  factory . Read < Vector < User > > ( ref  buff ) ; 
        _version = buff.ReadInt32(true);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}