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
public class ImportAuthorization : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IAuthService _service;
    private bool serialized = false;
    public ImportAuthorization(ITLObjectFactory objectFactory, IAuthService service)
    {
        factory = objectFactory;
        _service = service;
    }

    public int Constructor => -1518699091;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(_id, true);
            writer.WriteTLBytes(_bytes);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private long _id;
    public long Id
    {
        get => _id;
        set
        {
            serialized = false;
            _id = value;
        }
    }

    private byte[] _bytes;
    public byte[] Bytes
    {
        get => _bytes;
        set
        {
            serialized = false;
            _bytes = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        var auth = await _service.ImportAuthorization(_id, ctx.PermAuthKeyId!=0 ? ctx.PermAuthKeyId : ctx.AuthKeyId, _bytes);
        if(auth.AuthorizationType == Data.Auth.AuthorizationType.AuthBytesInvalid)
        {
            var resp = factory.Resolve<RpcError>();
            resp.ErrorCode = 400;
            resp.ErrorMessage = "AUTH_BYTES_INVALID";
            result.Result = resp;
        }
        else if (auth.AuthorizationType == Data.Auth.AuthorizationType.UserIdInvalid)
        {
            var resp = factory.Resolve<RpcError>();
            resp.ErrorCode = 400;
            resp.ErrorMessage = "USER_ID_INVALID";
            result.Result = resp;
        }
        else
        {
            var authorization = factory.Resolve<AuthorizationImpl>();
            var user = factory.Resolve<UserImpl>();
            user.Id = auth.User.Id;
            user.FirstName = auth.User.FirstName;
            user.LastName = auth.User.LastName;
            user.Phone = auth.User.Phone;
            user.Self = auth.User.Self;
            if (auth.User.Status == Data.UserStatus.Empty)
            {
                user.Status = factory.Resolve<UserStatusEmptyImpl>();
            }
            if (auth.User.Photo.Empty)
            {
                user.Photo = factory.Resolve<UserProfilePhotoEmptyImpl>();
            }
            authorization.User = user;
            result.Result = authorization;
        }
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _id = buff.ReadInt64(true);
        _bytes = buff.ReadTLBytes().ToArray();
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}