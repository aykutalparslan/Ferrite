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
public class DialogImpl : Dialog
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public DialogImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1460809483;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.Write(_peer.TLBytes, false);
            writer.WriteInt32(_topMessage, true);
            writer.WriteInt32(_readInboxMaxId, true);
            writer.WriteInt32(_readOutboxMaxId, true);
            writer.WriteInt32(_unreadCount, true);
            writer.WriteInt32(_unreadMentionsCount, true);
            writer.WriteInt32(_unreadReactionsCount, true);
            writer.Write(_notifySettings.TLBytes, false);
            if (_flags[0])
            {
                writer.WriteInt32(_pts, true);
            }

            if (_flags[1])
            {
                writer.Write(_draft.TLBytes, false);
            }

            if (_flags[4])
            {
                writer.WriteInt32(_folderId, true);
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

    public bool Pinned
    {
        get => _flags[2];
        set
        {
            serialized = false;
            _flags[2] = value;
        }
    }

    public bool UnreadMark
    {
        get => _flags[3];
        set
        {
            serialized = false;
            _flags[3] = value;
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

    private int _readInboxMaxId;
    public int ReadInboxMaxId
    {
        get => _readInboxMaxId;
        set
        {
            serialized = false;
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

    private int _unreadMentionsCount;
    public int UnreadMentionsCount
    {
        get => _unreadMentionsCount;
        set
        {
            serialized = false;
            _unreadMentionsCount = value;
        }
    }

    private int _unreadReactionsCount;
    public int UnreadReactionsCount
    {
        get => _unreadReactionsCount;
        set
        {
            serialized = false;
            _unreadReactionsCount = value;
        }
    }

    private PeerNotifySettings _notifySettings;
    public PeerNotifySettings NotifySettings
    {
        get => _notifySettings;
        set
        {
            serialized = false;
            _notifySettings = value;
        }
    }

    private int _pts;
    public int Pts
    {
        get => _pts;
        set
        {
            serialized = false;
            _flags[0] = true;
            _pts = value;
        }
    }

    private DraftMessage _draft;
    public DraftMessage Draft
    {
        get => _draft;
        set
        {
            serialized = false;
            _flags[1] = true;
            _draft = value;
        }
    }

    private int _folderId;
    public int FolderId
    {
        get => _folderId;
        set
        {
            serialized = false;
            _flags[4] = true;
            _folderId = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _peer = (Peer)factory.Read(buff.ReadInt32(true), ref buff);
        _topMessage = buff.ReadInt32(true);
        _readInboxMaxId = buff.ReadInt32(true);
        _readOutboxMaxId = buff.ReadInt32(true);
        _unreadCount = buff.ReadInt32(true);
        _unreadMentionsCount = buff.ReadInt32(true);
        _unreadReactionsCount = buff.ReadInt32(true);
        _notifySettings = (PeerNotifySettings)factory.Read(buff.ReadInt32(true), ref buff);
        if (_flags[0])
        {
            _pts = buff.ReadInt32(true);
        }

        if (_flags[1])
        {
            _draft = (DraftMessage)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[4])
        {
            _folderId = buff.ReadInt32(true);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}