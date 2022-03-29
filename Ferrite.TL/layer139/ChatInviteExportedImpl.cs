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
public class ChatInviteExportedImpl : ExportedChatInvite
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public ChatInviteExportedImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 179611673;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteTLString(_link);
            writer.WriteInt64(_adminId, true);
            writer.WriteInt32(_date, true);
            if (_flags[4])
            {
                writer.WriteInt32(_startDate, true);
            }

            if (_flags[1])
            {
                writer.WriteInt32(_expireDate, true);
            }

            if (_flags[2])
            {
                writer.WriteInt32(_usageLimit, true);
            }

            if (_flags[3])
            {
                writer.WriteInt32(_usage, true);
            }

            if (_flags[7])
            {
                writer.WriteInt32(_requested, true);
            }

            if (_flags[8])
            {
                writer.WriteTLString(_title);
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

    public bool Revoked
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool Permanent
    {
        get => _flags[5];
        set
        {
            serialized = false;
            _flags[5] = value;
        }
    }

    public bool RequestNeeded
    {
        get => _flags[6];
        set
        {
            serialized = false;
            _flags[6] = value;
        }
    }

    private string _link;
    public string Link
    {
        get => _link;
        set
        {
            serialized = false;
            _link = value;
        }
    }

    private long _adminId;
    public long AdminId
    {
        get => _adminId;
        set
        {
            serialized = false;
            _adminId = value;
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

    private int _startDate;
    public int StartDate
    {
        get => _startDate;
        set
        {
            serialized = false;
            _flags[4] = true;
            _startDate = value;
        }
    }

    private int _expireDate;
    public int ExpireDate
    {
        get => _expireDate;
        set
        {
            serialized = false;
            _flags[1] = true;
            _expireDate = value;
        }
    }

    private int _usageLimit;
    public int UsageLimit
    {
        get => _usageLimit;
        set
        {
            serialized = false;
            _flags[2] = true;
            _usageLimit = value;
        }
    }

    private int _usage;
    public int Usage
    {
        get => _usage;
        set
        {
            serialized = false;
            _flags[3] = true;
            _usage = value;
        }
    }

    private int _requested;
    public int Requested
    {
        get => _requested;
        set
        {
            serialized = false;
            _flags[7] = true;
            _requested = value;
        }
    }

    private string _title;
    public string Title
    {
        get => _title;
        set
        {
            serialized = false;
            _flags[8] = true;
            _title = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _link = buff.ReadTLString();
        _adminId = buff.ReadInt64(true);
        _date = buff.ReadInt32(true);
        if (_flags[4])
        {
            _startDate = buff.ReadInt32(true);
        }

        if (_flags[1])
        {
            _expireDate = buff.ReadInt32(true);
        }

        if (_flags[2])
        {
            _usageLimit = buff.ReadInt32(true);
        }

        if (_flags[3])
        {
            _usage = buff.ReadInt32(true);
        }

        if (_flags[7])
        {
            _requested = buff.ReadInt32(true);
        }

        if (_flags[8])
        {
            _title = buff.ReadTLString();
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}