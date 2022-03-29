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
public class DialogFolderImpl : Dialog
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public DialogFolderImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 1908216652;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.Write(_folder.TLBytes, false);
            writer.Write(_peer.TLBytes, false);
            writer.WriteInt32(_topMessage, true);
            writer.WriteInt32(_unreadMutedPeersCount, true);
            writer.WriteInt32(_unreadUnmutedPeersCount, true);
            writer.WriteInt32(_unreadMutedMessagesCount, true);
            writer.WriteInt32(_unreadUnmutedMessagesCount, true);
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

    public bool Pinned
    {
        get => _flags[2];
        set
        {
            serialized = false;
            _flags[2] = value;
        }
    }

    private Folder _folder;
    public Folder Folder
    {
        get => _folder;
        set
        {
            serialized = false;
            _folder = value;
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

    private int _topMessage;
    public int TopMessage
    {
        get => _topMessage;
        set
        {
            serialized = false;
            _topMessage = value;
        }
    }

    private int _unreadMutedPeersCount;
    public int UnreadMutedPeersCount
    {
        get => _unreadMutedPeersCount;
        set
        {
            serialized = false;
            _unreadMutedPeersCount = value;
        }
    }

    private int _unreadUnmutedPeersCount;
    public int UnreadUnmutedPeersCount
    {
        get => _unreadUnmutedPeersCount;
        set
        {
            serialized = false;
            _unreadUnmutedPeersCount = value;
        }
    }

    private int _unreadMutedMessagesCount;
    public int UnreadMutedMessagesCount
    {
        get => _unreadMutedMessagesCount;
        set
        {
            serialized = false;
            _unreadMutedMessagesCount = value;
        }
    }

    private int _unreadUnmutedMessagesCount;
    public int UnreadUnmutedMessagesCount
    {
        get => _unreadUnmutedMessagesCount;
        set
        {
            serialized = false;
            _unreadUnmutedMessagesCount = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        buff.Skip(4); _folder  =  factory . Read < Folder > ( ref  buff ) ; 
        buff.Skip(4); _peer  =  factory . Read < Peer > ( ref  buff ) ; 
        _topMessage = buff.ReadInt32(true);
        _unreadMutedPeersCount = buff.ReadInt32(true);
        _unreadUnmutedPeersCount = buff.ReadInt32(true);
        _unreadMutedMessagesCount = buff.ReadInt32(true);
        _unreadUnmutedMessagesCount = buff.ReadInt32(true);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}