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

namespace Ferrite.TL.currentLayer;
public class SecureCredentialsEncryptedImpl : SecureCredentialsEncrypted
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public SecureCredentialsEncryptedImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 871426631;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLBytes(_data);
            writer.WriteTLBytes(_hash);
            writer.WriteTLBytes(_secret);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private byte[] _data;
    public byte[] Data
    {
        get => _data;
        set
        {
            serialized = false;
            _data = value;
        }
    }

    private byte[] _hash;
    public byte[] Hash
    {
        get => _hash;
        set
        {
            serialized = false;
            _hash = value;
        }
    }

    private byte[] _secret;
    public byte[] Secret
    {
        get => _secret;
        set
        {
            serialized = false;
            _secret = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _data = buff.ReadTLBytes().ToArray();
        _hash = buff.ReadTLBytes().ToArray();
        _secret = buff.ReadTLBytes().ToArray();
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}