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
public class MessageImpl : Message
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public MessageImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 940666592;
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

            writer.WriteInt32(_date, true);
            writer.WriteTLString(_message);
            if (_flags[9])
            {
                writer.Write(_media.TLBytes, false);
            }

            if (_flags[6])
            {
                writer.Write(_replyMarkup.TLBytes, false);
            }

            if (_flags[7])
            {
                writer.Write(_entities.TLBytes, false);
            }

            if (_flags[10])
            {
                writer.WriteInt32(_views, true);
            }

            if (_flags[10])
            {
                writer.WriteInt32(_forwards, true);
            }

            if (_flags[23])
            {
                writer.Write(_replies.TLBytes, false);
            }

            if (_flags[15])
            {
                writer.WriteInt32(_editDate, true);
            }

            if (_flags[16])
            {
                writer.WriteTLString(_postAuthor);
            }

            if (_flags[17])
            {
                writer.WriteInt64(_groupedId, true);
            }

            if (_flags[20])
            {
                writer.Write(_reactions.TLBytes, false);
            }

            if (_flags[22])
            {
                writer.Write(_restrictionReason.TLBytes, false);
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

    public bool Post
    {
        get => _flags[14];
        set
        {
            serialized = false;
            _flags[14] = value;
        }
    }

    public bool FromScheduled
    {
        get => _flags[18];
        set
        {
            serialized = false;
            _flags[18] = value;
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

    public bool EditHide
    {
        get => _flags[21];
        set
        {
            serialized = false;
            _flags[21] = value;
        }
    }

    public bool Pinned
    {
        get => _flags[24];
        set
        {
            serialized = false;
            _flags[24] = value;
        }
    }

    public bool Noforwards
    {
        get => _flags[26];
        set
        {
            serialized = false;
            _flags[26] = value;
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

    private MessageMedia _media;
    public MessageMedia Media
    {
        get => _media;
        set
        {
            serialized = false;
            _flags[9] = true;
            _media = value;
        }
    }

    private ReplyMarkup _replyMarkup;
    public ReplyMarkup ReplyMarkup
    {
        get => _replyMarkup;
        set
        {
            serialized = false;
            _flags[6] = true;
            _replyMarkup = value;
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

    private int _views;
    public int Views
    {
        get => _views;
        set
        {
            serialized = false;
            _flags[10] = true;
            _views = value;
        }
    }

    private int _forwards;
    public int Forwards
    {
        get => _forwards;
        set
        {
            serialized = false;
            _flags[10] = true;
            _forwards = value;
        }
    }

    private MessageReplies _replies;
    public MessageReplies Replies
    {
        get => _replies;
        set
        {
            serialized = false;
            _flags[23] = true;
            _replies = value;
        }
    }

    private int _editDate;
    public int EditDate
    {
        get => _editDate;
        set
        {
            serialized = false;
            _flags[15] = true;
            _editDate = value;
        }
    }

    private string _postAuthor;
    public string PostAuthor
    {
        get => _postAuthor;
        set
        {
            serialized = false;
            _flags[16] = true;
            _postAuthor = value;
        }
    }

    private long _groupedId;
    public long GroupedId
    {
        get => _groupedId;
        set
        {
            serialized = false;
            _flags[17] = true;
            _groupedId = value;
        }
    }

    private MessageReactions _reactions;
    public MessageReactions Reactions
    {
        get => _reactions;
        set
        {
            serialized = false;
            _flags[20] = true;
            _reactions = value;
        }
    }

    private Vector<RestrictionReason> _restrictionReason;
    public Vector<RestrictionReason> RestrictionReason
    {
        get => _restrictionReason;
        set
        {
            serialized = false;
            _flags[22] = true;
            _restrictionReason = value;
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
        if (_flags[2])
        {
            _fwdFrom = (MessageFwdHeader)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[11])
        {
            _viaBotId = buff.ReadInt64(true);
        }

        if (_flags[3])
        {
            _replyTo = (MessageReplyHeader)factory.Read(buff.ReadInt32(true), ref buff);
        }

        _date = buff.ReadInt32(true);
        _message = buff.ReadTLString();
        if (_flags[9])
        {
            _media = (MessageMedia)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[6])
        {
            _replyMarkup = (ReplyMarkup)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[7])
        {
            buff.Skip(4);
            _entities = factory.Read<Vector<MessageEntity>>(ref buff);
        }

        if (_flags[10])
        {
            _views = buff.ReadInt32(true);
        }

        if (_flags[10])
        {
            _forwards = buff.ReadInt32(true);
        }

        if (_flags[23])
        {
            _replies = (MessageReplies)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[15])
        {
            _editDate = buff.ReadInt32(true);
        }

        if (_flags[16])
        {
            _postAuthor = buff.ReadTLString();
        }

        if (_flags[17])
        {
            _groupedId = buff.ReadInt64(true);
        }

        if (_flags[20])
        {
            _reactions = (MessageReactions)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[22])
        {
            buff.Skip(4);
            _restrictionReason = factory.Read<Vector<RestrictionReason>>(ref buff);
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