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

namespace Ferrite.TL.layer139.upload;
public class SaveBigFilePart : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public SaveBigFilePart(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -562337987;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(_fileId, true);
            writer.WriteInt32(_filePart, true);
            writer.WriteInt32(_fileTotalParts, true);
            writer.WriteTLBytes(_bytes);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private long _fileId;
    public long FileId
    {
        get => _fileId;
        set
        {
            serialized = false;
            _fileId = value;
        }
    }

    private int _filePart;
    public int FilePart
    {
        get => _filePart;
        set
        {
            serialized = false;
            _filePart = value;
        }
    }

    private int _fileTotalParts;
    public int FileTotalParts
    {
        get => _fileTotalParts;
        set
        {
            serialized = false;
            _fileTotalParts = value;
        }
    }

    private byte[] _bytes;
    public byte[] Bytes
    {
        get => _bytes;
        set
        {
            serialized = false;
            _bytes = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _fileId = buff.ReadInt64(true);
        _filePart = buff.ReadInt32(true);
        _fileTotalParts = buff.ReadInt32(true);
        _bytes = buff.ReadTLBytes().ToArray();
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}