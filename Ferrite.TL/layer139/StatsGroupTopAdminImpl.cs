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
public class StatsGroupTopAdminImpl : StatsGroupTopAdmin
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public StatsGroupTopAdminImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -682079097;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(_userId, true);
            writer.WriteInt32(_deleted, true);
            writer.WriteInt32(_kicked, true);
            writer.WriteInt32(_banned, true);
            serialized = true;
            return writer.ToReadOnlySequence();
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

    private int _deleted;
    public int Deleted
    {
        get => _deleted;
        set
        {
            serialized = false;
            _deleted = value;
        }
    }

    private int _kicked;
    public int Kicked
    {
        get => _kicked;
        set
        {
            serialized = false;
            _kicked = value;
        }
    }

    private int _banned;
    public int Banned
    {
        get => _banned;
        set
        {
            serialized = false;
            _banned = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _userId = buff.ReadInt64(true);
        _deleted = buff.ReadInt32(true);
        _kicked = buff.ReadInt32(true);
        _banned = buff.ReadInt32(true);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}