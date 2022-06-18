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
public class AcceptLoginToken : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IAuthService _auth;
    private readonly IUpdatesManager _updatesManager;
    private bool serialized = false;
    public AcceptLoginToken(ITLObjectFactory objectFactory, IAuthService auth, IUpdatesManager updatesManager)
    {
        factory = objectFactory;
        _auth = auth;
        _updatesManager = updatesManager;
    }

    public int Constructor => -392909491;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLBytes(_token);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private byte[] _token;
    public byte[] Token
    {
        get => _token;
        set
        {
            serialized = false;
            _token = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var acceptResult = await _auth.AcceptLoginToken(ctx.PermAuthKeyId!=0 ? ctx.PermAuthKeyId : ctx.AuthKeyId, _token);

        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        if (acceptResult != null)
        {
            var resp = factory.Resolve<currentLayer.AuthorizationImpl>();
            resp.ApiId = acceptResult.ApiId;
            resp.AppName = "Unknown";
            resp.AppVersion = acceptResult.AppVersion;
            resp.CallRequestsDisabled = false;
            resp.Country = "Turkey";
            resp.Current = true;
            resp.DateActive = (int)DateTimeOffset.Now.ToUnixTimeSeconds();
            resp.DateCreated = (int)DateTimeOffset.Now.ToUnixTimeSeconds();
            resp.DeviceModel = acceptResult.DeviceModel;
            resp.EncryptedRequestsDisabled = false;
            resp.Hash = acceptResult.Hash;
            resp.Ip = acceptResult.IP;
            resp.OfficialApp = true;
            resp.Platform = "Unknown";
            resp.Region = "Unknown";
            resp.SystemVersion = acceptResult.SystemVersion;
            result.Result = resp;
            _ = _updatesManager.SendUpdateLoginToken(acceptResult.AuthKeyId);
        }
        else
        {
            var err = factory.Resolve<RpcError>();
            err.ErrorCode = 501;
            err.ErrorMessage = "INTERNAL_SERVER_ERROR";
            result.Result = err;
        }
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _token = buff.ReadTLBytes().ToArray();
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}