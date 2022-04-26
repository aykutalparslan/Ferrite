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
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.TL.layer139.auth;
public class BindTempAuthKey : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IMTProtoService _mtproto;
    private readonly IAuthService _auth;
    private bool serialized = false;
    public BindTempAuthKey(ITLObjectFactory objectFactory, IMTProtoService mtproto, IAuthService auth)
    {
        factory = objectFactory;
        _mtproto = mtproto;
        _auth = auth;
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
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        var authKey = await _mtproto.GetAuthKeyAsync(ctx.AuthKeyId);
        var resp = factory.Resolve<RpcError>();
        if (authKey == null)
        {
            resp.ErrorCode = 501;
            resp.ErrorMessage = "INTERNAL_SERVER_ERROR";
            result.Result = resp;
        }
        var bindingMessage = DecryptBindingMessage(authKey);
        if (bindingMessage != null && bindingMessage.PermAuthKeyId == ctx.AuthKeyId &&
            bindingMessage.Nonce == _nonce)
        {
            var success = await _auth.BindTempAuthKey(bindingMessage.TempAuthKeyId,
            bindingMessage.PermAuthKeyId, _expiresAt);
            result.Result = success ? new BoolTrue() : new BoolFalse();
        }
        resp.ErrorCode = 400;
        resp.ErrorMessage = "ENCRYPTED_MESSAGE_INVALID";
        result.Result = resp;
        return result;
    }

    private BindAuthKeyInnerImpl DecryptBindingMessage(Span<byte> authKey)
    {
        var encrypted = _encryptedMessage.AsSpan();
        Span<byte> messageKey = encrypted.Slice(8, 16);
        AesIgeV1 aesIge = new AesIgeV1(authKey, messageKey);
        aesIge.Decrypt(encrypted.Slice(24));
        SequenceReader reader = IAsyncBinaryReader.Create(_encryptedMessage);
        reader.Skip(28);
        return factory.Read<BindAuthKeyInnerImpl>(ref reader);
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