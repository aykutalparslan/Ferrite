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
public class ChatInviteImpl : ChatInvite
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public ChatInviteImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 806110401;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteTLString(_title);
            if (_flags[5])
            {
                writer.WriteTLString(_about);
            }

            writer.Write(_photo.TLBytes, false);
            writer.WriteInt32(_participantsCount, true);
            if (_flags[4])
            {
                writer.Write(_participants.TLBytes, false);
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

    public bool Channel
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool Broadcast
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
        }
    }

    public bool Public
    {
        get => _flags[2];
        set
        {
            serialized = false;
            _flags[2] = value;
        }
    }

    public bool Megagroup
    {
        get => _flags[3];
        set
        {
            serialized = false;
            _flags[3] = value;
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

    private string _title;
    public string Title
    {
        get => _title;
        set
        {
            serialized = false;
            _title = value;
        }
    }

    private string _about;
    public string About
    {
        get => _about;
        set
        {
            serialized = false;
            _flags[5] = true;
            _about = value;
        }
    }

    private Photo _photo;
    public Photo Photo
    {
        get => _photo;
        set
        {
            serialized = false;
            _photo = value;
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

    private Vector<User> _participants;
    public Vector<User> Participants
    {
        get => _participants;
        set
        {
            serialized = false;
            _flags[4] = true;
            _participants = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _title = buff.ReadTLString();
        if (_flags[5])
        {
            _about = buff.ReadTLString();
        }

        buff.Skip(4); _photo  =  factory . Read < Photo > ( ref  buff ) ; 
        _participantsCount = buff.ReadInt32(true);
        if (_flags[4])
        {
            buff.Skip(4);
            _participants = factory.Read<Vector<User>>(ref buff);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}