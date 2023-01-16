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
public class GetAuthorizations : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IAccountService _accountService;
        private bool serialized = false;
    public GetAuthorizations(ITLObjectFactory objectFactory, IAccountService accountService)
    {
        factory = objectFactory;
        _accountService = accountService;
    }

    public int Constructor => -484392616;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        /*var serviceResult = await _accountService.GetAuthorizations(ctx.PermAuthKeyId != 0 ? 
            ctx.PermAuthKeyId : ctx.AuthKeyId);
        
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        var authorizations = factory.Resolve<AuthorizationsImpl>();
        authorizations.AuthorizationTtlDays = serviceResult.AuthorizationTTLDays;
        var authList = factory.Resolve<Vector<Authorization>>();
        foreach (var info in serviceResult.AppInfos)
        {
            var auth = factory.Resolve<AuthorizationImpl>();
            auth.ApiId = info.ApiId;
            auth.AppName = "Unknown";
            auth.AppVersion = info.AppVersion;
            auth.CallRequestsDisabled = info.CallRequestsDisabled;
            auth.Country = "Turkey";
            auth.Current = true;
            auth.DateActive = (int)DateTimeOffset.Now.ToUnixTimeSeconds();
            auth.DateCreated = (int)DateTimeOffset.Now.ToUnixTimeSeconds();
            auth.DeviceModel = info.DeviceModel;
            auth.EncryptedRequestsDisabled = info.EncryptedRequestsDisabled;
            auth.Hash = info.Hash;
            auth.Ip = info.IP;
            auth.OfficialApp = true;
            auth.Platform = "Unknown";
            auth.Region = "Unknown";
            auth.SystemVersion = info.SystemVersion;
            authList.Add(auth);
        }

        authorizations.Authorizations = authList;
        result.Result = authorizations;
        return result;*/
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}