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
public class ChannelParticipantAdminImpl : ChannelParticipant
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public ChannelParticipantAdminImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 885242707;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt64(_userId, true);
            if (_flags[1])
            {
                writer.WriteInt64(_inviterId, true);
            }

            writer.WriteInt64(_promotedBy, true);
            writer.WriteInt32(_date, true);
            writer.Write(_adminRights.TLBytes, false);
            if (_flags[2])
            {
                writer.WriteTLString(_rank);
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

    public bool CanEdit
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool Self
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
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

    private long _inviterId;
    public long InviterId
    {
        get => _inviterId;
        set
        {
            serialized = false;
            _flags[1] = true;
            _inviterId = value;
        }
    }

    private long _promotedBy;
    public long PromotedBy
    {
        get => _promotedBy;
        set
        {
            serialized = false;
            _promotedBy = value;
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

    private ChatAdminRights _adminRights;
    public ChatAdminRights AdminRights
    {
        get => _adminRights;
        set
        {
            serialized = false;
            _adminRights = value;
        }
    }

    private string _rank;
    public string Rank
    {
        get => _rank;
        set
        {
            serialized = false;
            _flags[2] = true;
            _rank = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _userId = buff.ReadInt64(true);
        if (_flags[1])
        {
            _inviterId = buff.ReadInt64(true);
        }

        _promotedBy = buff.ReadInt64(true);
        _date = buff.ReadInt32(true);
        _adminRights = (ChatAdminRights)factory.Read(buff.ReadInt32(true), ref buff);
        if (_flags[2])
        {
            _rank = buff.ReadTLString();
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}