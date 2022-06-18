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

namespace Ferrite.TL.currentLayer.upload;
public class FileCdnRedirectImpl : File
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public FileCdnRedirectImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -242427324;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt32(_dcId, true);
            writer.WriteTLBytes(_fileToken);
            writer.WriteTLBytes(_encryptionKey);
            writer.WriteTLBytes(_encryptionIv);
            writer.Write(_fileHashes.TLBytes, false);
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

    private byte[] _fileToken;
    public byte[] FileToken
    {
        get => _fileToken;
        set
        {
            serialized = false;
            _fileToken = value;
        }
    }

    private byte[] _encryptionKey;
    public byte[] EncryptionKey
    {
        get => _encryptionKey;
        set
        {
            serialized = false;
            _encryptionKey = value;
        }
    }

    private byte[] _encryptionIv;
    public byte[] EncryptionIv
    {
        get => _encryptionIv;
        set
        {
            serialized = false;
            _encryptionIv = value;
        }
    }

    private Vector<FileHash> _fileHashes;
    public Vector<FileHash> FileHashes
    {
        get => _fileHashes;
        set
        {
            serialized = false;
            _fileHashes = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _dcId = buff.ReadInt32(true);
        _fileToken = buff.ReadTLBytes().ToArray();
        _encryptionKey = buff.ReadTLBytes().ToArray();
        _encryptionIv = buff.ReadTLBytes().ToArray();
        buff.Skip(4); _fileHashes  =  factory . Read < Vector < FileHash > > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}