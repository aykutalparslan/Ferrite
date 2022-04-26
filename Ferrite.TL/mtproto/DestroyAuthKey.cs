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
public class DestroyAuthKey : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IDistributedCache _store;
    private readonly IPersistentStore _cache;
    private bool serialized = false;
    public DestroyAuthKey(ITLObjectFactory objectFactory, IDistributedCache store, IPersistentStore cache)
    {
        factory = objectFactory;
        _store = store;
        _cache = cache;
    }

    public int Constructor => -784117408;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        bool deleteCache = await _cache.DeleteAuthKeyAsync(ctx.AuthKeyId);
        bool deleteDatabase = await _store.DeleteAuthKeyAsync(ctx.AuthKeyId);
        if(deleteCache && deleteDatabase)
        {
            var resp = factory.Resolve<DestroyAuthKeyOk>();
            return resp;
        }
        var fail = factory.Resolve<DestroyAuthKeyFail>();
        return fail;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}