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
public class UpdateShortChatMessageImpl : Updates
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public UpdateShortChatMessageImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 1299050149;
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
            writer.WriteInt64(_fromId, true);
            writer.WriteInt64(_chatId, true);
            writer.WriteTLString(_message);
            writer.WriteInt32(_pts, true);
            writer.WriteInt32(_ptsCount, true);
            writer.WriteInt32(_date, true);
            if (_flags[2])
            {
                writer.Write(_fwdFrom.TLBytes, false);
            }

            if (_flags[11])
            {
                writer.WriteInt64(_viaBotId, true);
            }

            if (_flags[3])
            {
                writer.Write(_replyTo.TLBytes, false);
            }

            if (_flags[7])
            {
                writer.Write(_entities.TLBytes, false);
            }

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

    private long _fromId;
    public long FromId
    {
        get => _fromId;
        set
        {
            serialized = false;
            _fromId = value;
        }
    }

    private long _chatId;
    public long ChatId
    {
        get => _chatId;
        set
        {
            serialized = false;
            _chatId = value;
        }
    }

    private string _message;
    public string Message
    {
        get => _message;
        set
        {
            serialized = false;
            _message = value;
        }
    }

    private int _pts;
    public int Pts
    {
        get => _pts;
        set
        {
            serialized = false;
            _pts = value;
        }
    }

    private int _ptsCount;
    public int PtsCount
    {
        get => _ptsCount;
        set
        {
            serialized = false;
            _ptsCount = value;
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

    private MessageFwdHeader _fwdFrom;
    public MessageFwdHeader FwdFrom
    {
        get => _fwdFrom;
        set
        {
            serialized = false;
            _flags[2] = true;
            _fwdFrom = value;
        }
    }

    private long _viaBotId;
    public long ViaBotId
    {
        get => _viaBotId;
        set
        {
            serialized = false;
            _flags[11] = true;
            _viaBotId = value;
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

    private Vector<MessageEntity> _entities;
    public Vector<MessageEntity> Entities
    {
        get => _entities;
        set
        {
            serialized = false;
            _flags[7] = true;
            _entities = value;
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
        _fromId = buff.ReadInt64(true);
        _chatId = buff.ReadInt64(true);
        _message = buff.ReadTLString();
        _pts = buff.ReadInt32(true);
        _ptsCount = buff.ReadInt32(true);
        _date = buff.ReadInt32(true);
        if (_flags[2])
        {
            buff.Skip(4);
            _fwdFrom = factory.Read<MessageFwdHeader>(ref buff);
        }

        if (_flags[11])
        {
            _viaBotId = buff.ReadInt64(true);
        }

        if (_flags[3])
        {
            buff.Skip(4);
            _replyTo = factory.Read<MessageReplyHeader>(ref buff);
        }

        if (_flags[7])
        {
            buff.Skip(4);
            _entities = factory.Read<Vector<MessageEntity>>(ref buff);
        }

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