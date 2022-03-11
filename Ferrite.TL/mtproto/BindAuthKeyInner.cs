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

namespace Ferrite.TL.mtproto;
public class BindAuthKeyInner : ITLObject
{
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private ITLObjectFactory factory;
    private bool serialized = false;
    public BindAuthKeyInner(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => 1973679973;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(nonce, true);
            writer.WriteInt64(tempAuthKeyId, true);
            writer.WriteInt64(permAuthKeyId, true);
            writer.WriteInt64(tempSessionId, true);
            writer.WriteInt32(expiresAt, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private long nonce;
    public long Nonce
    {
        get => nonce;
        set
        {
            serialized = false;
            nonce = value;
        }
    }

    private long tempAuthKeyId;
    public long TempAuthKeyId
    {
        get => tempAuthKeyId;
        set
        {
            serialized = false;
            tempAuthKeyId = value;
        }
    }

    private long permAuthKeyId;
    public long PermAuthKeyId
    {
        get => permAuthKeyId;
        set
        {
            serialized = false;
            permAuthKeyId = value;
        }
    }

    private long tempSessionId;
    public long TempSessionId
    {
        get => tempSessionId;
        set
        {
            serialized = false;
            tempSessionId = value;
        }
    }

    private int expiresAt;
    public int ExpiresAt
    {
        get => expiresAt;
        set
        {
            serialized = false;
            expiresAt = value;
        }
    }

    public bool IsMethod => false;
    public ITLObject Execute(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        nonce = buff.ReadInt64(true);
        tempAuthKeyId = buff.ReadInt64(true);
        permAuthKeyId = buff.ReadInt64(true);
        tempSessionId = buff.ReadInt64(true);
        expiresAt = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}