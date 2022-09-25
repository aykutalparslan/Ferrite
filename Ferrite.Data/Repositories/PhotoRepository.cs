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

using MessagePack;

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
    public bool PutProfilePhoto(long userId, long fileId, long accessHash, byte[] referenceBytes, DateTime date)
    {
        PhotoDTO photo = new PhotoDTO(false, fileId, accessHash, referenceBytes,
            (int)DateTimeOffset.Now.ToUnixTimeSeconds(), 
            null, 
            null, 2);
        var photoBytes = MessagePackSerializer.Serialize(photo);
        return _store.Put(photoBytes, userId, fileId);
    }

    public bool DeleteProfilePhoto(long userId, long fileId)
    {
        return _store.Delete(userId, fileId);
    }

    public IReadOnlyCollection<PhotoDTO> GetProfilePhotos(long userId)
    {
        List<PhotoDTO> photos = new();
        var iter = _store.Iterate(userId);
        foreach (var photoBytes in iter)
        {
            var photo = MessagePackSerializer.Deserialize<PhotoDTO>(photoBytes);
            var iterThumb = _storeThumb.Iterate(photo.Id);
            List<PhotoSizeDTO> photoSizes = new();
            foreach (var thumbBytes in iterThumb)
            {
                var thumb = MessagePackSerializer.Deserialize<ThumbnailDTO>(thumbBytes);
                PhotoSizeDTO size = new PhotoSizeDTO(PhotoSizeType.Default,
                    thumb.Type, thumb.Width, thumb.Height, 
                    thumb.Size, thumb.Bytes, thumb.Sizes);
            }

            photo = photo with { Sizes = photoSizes };
            photos.Add(photo);
        }

        return photos;
    }

    public PhotoDTO? GetProfilePhoto(long userId, long fileId)
    {
        var photoBytes = _store.Get(userId, fileId);
        if (photoBytes == null) return null;
        var photo = MessagePackSerializer.Deserialize<PhotoDTO>(photoBytes);
        var iterThumb = _storeThumb.Iterate(fileId);
        List<PhotoSizeDTO> photoSizes = new();
        foreach (var thumbBytes in iterThumb)
        {
            var thumb = MessagePackSerializer.Deserialize<ThumbnailDTO>(thumbBytes);
            PhotoSizeDTO size = new PhotoSizeDTO(PhotoSizeType.Default,
                thumb.Type, thumb.Width, thumb.Height, 
                thumb.Size, thumb.Bytes, thumb.Sizes);
        }

        photo = photo with { Sizes = photoSizes };
        return photo;
    }

    public bool PutThumbnail(ThumbnailDTO thumbnail)
    {
        var thumbBytes = MessagePackSerializer.Serialize(thumbnail);
        return _storeThumb.Put(thumbBytes, thumbnail.FileId,
            thumbnail.ThumbnailFileId, thumbnail.Type);
    }

    public IReadOnlyCollection<ThumbnailDTO> GetThumbnails(long photoId)
    {
        List<ThumbnailDTO> thumbs = new();
        var iter = _storeThumb.Iterate(photoId);
        foreach (var thumbBytes in iter)
        {
            var thumb = MessagePackSerializer.Deserialize<ThumbnailDTO>(thumbBytes);
            thumbs.Add(thumb);
        }

        return thumbs;
    }
}