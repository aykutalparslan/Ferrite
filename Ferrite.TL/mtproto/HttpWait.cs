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
public class HttpWait : ITLObject
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public HttpWait(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -1835453025;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt32(maxDelay, true);
            writer.WriteInt32(waitAfter, true);
            writer.WriteInt32(maxWait, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private int maxDelay;
    public int MaxDelay
    {
        get => maxDelay;
        set
        {
            serialized = false;
            maxDelay = value;
        }
    }

    private int waitAfter;
    public int WaitAfter
    {
        get => waitAfter;
        set
        {
            serialized = false;
            waitAfter = value;
        }
    }

    private int maxWait;
    public int MaxWait
    {
        get => maxWait;
        set
        {
            serialized = false;
            maxWait = value;
        }
    }

    public bool IsMethod => true;
    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        maxDelay = buff.ReadInt32(true);
        waitAfter = buff.ReadInt32(true);
        maxWait = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}