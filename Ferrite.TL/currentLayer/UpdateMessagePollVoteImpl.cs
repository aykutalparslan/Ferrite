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
public class UpdateMessagePollVoteImpl : Update
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public UpdateMessagePollVoteImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 274961865;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(_pollId, true);
            writer.WriteInt64(_userId, true);
            writer.Write(_options.TLBytes, false);
            writer.WriteInt32(_qts, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private long _pollId;
    public long PollId
    {
        get => _pollId;
        set
        {
            serialized = false;
            _pollId = value;
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

    private VectorOfBytes _options;
    public VectorOfBytes Options
    {
        get => _options;
        set
        {
            serialized = false;
            _options = value;
        }
    }

    private int _qts;
    public int Qts
    {
        get => _qts;
        set
        {
            serialized = false;
            _qts = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _pollId = buff.ReadInt64(true);
        _userId = buff.ReadInt64(true);
        buff.Skip(4); _options  =  factory . Read < VectorOfBytes > ( ref  buff ) ; 
        _qts = buff.ReadInt32(true);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}