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
using Ferrite.Data;
using Ferrite.Utils;

namespace Ferrite.TL.mtproto;
public class DestroySession : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IDistributedCache _cache;
    private bool serialized = false;
    public DestroySession(ITLObjectFactory objectFactory, IDistributedCache cache)
    {
        factory = objectFactory;
        _cache = cache;
    }

    public int Constructor => -414113498;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(sessionId, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private long sessionId;
    public long SessionId
    {
        get => sessionId;
        set
        {
            serialized = false;
            sessionId = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var destroyed = await _cache.RemoveSessionAsync(sessionId);
        if (destroyed)
        {
            var resp = factory.Resolve<DestroySessionOk>();
            return resp;
        }
        var none = factory.Resolve<DestroySessionNone>();
        return none;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        sessionId = buff.ReadInt64(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}