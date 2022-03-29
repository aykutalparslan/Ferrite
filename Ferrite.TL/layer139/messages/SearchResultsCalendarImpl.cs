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
public class SearchResultsCalendarImpl : SearchResultsCalendar
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public SearchResultsCalendarImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 343859772;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt32(_count, true);
            writer.WriteInt32(_minDate, true);
            writer.WriteInt32(_minMsgId, true);
            if (_flags[1])
            {
                writer.WriteInt32(_offsetIdOffset, true);
            }

            writer.Write(_periods.TLBytes, false);
            writer.Write(_messages.TLBytes, false);
            writer.Write(_chats.TLBytes, false);
            writer.Write(_users.TLBytes, false);
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

    public bool Inexact
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    private int _count;
    public int Count
    {
        get => _count;
        set
        {
            serialized = false;
            _count = value;
        }
    }

    private int _minDate;
    public int MinDate
    {
        get => _minDate;
        set
        {
            serialized = false;
            _minDate = value;
        }
    }

    private int _minMsgId;
    public int MinMsgId
    {
        get => _minMsgId;
        set
        {
            serialized = false;
            _minMsgId = value;
        }
    }

    private int _offsetIdOffset;
    public int OffsetIdOffset
    {
        get => _offsetIdOffset;
        set
        {
            serialized = false;
            _flags[1] = true;
            _offsetIdOffset = value;
        }
    }

    private Vector<SearchResultsCalendarPeriod> _periods;
    public Vector<SearchResultsCalendarPeriod> Periods
    {
        get => _periods;
        set
        {
            serialized = false;
            _periods = value;
        }
    }

    private Vector<Message> _messages;
    public Vector<Message> Messages
    {
        get => _messages;
        set
        {
            serialized = false;
            _messages = value;
        }
    }

    private Vector<Chat> _chats;
    public Vector<Chat> Chats
    {
        get => _chats;
        set
        {
            serialized = false;
            _chats = value;
        }
    }

    private Vector<User> _users;
    public Vector<User> Users
    {
        get => _users;
        set
        {
            serialized = false;
            _users = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _count = buff.ReadInt32(true);
        _minDate = buff.ReadInt32(true);
        _minMsgId = buff.ReadInt32(true);
        if (_flags[1])
        {
            _offsetIdOffset = buff.ReadInt32(true);
        }

        buff.Skip(4); _periods  =  factory . Read < Vector < SearchResultsCalendarPeriod > > ( ref  buff ) ; 
        buff.Skip(4); _messages  =  factory . Read < Vector < Message > > ( ref  buff ) ; 
        buff.Skip(4); _chats  =  factory . Read < Vector < Chat > > ( ref  buff ) ; 
        buff.Skip(4); _users  =  factory . Read < Vector < User > > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}