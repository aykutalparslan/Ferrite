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
using Ferrite.Services;
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer.account;
public class ChangeAuthorizationSettings : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IAccountService _account;
    private bool serialized = false;
    public ChangeAuthorizationSettings(ITLObjectFactory objectFactory, IAccountService account)
    {
        factory = objectFactory;
        _account = account;
    }

    public int Constructor => 1089766498;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt64(_hash, true);
            if (_flags[0])
            {
                writer.WriteInt32(Bool.GetConstructor(_encryptedRequestsDisabled), true);
            }

            if (_flags[1])
            {
                writer.WriteInt32(Bool.GetConstructor(_callRequestsDisabled), true);
            }

            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private Flags _flags;
    public Flags Flags
    {
        get => _flags;
        set
        {
            serialized = false;
            _flags = value;
        }
    }

    private long _hash;
    public long Hash
    {
        get => _hash;
        set
        {
            serialized = false;
            _hash = value;
        }
    }

    private bool _encryptedRequestsDisabled;
    public bool EncryptedRequestsDisabled
    {
        get => _encryptedRequestsDisabled;
        set
        {
            serialized = false;
            _flags[0] = true;
            _encryptedRequestsDisabled = value;
        }
    }

    private bool _callRequestsDisabled;
    public bool CallRequestsDisabled
    {
        get => _callRequestsDisabled;
        set
        {
            serialized = false;
            _flags[1] = true;
            _callRequestsDisabled = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        /*var serviceResult = await _account.ChangeAuthorizationSettings(ctx.AuthKeyId, _hash, 
            _encryptedRequestsDisabled, _callRequestsDisabled);
        var rpcResult = factory.Resolve<RpcResult>();
        rpcResult.ReqMsgId = ctx.MessageId;
        if (!serviceResult.Success)
        {
            var err = factory.Resolve<RpcError>();
            err.ErrorCode = serviceResult.ErrorMessage.Code;
            err.ErrorMessage = serviceResult.ErrorMessage.Message;
        }
        else
        {
            rpcResult.Result = serviceResult.Result ? new BoolTrue() : new BoolFalse();
        }
        return rpcResult;*/
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _hash = buff.ReadInt64(true);
        if (_flags[0])
        {
            _encryptedRequestsDisabled = Bool.Read(ref buff);
        }

        if (_flags[1])
        {
            _callRequestsDisabled = Bool.Read(ref buff);
        }
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}