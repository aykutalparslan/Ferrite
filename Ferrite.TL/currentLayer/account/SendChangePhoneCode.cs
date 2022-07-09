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
using Ferrite.TL.currentLayer.auth;
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer.account;
public class SendChangePhoneCode : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IAccountService _accountService;
    private bool serialized = false;
    public SendChangePhoneCode(ITLObjectFactory objectFactory, IAccountService accountService)
    {
        factory = objectFactory;
        _accountService = accountService;
    }

    public int Constructor => -2108208411;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLString(_phoneNumber);
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
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        var serviceResult = await _accountService.SendChangePhoneCode(ctx.PermAuthKeyId!=0 ? 
            ctx.PermAuthKeyId : ctx.AuthKeyId, _phoneNumber, new Data.Auth.CodeSettingsDTO());
        if (!serviceResult.Success)
        {
            var err = factory.Resolve<RpcError>();
            err.ErrorCode = serviceResult.ErrorMessage.Code;
            err.ErrorMessage = serviceResult.ErrorMessage.Message;
            result.Result = err;
        }
        else
        {
            var sent = serviceResult.Result;
            var sentCode = factory.Resolve<SentCodeImpl>();
            var codeType = factory.Resolve<SentCodeTypeSmsImpl>();
            var nextType = factory.Resolve<CodeTypeSmsImpl>();
            sentCode.NextType = nextType;
            sentCode.PhoneCodeHash = sent.PhoneCodeHash;
            sentCode.Timeout = sent.Timeout;
            sentCode.Type = codeType;

            result.Result = sentCode;
        }
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _phoneNumber = buff.ReadTLString();
        _settings = (CodeSettings)factory.Read(buff.ReadInt32(true), ref buff);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}