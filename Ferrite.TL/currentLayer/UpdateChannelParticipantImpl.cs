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
public class UpdateChannelParticipantImpl : Update
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public UpdateChannelParticipantImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1738720581;
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
            writer.WriteInt32(_date, true);
            writer.WriteInt64(_actorId, true);
            writer.WriteInt64(_userId, true);
            if (_flags[0])
            {
                writer.Write(_prevParticipant.TLBytes, false);
            }

            if (_flags[1])
            {
                writer.Write(_newParticipant.TLBytes, false);
            }

            if (_flags[2])
            {
                writer.Write(_invite.TLBytes, false);
            }

            writer.WriteInt32(_qts, true);
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

    private long _actorId;
    public long ActorId
    {
        get => _actorId;
        set
        {
            serialized = false;
            _actorId = value;
        }
    }

    private long _userId;
    public long UserId
    {
        get => _userId;
        set
        {
            serialized = false;
            _userId = value;
        }
    }

    private ChannelParticipant _prevParticipant;
    public ChannelParticipant PrevParticipant
    {
        get => _prevParticipant;
        set
        {
            serialized = false;
            _flags[0] = true;
            _prevParticipant = value;
        }
    }

    private ChannelParticipant _newParticipant;
    public ChannelParticipant NewParticipant
    {
        get => _newParticipant;
        set
        {
            serialized = false;
            _flags[1] = true;
            _newParticipant = value;
        }
    }

    private ExportedChatInvite _invite;
    public ExportedChatInvite Invite
    {
        get => _invite;
        set
        {
            serialized = false;
            _flags[2] = true;
            _invite = value;
        }
    }

    private int _qts;
    public int Qts
    {
        get => _qts;
        set
        {
            serialized = false;
            _qts = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _channelId = buff.ReadInt64(true);
        _date = buff.ReadInt32(true);
        _actorId = buff.ReadInt64(true);
        _userId = buff.ReadInt64(true);
        if (_flags[0])
        {
            _prevParticipant = (ChannelParticipant)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[1])
        {
            _newParticipant = (ChannelParticipant)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[2])
        {
            _invite = (ExportedChatInvite)factory.Read(buff.ReadInt32(true), ref buff);
        }

        _qts = buff.ReadInt32(true);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}