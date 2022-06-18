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
public class GroupCallImpl : GroupCall
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public GroupCallImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -711498484;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt64(_id, true);
            writer.WriteInt64(_accessHash, true);
            writer.WriteInt32(_participantsCount, true);
            if (_flags[3])
            {
                writer.WriteTLString(_title);
            }

            if (_flags[4])
            {
                writer.WriteInt32(_streamDcId, true);
            }

            if (_flags[5])
            {
                writer.WriteInt32(_recordStartDate, true);
            }

            if (_flags[7])
            {
                writer.WriteInt32(_scheduleDate, true);
            }

            if (_flags[10])
            {
                writer.WriteInt32(_unmutedVideoCount, true);
            }

            writer.WriteInt32(_unmutedVideoLimit, true);
            writer.WriteInt32(_version, true);
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

    public bool JoinMuted
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
        }
    }

    public bool CanChangeJoinMuted
    {
        get => _flags[2];
        set
        {
            serialized = false;
            _flags[2] = value;
        }
    }

    public bool JoinDateAsc
    {
        get => _flags[6];
        set
        {
            serialized = false;
            _flags[6] = value;
        }
    }

    public bool ScheduleStartSubscribed
    {
        get => _flags[8];
        set
        {
            serialized = false;
            _flags[8] = value;
        }
    }

    public bool CanStartVideo
    {
        get => _flags[9];
        set
        {
            serialized = false;
            _flags[9] = value;
        }
    }

    public bool RecordVideoActive
    {
        get => _flags[11];
        set
        {
            serialized = false;
            _flags[11] = value;
        }
    }

    public bool RtmpStream
    {
        get => _flags[12];
        set
        {
            serialized = false;
            _flags[12] = value;
        }
    }

    public bool ListenersHidden
    {
        get => _flags[13];
        set
        {
            serialized = false;
            _flags[13] = value;
        }
    }

    private long _id;
    public long Id
    {
        get => _id;
        set
        {
            serialized = false;
            _id = value;
        }
    }

    private long _accessHash;
    public long AccessHash
    {
        get => _accessHash;
        set
        {
            serialized = false;
            _accessHash = value;
        }
    }

    private int _participantsCount;
    public int ParticipantsCount
    {
        get => _participantsCount;
        set
        {
            serialized = false;
            _participantsCount = value;
        }
    }

    private string _title;
    public string Title
    {
        get => _title;
        set
        {
            serialized = false;
            _flags[3] = true;
            _title = value;
        }
    }

    private int _streamDcId;
    public int StreamDcId
    {
        get => _streamDcId;
        set
        {
            serialized = false;
            _flags[4] = true;
            _streamDcId = value;
        }
    }

    private int _recordStartDate;
    public int RecordStartDate
    {
        get => _recordStartDate;
        set
        {
            serialized = false;
            _flags[5] = true;
            _recordStartDate = value;
        }
    }

    private int _scheduleDate;
    public int ScheduleDate
    {
        get => _scheduleDate;
        set
        {
            serialized = false;
            _flags[7] = true;
            _scheduleDate = value;
        }
    }

    private int _unmutedVideoCount;
    public int UnmutedVideoCount
    {
        get => _unmutedVideoCount;
        set
        {
            serialized = false;
            _flags[10] = true;
            _unmutedVideoCount = value;
        }
    }

    private int _unmutedVideoLimit;
    public int UnmutedVideoLimit
    {
        get => _unmutedVideoLimit;
        set
        {
            serialized = false;
            _unmutedVideoLimit = value;
        }
    }

    private int _version;
    public int Version
    {
        get => _version;
        set
        {
            serialized = false;
            _version = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _id = buff.ReadInt64(true);
        _accessHash = buff.ReadInt64(true);
        _participantsCount = buff.ReadInt32(true);
        if (_flags[3])
        {
            _title = buff.ReadTLString();
        }

        if (_flags[4])
        {
            _streamDcId = buff.ReadInt32(true);
        }

        if (_flags[5])
        {
            _recordStartDate = buff.ReadInt32(true);
        }

        if (_flags[7])
        {
            _scheduleDate = buff.ReadInt32(true);
        }

        if (_flags[10])
        {
            _unmutedVideoCount = buff.ReadInt32(true);
        }

        _unmutedVideoLimit = buff.ReadInt32(true);
        _version = buff.ReadInt32(true);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}