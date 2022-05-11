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

namespace Ferrite.TL.layer139.account;
public class UpdateUsername : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IAccountService _account;
    private bool serialized = false;
    public UpdateUsername(ITLObjectFactory objectFactory, IAccountService account)
    {
        factory = objectFactory;
        _account = account;
    }

    public int Constructor => 1040964988;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLString(_username);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private string _username;
    public string Username
    {
        get => _username;
        set
        {
            serialized = false;
            _username = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        var user = await _account.UpdateUsername(ctx.AuthKeyId, _username);
        if (user == null)
        {
            var err = factory.Resolve<RpcError>();
            err.ErrorCode = 400;
            err.ErrorMessage = "USERNAME_INVALID";
            result.Result = err;
            return result;
        }

        var userImpl = factory.Resolve<UserImpl>();
        userImpl.Id = user.Id;
        userImpl.FirstName = user.FirstName;
        userImpl.LastName = user.LastName;
        userImpl.Phone = user.Phone;
        userImpl.Self = user.Self;
        if(user.Status == Data.UserStatus.Empty)
        {
            userImpl.Status = factory.Resolve<UserStatusEmptyImpl>();
        }
        if (user.Photo.Empty)
        {
            userImpl.Photo = factory.Resolve<UserProfilePhotoEmptyImpl>();
        }

        result.Result = userImpl;
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _username = buff.ReadTLString();
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}