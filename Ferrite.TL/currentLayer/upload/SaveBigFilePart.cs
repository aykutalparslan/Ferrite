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
using DotNext.IO.Pipelines;
using Ferrite.Data;
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer.upload;
public class SaveBigFilePart : ITLObject, ITLMethod, IPipeOwner
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IDistributedObjectStore _objectStore;
    private readonly IPersistentStore _store;
    public SaveBigFilePart(ITLObjectFactory objectFactory, IDistributedObjectStore objectStore, IPersistentStore store)
    {
        factory = objectFactory;
        _objectStore = objectStore;
        _store = store;
    }

    public int Constructor => -562337987;
    public ReadOnlySequence<byte> TLBytes => throw new NotSupportedException();

    private long _fileId;
    public long FileId
    {
        get => _fileId;
        set => _fileId = value;
    }

    private int _filePart;
    public int FilePart
    {
        get => _filePart;
        set => _filePart = value;
    }

    private int _fileTotalParts;
    public int FileTotalParts
    {
        get => _fileTotalParts;
        set => _fileTotalParts = value;
    }

    private MTProtoPipe _bytes;
    private int _length = 0;

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var stream = new MTProtoStream(_bytes.Input, _length);
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        var success = await _objectStore.SaveBigFilePart(_fileId, _filePart, _fileTotalParts, stream);
        await _store.SaveBigFilePartAsync(new FilePart(_fileId, _filePart, _length));
        result.Result = success ? new BoolTrue() : new BoolFalse();
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        throw new NotSupportedException();
    }

    public void WriteTo(Span<byte> buff)
    {
        throw new NotSupportedException();
    }

    public async Task<bool> SetPipe(MTProtoPipe value)
    {
        _bytes = value;
        _fileId = await _bytes.Input.ReadInt64Async(true);
        _filePart = await _bytes.Input.ReadInt32Async(true);
        _fileTotalParts = await _bytes.Input.ReadInt32Async(true);
        _length = await _bytes.Input.ReadTLBytesLength();
        return true;
    }

    public int Length => _length;
}