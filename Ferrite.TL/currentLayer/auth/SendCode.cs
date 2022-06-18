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

namespace Ferrite.TL.currentLayer.auth;
public class SendCode : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IAuthService _auth;
    private bool serialized = false;
    public SendCode(ITLObjectFactory objectFactory, IAuthService auth)
    {
        factory = objectFactory;
        _auth = auth;
    }

    public int Constructor => -1502141361;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLString(_phoneNumber);
            writer.WriteInt32(_apiId, true);
            writer.WriteTLString(_apiHash);
            writer.Write(_settings.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private string _phoneNumber;
    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            serialized = false;
            _phoneNumber = value;
        }
    }

    private int _apiId;
    public int ApiId
    {
        get => _apiId;
        set
        {
            serialized = false;
            _apiId = value;
        }
    }

    private string _apiHash;
    public string ApiHash
    {
        get => _apiHash;
        set
        {
            serialized = false;
            _apiHash = value;
        }
    }

    private CodeSettings _settings;
    public CodeSettings Settings
    {
        get => _settings;
        set
        {
            serialized = false;
            _settings = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var sent = await _auth.SendCode(_phoneNumber, _apiId, _apiHash,
            new Data.Auth.CodeSettings());
        var sentCode = factory.Resolve<SentCodeImpl>();
        var codeType = factory.Resolve<SentCodeTypeSmsImpl>();
        var nextType = factory.Resolve<CodeTypeSmsImpl>();
        sentCode.NextType = nextType;
        sentCode.PhoneCodeHash = sent.PhoneCodeHash;
        sentCode.Timeout = sent.Timeout;
        sentCode.Type = codeType;
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        result.Result = sentCode;
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _phoneNumber = buff.ReadTLString();
        _apiId = buff.ReadInt32(true);
        _apiHash = buff.ReadTLString();
        _settings = (CodeSettings)factory.Read(buff.ReadInt32(true), ref buff);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}