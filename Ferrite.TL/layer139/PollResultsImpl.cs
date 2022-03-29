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
public class PollResultsImpl : PollResults
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PollResultsImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -591909213;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            if (_flags[1])
            {
                writer.Write(_results.TLBytes, false);
            }

            if (_flags[2])
            {
                writer.WriteInt32(_totalVoters, true);
            }

            if (_flags[3])
            {
                writer.Write(_recentVoters.TLBytes, false);
            }

            if (_flags[4])
            {
                writer.WriteTLString(_solution);
            }

            if (_flags[4])
            {
                writer.Write(_solutionEntities.TLBytes, false);
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

    public bool Min
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    private Vector<PollAnswerVoters> _results;
    public Vector<PollAnswerVoters> Results
    {
        get => _results;
        set
        {
            serialized = false;
            _flags[1] = true;
            _results = value;
        }
    }

    private int _totalVoters;
    public int TotalVoters
    {
        get => _totalVoters;
        set
        {
            serialized = false;
            _flags[2] = true;
            _totalVoters = value;
        }
    }

    private VectorOfLong _recentVoters;
    public VectorOfLong RecentVoters
    {
        get => _recentVoters;
        set
        {
            serialized = false;
            _flags[3] = true;
            _recentVoters = value;
        }
    }

    private string _solution;
    public string Solution
    {
        get => _solution;
        set
        {
            serialized = false;
            _flags[4] = true;
            _solution = value;
        }
    }

    private Vector<MessageEntity> _solutionEntities;
    public Vector<MessageEntity> SolutionEntities
    {
        get => _solutionEntities;
        set
        {
            serialized = false;
            _flags[4] = true;
            _solutionEntities = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        if (_flags[1])
        {
            buff.Skip(4);
            _results = factory.Read<Vector<PollAnswerVoters>>(ref buff);
        }

        if (_flags[2])
        {
            _totalVoters = buff.ReadInt32(true);
        }

        if (_flags[3])
        {
            buff.Skip(4);
            _recentVoters = factory.Read<VectorOfLong>(ref buff);
        }

        if (_flags[4])
        {
            _solution = buff.ReadTLString();
        }

        if (_flags[4])
        {
            buff.Skip(4);
            _solutionEntities = factory.Read<Vector<MessageEntity>>(ref buff);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}