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
using Ferrite.TL.layer139.help;
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.TL.layer139.auth;
public class SignIn : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IAuthService _auth;
    private bool serialized = false;
    public SignIn(ITLObjectFactory objectFactory, IAuthService auth)
    {
        factory = objectFactory;
        _auth = auth;
    }

    public int Constructor => -1126886015;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLString(_phoneNumber);
            writer.WriteTLString(_phoneCodeHash);
            writer.WriteTLString(_phoneCode);
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

    private string _phoneCodeHash;
    public string PhoneCodeHash
    {
        get => _phoneCodeHash;
        set
        {
            serialized = false;
            _phoneCodeHash = value;
        }
    }

    private string _phoneCode;
    public string PhoneCode
    {
        get => _phoneCode;
        set
        {
            serialized = false;
            _phoneCode = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var signInResult = await _auth.SignIn(ctx.AuthKeyId, _phoneNumber, _phoneCodeHash, _phoneCode);
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        if (signInResult != null &&
            signInResult.AuthorizationType == Data.Auth.AuthorizationType.SignUpRequired)
        {
            var signUpRequired = factory.Resolve<AuthorizationSignUpRequiredImpl>();
            result.Result = signUpRequired;
        }else if(signInResult != null &&
            signInResult.AuthorizationType == Data.Auth.AuthorizationType.PhoneCodeInvalid)
        {
            var resp = factory.Resolve<RpcError>();
            resp.ErrorCode = 400;
            resp.ErrorMessage = "PHONE_CODE_INVALID";
            result.Result = resp;
        }
        else if(signInResult != null)
        {
            var authorization = factory.Resolve<AuthorizationImpl>();
            var user = factory.Resolve<UserImpl>();
            user.Id = signInResult.User.Id;
            user.FirstName = signInResult.User.FirstName;
            user.LastName = signInResult.User.LastName;
            user.Phone = signInResult.User.Phone;
            user.Self = signInResult.User.Self;
            if(signInResult.User.Status == Data.UserStatus.Empty)
            {
                user.Status = factory.Resolve<UserStatusEmptyImpl>();
            }
            if (signInResult.User.Photo.Empty)
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
        _phoneNumber = buff.ReadTLString();
        _phoneCodeHash = buff.ReadTLString();
        _phoneCode = buff.ReadTLString();
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}