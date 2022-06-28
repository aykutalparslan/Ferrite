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
public class ChangePhone : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IAccountService _accountService;
    private bool serialized = false;
    public ChangePhone(ITLObjectFactory objectFactory, IAccountService accountService)
    {
        factory = objectFactory;
        _accountService = accountService;
    }

    public int Constructor => 1891839707;
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
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        var serviceResult = await _accountService.ChangePhone(ctx.PermAuthKeyId!=0 ? 
            ctx.PermAuthKeyId : ctx.AuthKeyId, _phoneNumber, _phoneCodeHash, _phoneCode);
        if (!serviceResult.Success)
        {
            var err = factory.Resolve<RpcError>();
            err.ErrorCode = serviceResult.ErrorMessage.Code;
            err.ErrorMessage = serviceResult.ErrorMessage.Message;
            result.Result = err;
        }
        else
        {
            var user = serviceResult.Result;
            var userImpl = factory.Resolve<UserImpl>();
            userImpl.Id = user.Id;
            userImpl.FirstName = user.FirstName;
            userImpl.LastName = user.LastName;
            userImpl.Phone = user.Phone;
            userImpl.Self = user.Self;
            if (user.Username?.Length > 0)
            {
                userImpl.Username = user.Username;
            }
            if(user.Status == Data.UserStatus.Empty)
            {
                userImpl.Status = factory.Resolve<UserStatusEmptyImpl>();
            }
            if (user.Photo.Empty)
            {
                userImpl.Photo = factory.Resolve<UserProfilePhotoEmptyImpl>();
            }
            else
            {
                var photo = factory.Resolve<UserProfilePhotoImpl>();
                photo.DcId = user.Photo.DcId;
                photo.PhotoId = user.Photo.PhotoId;
                photo.HasVideo = user.Photo.HasVideo;
                if (user.Photo.StrippedThumb is { Length: > 0 })
                {
                    photo.StrippedThumb = user.Photo.StrippedThumb;
                }
                userImpl.Photo = photo;
            }
            result.Result = userImpl;
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