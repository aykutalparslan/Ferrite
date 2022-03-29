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
public class SendMedia : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public SendMedia(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -497026848;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.Write(_peer.TLBytes, false);
            if (_flags[0])
            {
                writer.WriteInt32(_replyToMsgId, true);
            }

            writer.Write(_media.TLBytes, false);
            writer.WriteTLString(_message);
            writer.WriteInt64(_randomId, true);
            if (_flags[2])
            {
                writer.Write(_replyMarkup.TLBytes, false);
            }

            if (_flags[3])
            {
                writer.Write(_entities.TLBytes, false);
            }

            if (_flags[10])
            {
                writer.WriteInt32(_scheduleDate, true);
            }

            if (_flags[13])
            {
                writer.Write(_sendAs.TLBytes, false);
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

    public bool Silent
    {
        get => _flags[5];
        set
        {
            serialized = false;
            _flags[5] = value;
        }
    }

    public bool Background
    {
        get => _flags[6];
        set
        {
            serialized = false;
            _flags[6] = value;
        }
    }

    public bool ClearDraft
    {
        get => _flags[7];
        set
        {
            serialized = false;
            _flags[7] = value;
        }
    }

    public bool Noforwards
    {
        get => _flags[14];
        set
        {
            serialized = false;
            _flags[14] = value;
        }
    }

    private InputPeer _peer;
    public InputPeer Peer
    {
        get => _peer;
        set
        {
            serialized = false;
            _peer = value;
        }
    }

    private int _replyToMsgId;
    public int ReplyToMsgId
    {
        get => _replyToMsgId;
        set
        {
            serialized = false;
            _flags[0] = true;
            _replyToMsgId = value;
        }
    }

    private InputMedia _media;
    public InputMedia Media
    {
        get => _media;
        set
        {
            serialized = false;
            _media = value;
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

    private long _randomId;
    public long RandomId
    {
        get => _randomId;
        set
        {
            serialized = false;
            _randomId = value;
        }
    }

    private ReplyMarkup _replyMarkup;
    public ReplyMarkup ReplyMarkup
    {
        get => _replyMarkup;
        set
        {
            serialized = false;
            _flags[2] = true;
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
            _flags[3] = true;
            _entities = value;
        }
    }

    private int _scheduleDate;
    public int ScheduleDate
    {
        get => _scheduleDate;
        set
        {
            serialized = false;
            _flags[10] = true;
            _scheduleDate = value;
        }
    }

    private InputPeer _sendAs;
    public InputPeer SendAs
    {
        get => _sendAs;
        set
        {
            serialized = false;
            _flags[13] = true;
            _sendAs = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        buff.Skip(4); _peer  =  factory . Read < InputPeer > ( ref  buff ) ; 
        if (_flags[0])
        {
            _replyToMsgId = buff.ReadInt32(true);
        }

        buff.Skip(4); _media  =  factory . Read < InputMedia > ( ref  buff ) ; 
        _message = buff.ReadTLString();
        _randomId = buff.ReadInt64(true);
        if (_flags[2])
        {
            buff.Skip(4);
            _replyMarkup = factory.Read<ReplyMarkup>(ref buff);
        }

        if (_flags[3])
        {
            buff.Skip(4);
            _entities = factory.Read<Vector<MessageEntity>>(ref buff);
        }

        if (_flags[10])
        {
            _scheduleDate = buff.ReadInt32(true);
        }

        if (_flags[13])
        {
            buff.Skip(4);
            _sendAs = factory.Read<InputPeer>(ref buff);
        }
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}