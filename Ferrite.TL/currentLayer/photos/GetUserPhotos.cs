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

namespace Ferrite.TL.currentLayer.photos;
public class GetUserPhotos : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IPersistentStore _store;
    private readonly IPhotosService _photos;
    private bool serialized = false;
    public GetUserPhotos(ITLObjectFactory objectFactory, IPersistentStore store, IPhotosService photos)
    {
        factory = objectFactory;
        _store = store;
        _photos = photos;
    }

    public int Constructor => -1848823128;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_userId.TLBytes, false);
            writer.WriteInt32(_offset, true);
            writer.WriteInt64(_maxId, true);
            writer.WriteInt32(_limit, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private InputUser _userId;
    public InputUser UserId
    {
        get => _userId;
        set
        {
            serialized = false;
            _userId = value;
        }
    }

    private int _offset;
    public int Offset
    {
        get => _offset;
        set
        {
            serialized = false;
            _offset = value;
        }
    }

    private long _maxId;
    public long MaxId
    {
        get => _maxId;
        set
        {
            serialized = false;
            _maxId = value;
        }
    }

    private int _limit;
    public int Limit
    {
        get => _limit;
        set
        {
            serialized = false;
            _limit = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var auth = await _store.GetAuthorizationAsync(ctx.CurrentAuthKeyId);
        var userPhotos = await _photos.GetUserPhotos(auth.AuthKeyId, 
            auth.UserId, _offset, _maxId, _limit);
        var photos = factory.Resolve<PhotosImpl>();
        photos.Photos = factory.Resolve<Vector<currentLayer.Photo>>();
        photos.Users = factory.Resolve<Vector<User>>();
        foreach (var p in userPhotos.PhotosInner)
        {
            var photo = factory.Resolve<currentLayer.PhotoImpl>();
            photo.Id = p.Id;
            photo.Date = p.Date;
            photo.Sizes = factory.Resolve<Vector<PhotoSize>>();
            foreach (var s in p.Sizes)
            {
                var size = factory.Resolve<PhotoSizeImpl>();
                size.Type = s.Type;
                size.Size = s.Size;
                size.H = s.H;
                size.W = s.W;
                photo.Sizes.Add(size);
            }
            photo.FileReference = p.FileReference;
            photo.Date = p.Date;
            photo.AccessHash = p.AccessHash;
            photo.DcId = p.DcId;
            photo.HasStickers = p.HasStickers;
            photo.VideoSizes = factory.Resolve<Vector<VideoSize>>();
            foreach (var s in p.VideoSizes)
            {
                var size = factory.Resolve<VideoSizeImpl>();
                size.Type = s.Type;
                size.Size = s.Size;
                size.H = s.H;
                size.W = s.W;
                size.VideoStartTs = s.VideoStartTs;
                photo.VideoSizes.Add(size);
            }
            photos.Photos.Add(photo);
        }

        foreach (var user in userPhotos.Users)
        {
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
            photos.Users.Add(userImpl);
        }

        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        result.Result = photos;
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _userId = (InputUser)factory.Read(buff.ReadInt32(true), ref buff);
        _offset = buff.ReadInt32(true);
        _maxId = buff.ReadInt64(true);
        _limit = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}