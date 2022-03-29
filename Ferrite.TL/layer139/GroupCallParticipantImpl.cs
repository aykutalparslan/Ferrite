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
public class GroupCallParticipantImpl : GroupCallParticipant
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public GroupCallParticipantImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -341428482;
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
            writer.WriteInt32(_date, true);
            if (_flags[3])
            {
                writer.WriteInt32(_activeDate, true);
            }

            writer.WriteInt32(_source, true);
            if (_flags[7])
            {
                writer.WriteInt32(_volume, true);
            }

            if (_flags[11])
            {
                writer.WriteTLString(_about);
            }

            if (_flags[13])
            {
                writer.WriteInt64(_raiseHandRating, true);
            }

            if (_flags[6])
            {
                writer.Write(_video.TLBytes, false);
            }

            if (_flags[14])
            {
                writer.Write(_presentation.TLBytes, false);
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

    public bool Muted
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool Left
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
        }
    }

    public bool CanSelfUnmute
    {
        get => _flags[2];
        set
        {
            serialized = false;
            _flags[2] = value;
        }
    }

    public bool JustJoined
    {
        get => _flags[4];
        set
        {
            serialized = false;
            _flags[4] = value;
        }
    }

    public bool Versioned
    {
        get => _flags[5];
        set
        {
            serialized = false;
            _flags[5] = value;
        }
    }

    public bool Min
    {
        get => _flags[8];
        set
        {
            serialized = false;
            _flags[8] = value;
        }
    }

    public bool MutedByYou
    {
        get => _flags[9];
        set
        {
            serialized = false;
            _flags[9] = value;
        }
    }

    public bool VolumeByAdmin
    {
        get => _flags[10];
        set
        {
            serialized = false;
            _flags[10] = value;
        }
    }

    public bool Self
    {
        get => _flags[12];
        set
        {
            serialized = false;
            _flags[12] = value;
        }
    }

    public bool VideoJoined
    {
        get => _flags[15];
        set
        {
            serialized = false;
            _flags[15] = value;
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

    private int _activeDate;
    public int ActiveDate
    {
        get => _activeDate;
        set
        {
            serialized = false;
            _flags[3] = true;
            _activeDate = value;
        }
    }

    private int _source;
    public int Source
    {
        get => _source;
        set
        {
            serialized = false;
            _source = value;
        }
    }

    private int _volume;
    public int Volume
    {
        get => _volume;
        set
        {
            serialized = false;
            _flags[7] = true;
            _volume = value;
        }
    }

    private string _about;
    public string About
    {
        get => _about;
        set
        {
            serialized = false;
            _flags[11] = true;
            _about = value;
        }
    }

    private long _raiseHandRating;
    public long RaiseHandRating
    {
        get => _raiseHandRating;
        set
        {
            serialized = false;
            _flags[13] = true;
            _raiseHandRating = value;
        }
    }

    private GroupCallParticipantVideo _video;
    public GroupCallParticipantVideo Video
    {
        get => _video;
        set
        {
            serialized = false;
            _flags[6] = true;
            _video = value;
        }
    }

    private GroupCallParticipantVideo _presentation;
    public GroupCallParticipantVideo Presentation
    {
        get => _presentation;
        set
        {
            serialized = false;
            _flags[14] = true;
            _presentation = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        buff.Skip(4); _peer  =  factory . Read < Peer > ( ref  buff ) ; 
        _date = buff.ReadInt32(true);
        if (_flags[3])
        {
            _activeDate = buff.ReadInt32(true);
        }

        _source = buff.ReadInt32(true);
        if (_flags[7])
        {
            _volume = buff.ReadInt32(true);
        }

        if (_flags[11])
        {
            _about = buff.ReadTLString();
        }

        if (_flags[13])
        {
            _raiseHandRating = buff.ReadInt64(true);
        }

        if (_flags[6])
        {
            buff.Skip(4);
            _video = factory.Read<GroupCallParticipantVideo>(ref buff);
        }

        if (_flags[14])
        {
            buff.Skip(4);
            _presentation = factory.Read<GroupCallParticipantVideo>(ref buff);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}