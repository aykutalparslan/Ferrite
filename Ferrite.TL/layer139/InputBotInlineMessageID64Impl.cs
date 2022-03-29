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
public class InputBotInlineMessageID64Impl : InputBotInlineMessageID
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public InputBotInlineMessageID64Impl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1227287081;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt32(_dcId, true);
            writer.WriteInt64(_ownerId, true);
            writer.WriteInt32(_id, true);
            writer.WriteInt64(_accessHash, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private int _dcId;
    public int DcId
    {
        get => _dcId;
        set
        {
            serialized = false;
            _dcId = value;
        }
    }

    private long _ownerId;
    public long OwnerId
    {
        get => _ownerId;
        set
        {
            serialized = false;
            _ownerId = value;
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

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _dcId = buff.ReadInt32(true);
        _ownerId = buff.ReadInt64(true);
        _id = buff.ReadInt32(true);
        _accessHash = buff.ReadInt64(true);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}