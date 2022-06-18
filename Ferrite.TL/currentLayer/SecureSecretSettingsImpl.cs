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
public class SecureSecretSettingsImpl : SecureSecretSettings
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public SecureSecretSettingsImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 354925740;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_secureAlgo.TLBytes, false);
            writer.WriteTLBytes(_secureSecret);
            writer.WriteInt64(_secureSecretId, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private SecurePasswordKdfAlgo _secureAlgo;
    public SecurePasswordKdfAlgo SecureAlgo
    {
        get => _secureAlgo;
        set
        {
            serialized = false;
            _secureAlgo = value;
        }
    }

    private byte[] _secureSecret;
    public byte[] SecureSecret
    {
        get => _secureSecret;
        set
        {
            serialized = false;
            _secureSecret = value;
        }
    }

    private long _secureSecretId;
    public long SecureSecretId
    {
        get => _secureSecretId;
        set
        {
            serialized = false;
            _secureSecretId = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _secureAlgo = (SecurePasswordKdfAlgo)factory.Read(buff.ReadInt32(true), ref buff);
        _secureSecret = buff.ReadTLBytes().ToArray();
        _secureSecretId = buff.ReadInt64(true);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}