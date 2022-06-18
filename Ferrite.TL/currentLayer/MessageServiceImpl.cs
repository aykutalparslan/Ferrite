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
public class MessageServiceImpl : Message
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public MessageServiceImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 721967202;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt32(_id, true);
            if (_flags[8])
            {
                writer.Write(_fromId.TLBytes, false);
            }

            writer.Write(_peerId.TLBytes, false);
            if (_flags[3])
            {
                writer.Write(_replyTo.TLBytes, false);
            }

            writer.WriteInt32(_date, true);
            writer.Write(_action.TLBytes, false);
            if (_flags[25])
            {
                writer.WriteInt32(_ttlPeriod, true);
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

    public bool Out
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
        }
    }

    public bool Mentioned
    {
        get => _flags[4];
        set
        {
            serialized = false;
            _flags[4] = value;
        }
    }

    public bool MediaUnread
    {
        get => _flags[5];
        set
        {
            serialized = false;
            _flags[5] = value;
        }
    }

    public bool Silent
    {
        get => _flags[13];
        set
        {
            serialized = false;
            _flags[13] = value;
        }
    }

    public bool Post
    {
        get => _flags[14];
        set
        {
            serialized = false;
            _flags[14] = value;
        }
    }

    public bool Legacy
    {
        get => _flags[19];
        set
        {
            serialized = false;
            _flags[19] = value;
        }
    }

    private int _id;
    public int Id
    {
        get => _id;
        set
        {
            serialized = false;
            _id = value;
        }
    }

    private Peer _fromId;
    public Peer FromId
    {
        get => _fromId;
        set
        {
            serialized = false;
            _flags[8] = true;
            _fromId = value;
        }
    }

    private Peer _peerId;
    public Peer PeerId
    {
        get => _peerId;
        set
        {
            serialized = false;
            _peerId = value;
        }
    }

    private MessageReplyHeader _replyTo;
    public MessageReplyHeader ReplyTo
    {
        get => _replyTo;
        set
        {
            serialized = false;
            _flags[3] = true;
            _replyTo = value;
        }
    }

    private int _date;
    public int Date
    {
        get => _date;
        set
        {
            serialized = false;
            _date = value;
        }
    }

    private MessageAction _action;
    public MessageAction Action
    {
        get => _action;
        set
        {
            serialized = false;
            _action = value;
        }
    }

    private int _ttlPeriod;
    public int TtlPeriod
    {
        get => _ttlPeriod;
        set
        {
            serialized = false;
            _flags[25] = true;
            _ttlPeriod = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _id = buff.ReadInt32(true);
        if (_flags[8])
        {
            _fromId = (Peer)factory.Read(buff.ReadInt32(true), ref buff);
        }

        _peerId = (Peer)factory.Read(buff.ReadInt32(true), ref buff);
        if (_flags[3])
        {
            _replyTo = (MessageReplyHeader)factory.Read(buff.ReadInt32(true), ref buff);
        }

        _date = buff.ReadInt32(true);
        _action = (MessageAction)factory.Read(buff.ReadInt32(true), ref buff);
        if (_flags[25])
        {
            _ttlPeriod = buff.ReadInt32(true);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}