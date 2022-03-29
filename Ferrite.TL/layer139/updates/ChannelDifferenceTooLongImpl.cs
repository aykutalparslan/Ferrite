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

namespace Ferrite.TL.layer139.updates;
public class ChannelDifferenceTooLongImpl : ChannelDifference
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public ChannelDifferenceTooLongImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1531132162;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            if (_flags[1])
            {
                writer.WriteInt32(_timeout, true);
            }

            writer.Write(_dialog.TLBytes, false);
            writer.Write(_messages.TLBytes, false);
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

    public bool Final
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    private int _timeout;
    public int Timeout
    {
        get => _timeout;
        set
        {
            serialized = false;
            _flags[1] = true;
            _timeout = value;
        }
    }

    private Dialog _dialog;
    public Dialog Dialog
    {
        get => _dialog;
        set
        {
            serialized = false;
            _dialog = value;
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
        if (_flags[1])
        {
            _timeout = buff.ReadInt32(true);
        }

        buff.Skip(4); _dialog  =  factory . Read < Dialog > ( ref  buff ) ; 
        buff.Skip(4); _messages  =  factory . Read < Vector < Message > > ( ref  buff ) ; 
        buff.Skip(4); _chats  =  factory . Read < Vector < Chat > > ( ref  buff ) ; 
        buff.Skip(4); _users  =  factory . Read < Vector < User > > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}