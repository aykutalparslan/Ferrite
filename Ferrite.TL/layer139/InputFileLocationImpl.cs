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
public class InputFileLocationImpl : InputFileLocation
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public InputFileLocationImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -539317279;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(_volumeId, true);
            writer.WriteInt32(_localId, true);
            writer.WriteInt64(_secret, true);
            writer.WriteTLBytes(_fileReference);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private long _volumeId;
    public long VolumeId
    {
        get => _volumeId;
        set
        {
            serialized = false;
            _volumeId = value;
        }
    }

    private int _localId;
    public int LocalId
    {
        get => _localId;
        set
        {
            serialized = false;
            _localId = value;
        }
    }

    private long _secret;
    public long Secret
    {
        get => _secret;
        set
        {
            serialized = false;
            _secret = value;
        }
    }

    private byte[] _fileReference;
    public byte[] FileReference
    {
        get => _fileReference;
        set
        {
            serialized = false;
            _fileReference = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _volumeId = buff.ReadInt64(true);
        _localId = buff.ReadInt32(true);
        _secret = buff.ReadInt64(true);
        _fileReference = buff.ReadTLBytes().ToArray();
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}