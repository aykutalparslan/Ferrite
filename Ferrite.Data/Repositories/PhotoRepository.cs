// 
// Project Ferrite is an Implementation of the Telegram Server API
// Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 

using System.Text;
using Ferrite.TL.slim;
using Ferrite.TL.slim.layer150;
using Ferrite.TL.slim.layer150.dto;

namespace Ferrite.Data.Repositories;

public class PhotoRepository : IPhotoRepository
{
    private readonly IKVStore _store;
    private readonly IKVStore _storeThumb;
    public PhotoRepository(IKVStore store, IKVStore storeThumb)
    {
        _store = store;
        _store.SetSchema(new TableDefinition("ferrite", "profile_photos",
            new KeyDefinition("pk",
                new DataColumn { Name = "user_id", Type = DataType.Long },
                new DataColumn { Name = "file_id", Type = DataType.Long })));
        _storeThumb = storeThumb;
        _storeThumb.SetSchema(new TableDefinition("ferrite", "thumbnails",
            new KeyDefinition("pk",
                new DataColumn { Name = "file_id", Type = DataType.Long },
                new DataColumn { Name = "thumb_file_id", Type = DataType.Long },
                new DataColumn { Name = "thumb_type", Type = DataType.String })));
    }
    public bool PutProfilePhoto(long userId, long fileId, long accessHash, byte[] referenceBytes, DateTimeOffset date)
    {
        using var photoBytes = Photo.Builder()
            .Id(fileId)
            .AccessHash(accessHash)
            .FileReference(referenceBytes)
            .Date((int)date.ToUnixTimeSeconds())
            .DcId(2)
            .Sizes(new Vector()).Build().TLBytes!.Value;
        
        return _store.Put(photoBytes.AsSpan().ToArray(), userId, fileId);
    }

    public bool DeleteProfilePhoto(long userId, long fileId)
    {
        return _store.Delete(userId, fileId);
    }

    public IReadOnlyList<TLBytes> GetProfilePhotos(long userId)
    {
        List<TLBytes> photos = new();
        var iter = _store.Iterate(userId);
        foreach (var photoBytes in iter)
        {
            var photoSizes = GetPhotoSizes(((Photo)photoBytes.AsSpan()).Id);
            var photo = ((Photo)photoBytes.AsSpan()).Clone().Sizes(photoSizes).Build();
            photos.Add(photo.TLBytes!.Value);
        }

        return photos;
    }

    public TLBytes? GetProfilePhoto(long userId, long fileId)
    {
        var photoBytes = _store.Get(userId, fileId);
        if (photoBytes == null) return null;
        var photoSizes = GetPhotoSizes(((Photo)photoBytes.AsSpan()).Id);
        var photo = ((Photo)photoBytes.AsSpan()).Clone().Sizes(photoSizes).Build();
        return photo.TLBytes!.Value;
    }
    
    private Vector GetPhotoSizes(long photoId)
    {
        var iterThumb = _storeThumb.Iterate(photoId);
        Vector photoSizes = new();
        foreach (var thumbBytes in iterThumb)
        {
            var thumb = (Thumbnail)thumbBytes.AsSpan();
            photoSizes.AppendTLObject(thumb.PhotoSize);
        }

        return photoSizes;
    }

    public bool PutThumbnail(TLBytes thumbnail)
    {
        var thumb = (Thumbnail)thumbnail;
        var photoSize = (PhotoSize)thumb.PhotoSize.ToArray().AsSpan();
        return _storeThumb.Put(thumbnail.AsSpan().ToArray(), thumb.FileId,
            thumb.ThumbFileId, Encoding.UTF8.GetString(photoSize.Type));
    }

    public IReadOnlyList<TLBytes> GetThumbnails(long photoId)
    {
        List<TLBytes> thumbs = new();
        var iter = _storeThumb.Iterate(photoId);
        foreach (var thumbBytes in iter)
        {
            thumbs.Add(new TLBytes(thumbBytes, 0, thumbBytes.Length));
        }

        return thumbs;
    }
}