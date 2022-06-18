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
public class PollImpl : Poll
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PollImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -2032041631;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(_id, true);
            writer.Write<Flags>(_flags);
            writer.WriteTLString(_question);
            writer.Write(_answers.TLBytes, false);
            if (_flags[4])
            {
                writer.WriteInt32(_closePeriod, true);
            }

            if (_flags[5])
            {
                writer.WriteInt32(_closeDate, true);
            }

            serialized = true;
            return writer.ToReadOnlySequence();
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

    public bool Closed
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool PublicVoters
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
        }
    }

    public bool MultipleChoice
    {
        get => _flags[2];
        set
        {
            serialized = false;
            _flags[2] = value;
        }
    }

    public bool Quiz
    {
        get => _flags[3];
        set
        {
            serialized = false;
            _flags[3] = value;
        }
    }

    private string _question;
    public string Question
    {
        get => _question;
        set
        {
            serialized = false;
            _question = value;
        }
    }

    private Vector<PollAnswer> _answers;
    public Vector<PollAnswer> Answers
    {
        get => _answers;
        set
        {
            serialized = false;
            _answers = value;
        }
    }

    private int _closePeriod;
    public int ClosePeriod
    {
        get => _closePeriod;
        set
        {
            serialized = false;
            _flags[4] = true;
            _closePeriod = value;
        }
    }

    private int _closeDate;
    public int CloseDate
    {
        get => _closeDate;
        set
        {
            serialized = false;
            _flags[5] = true;
            _closeDate = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _id = buff.ReadInt64(true);
        _flags = buff.Read<Flags>();
        _question = buff.ReadTLString();
        buff.Skip(4); _answers  =  factory . Read < Vector < PollAnswer > > ( ref  buff ) ; 
        if (_flags[4])
        {
            _closePeriod = buff.ReadInt32(true);
        }

        if (_flags[5])
        {
            _closeDate = buff.ReadInt32(true);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}