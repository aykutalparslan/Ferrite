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

namespace Ferrite.TL.layer139.auth;
public class BindTempAuthKey : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public BindTempAuthKey(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -841733627;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(_permAuthKeyId, true);
            writer.WriteInt64(_nonce, true);
            writer.WriteInt32(_expiresAt, true);
            writer.WriteTLBytes(_encryptedMessage);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private long _permAuthKeyId;
    public long PermAuthKeyId
    {
        get => _permAuthKeyId;
        set
        {
            serialized = false;
            _permAuthKeyId = value;
        }
    }

    private long _nonce;
    public long Nonce
    {
        get => _nonce;
        set
        {
            serialized = false;
            _nonce = value;
        }
    }

    private int _expiresAt;
    public int ExpiresAt
    {
        get => _expiresAt;
        set
        {
            serialized = false;
            _expiresAt = value;
        }
    }

    private byte[] _encryptedMessage;
    public byte[] EncryptedMessage
    {
        get => _encryptedMessage;
        set
        {
            serialized = false;
            _encryptedMessage = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _permAuthKeyId = buff.ReadInt64(true);
        _nonce = buff.ReadInt64(true);
        _expiresAt = buff.ReadInt32(true);
        _encryptedMessage = buff.ReadTLBytes().ToArray();
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}