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
public class WebFileImpl : WebFile
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public WebFileImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 568808380;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt32(_size, true);
            writer.WriteTLString(_mimeType);
            writer.Write(_fileType.TLBytes, false);
            writer.WriteInt32(_mtime, true);
            writer.WriteTLBytes(_bytes);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private int _size;
    public int Size
    {
        get => _size;
        set
        {
            serialized = false;
            _size = value;
        }
    }

    private string _mimeType;
    public string MimeType
    {
        get => _mimeType;
        set
        {
            serialized = false;
            _mimeType = value;
        }
    }

    private storage.FileType _fileType;
    public storage.FileType FileType
    {
        get => _fileType;
        set
        {
            serialized = false;
            _fileType = value;
        }
    }

    private int _mtime;
    public int Mtime
    {
        get => _mtime;
        set
        {
            serialized = false;
            _mtime = value;
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

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _size = buff.ReadInt32(true);
        _mimeType = buff.ReadTLString();
        buff.Skip(4); _fileType  =  factory . Read < storage . FileType > ( ref  buff ) ; 
        _mtime = buff.ReadInt32(true);
        _bytes = buff.ReadTLBytes().ToArray();
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}