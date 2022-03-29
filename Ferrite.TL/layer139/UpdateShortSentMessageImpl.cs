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
public class UpdateShortSentMessageImpl : Updates
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public UpdateShortSentMessageImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1877614335;
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
            writer.WriteInt32(_pts, true);
            writer.WriteInt32(_ptsCount, true);
            writer.WriteInt32(_date, true);
            if (_flags[9])
            {
                writer.Write(_media.TLBytes, false);
            }

            if (_flags[7])
            {
                writer.Write(_entities.TLBytes, false);
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

    private int _pts;
    public int Pts
    {
        get => _pts;
        set
        {
            serialized = false;
            _pts = value;
        }
    }

    private int _ptsCount;
    public int PtsCount
    {
        get => _ptsCount;
        set
        {
            serialized = false;
            _ptsCount = value;
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
        _pts = buff.ReadInt32(true);
        _ptsCount = buff.ReadInt32(true);
        _date = buff.ReadInt32(true);
        if (_flags[9])
        {
            buff.Skip(4);
            _media = factory.Read<MessageMedia>(ref buff);
        }

        if (_flags[7])
        {
            buff.Skip(4);
            _entities = factory.Read<Vector<MessageEntity>>(ref buff);
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