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
using System.IO.Pipelines;
using DotNext.Buffers;
using DotNext.IO;
using DotNext.IO.Pipelines;
using Ferrite.Data;
using Ferrite.Utils;

namespace Ferrite.TL.layer139.upload;
public class SaveFilePart : ITLObject, ITLMethod, IPipeOwner
{
    private readonly IDistributedObjectStore _objectStore;
    public SaveFilePart(IDistributedObjectStore objectStore)
    {
        _objectStore = objectStore;
    }

    public int Constructor => -1291540959;
    public void Parse(ref SequenceReader buff)
    {
        throw new NotSupportedException();
    }

    public void WriteTo(Span<byte> buff)
    {
        throw new NotSupportedException();
    }

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

    private MTProtoPipe _bytes;
    private int _length = 0;

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var stream = new MTProtoStream(_bytes.Input, _length);
        await _objectStore.SaveFilePart(_fileId, _filePart, stream);
        throw new NotImplementedException();
    }

    public async Task<bool> SetPipe(MTProtoPipe value)
    {
        _bytes = value;
        _fileId = await _bytes.Input.ReadInt64Async(false);
        _filePart = await _bytes.Input.ReadInt32Async(false);
        _length = await _bytes.Input.ReadTLBytesLength();
        return true;
    }

    public int Length => _length;
}