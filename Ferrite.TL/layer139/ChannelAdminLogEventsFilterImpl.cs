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
public class ChannelAdminLogEventsFilterImpl : ChannelAdminLogEventsFilter
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public ChannelAdminLogEventsFilterImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -368018716;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
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

    public bool Join
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool Leave
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
        }
    }

    public bool Invite
    {
        get => _flags[2];
        set
        {
            serialized = false;
            _flags[2] = value;
        }
    }

    public bool Ban
    {
        get => _flags[3];
        set
        {
            serialized = false;
            _flags[3] = value;
        }
    }

    public bool Unban
    {
        get => _flags[4];
        set
        {
            serialized = false;
            _flags[4] = value;
        }
    }

    public bool Kick
    {
        get => _flags[5];
        set
        {
            serialized = false;
            _flags[5] = value;
        }
    }

    public bool Unkick
    {
        get => _flags[6];
        set
        {
            serialized = false;
            _flags[6] = value;
        }
    }

    public bool Promote
    {
        get => _flags[7];
        set
        {
            serialized = false;
            _flags[7] = value;
        }
    }

    public bool Demote
    {
        get => _flags[8];
        set
        {
            serialized = false;
            _flags[8] = value;
        }
    }

    public bool Info
    {
        get => _flags[9];
        set
        {
            serialized = false;
            _flags[9] = value;
        }
    }

    public bool Settings
    {
        get => _flags[10];
        set
        {
            serialized = false;
            _flags[10] = value;
        }
    }

    public bool Pinned
    {
        get => _flags[11];
        set
        {
            serialized = false;
            _flags[11] = value;
        }
    }

    public bool Edit
    {
        get => _flags[12];
        set
        {
            serialized = false;
            _flags[12] = value;
        }
    }

    public bool Delete
    {
        get => _flags[13];
        set
        {
            serialized = false;
            _flags[13] = value;
        }
    }

    public bool GroupCall
    {
        get => _flags[14];
        set
        {
            serialized = false;
            _flags[14] = value;
        }
    }

    public bool Invites
    {
        get => _flags[15];
        set
        {
            serialized = false;
            _flags[15] = value;
        }
    }

    public bool Send
    {
        get => _flags[16];
        set
        {
            serialized = false;
            _flags[16] = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}