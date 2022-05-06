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
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.TL.layer139.account;
public class RegisterDevice : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IAccountService _account;
    private bool serialized = false;
    public RegisterDevice(ITLObjectFactory objectFactory, IAccountService account)
    {
        factory = objectFactory;
        _account = account;
    }

    public int Constructor => -326762118;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt32(_tokenType, true);
            writer.WriteTLString(_token);
            writer.WriteInt32(Bool.GetConstructor(_appSandbox), true);
            writer.WriteTLBytes(_secret);
            writer.Write(_otherUids.TLBytes, false);
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

    public bool NoMuted
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    private int _tokenType;
    public int TokenType
    {
        get => _tokenType;
        set
        {
            serialized = false;
            _tokenType = value;
        }
    }

    private string _token;
    public string Token
    {
        get => _token;
        set
        {
            serialized = false;
            _token = value;
        }
    }

    private bool _appSandbox;
    public bool AppSandbox
    {
        get => _appSandbox;
        set
        {
            serialized = false;
            _appSandbox = value;
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

    private VectorOfLong _otherUids;
    public VectorOfLong OtherUids
    {
        get => _otherUids;
        set
        {
            serialized = false;
            _otherUids = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        var success = await _account.RegisterDevice(new DeviceInfo()
        {
            AuthKeyId = ctx.AuthKeyId,
            Secret = _secret,
            Token = _token,
            AppSandbox = _appSandbox,
            NoMuted = NoMuted,
            TokenType = _tokenType,
            OtherUserIds = _otherUids
        });
        result.Result = success ? new BoolTrue() : new BoolFalse();
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _tokenType = buff.ReadInt32(true);
        _token = buff.ReadTLString();
        _appSandbox = Bool.Read(ref buff);
        _secret = buff.ReadTLBytes().ToArray();
        buff.Skip(4); _otherUids  =  factory . Read < VectorOfLong > ( ref  buff ) ; 
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}