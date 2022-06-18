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
using Ferrite.Data.Auth;
using Ferrite.Services;
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer.auth;
public class ExportLoginToken : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IAuthService _auth;
    private bool serialized = false;
    public ExportLoginToken(ITLObjectFactory objectFactory, IAuthService auth)
    {
        factory = objectFactory;
        _auth = auth;
    }

    public int Constructor => -1210022402;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt32(_apiId, true);
            writer.WriteTLString(_apiHash);
            writer.Write(_exceptIds.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
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

    private VectorOfLong _exceptIds;
    public VectorOfLong ExceptIds
    {
        get => _exceptIds;
        set
        {
            serialized = false;
            _exceptIds = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        var token = await _auth.ExportLoginToken(ctx.PermAuthKeyId!=0 ? ctx.PermAuthKeyId : ctx.AuthKeyId, ctx.SessionId, _apiId, _apiHash, _exceptIds);
        if(token.LoginTokenType == LoginTokenType.TokenSuccess)
        {
            var resp = factory.Resolve<LoginTokenSuccessImpl>();
            var authorization = factory.Resolve<AuthorizationImpl>();
            var user = factory.Resolve<UserImpl>();
            user.Id = token.Authorization.User.Id;
            user.FirstName = token.Authorization.User.FirstName;
            user.LastName = token.Authorization.User.LastName;
            user.Phone = token.Authorization.User.Phone;
            user.Self = token.Authorization.User.Self;
            if (token.Authorization.User.Status == Data.UserStatus.Empty)
            {
                user.Status = factory.Resolve<UserStatusEmptyImpl>();
            }
            if (token.Authorization.User.Photo.Empty)
            {
                user.Photo = factory.Resolve<UserProfilePhotoEmptyImpl>();
            }
            authorization.User = user;
            resp.Authorization = authorization;
            result.Result = resp;
        }
        else
        {
            var resp = factory.Resolve<LoginTokenImpl>();
            resp.Expires = token.Expires;
            resp.Token = token.Token;
            result.Result = resp;
        }
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _apiId = buff.ReadInt32(true);
        _apiHash = buff.ReadTLString();
        buff.Skip(4); _exceptIds  =  factory . Read < VectorOfLong > ( ref  buff ) ; 
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}