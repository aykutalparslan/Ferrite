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

using System.Runtime.InteropServices;
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Data.Photos;
using Photo = Ferrite.Data.Photos.Photo;

namespace Ferrite.Services;

public class PhotosService : IPhotosService
{
    private readonly IPersistentStore _store;
    private readonly IDistributedObjectStore _objectStore;
    private readonly IPhotoProcessor _photoProcessor;
    private readonly IRandomGenerator _random;
    public PhotosService(IPersistentStore store, IDistributedObjectStore objectStore, 
        IPhotoProcessor photoProcessor, IRandomGenerator random)
    {
        _store = store;
        _objectStore = objectStore;
        _photoProcessor = photoProcessor;
        _random = random;
    }
    public async Task<ServiceResult<Photo>> UpdateProfilePhoto(long authKeyId, InputPhoto id)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        var user = await _store.GetUserAsync(auth.UserId);
        var date = DateTimeOffset.Now;
        await _store.SaveProfilePhotoAsync(auth.UserId, id.Id, id.AccessHash,id.FileReference, date);
        var photoInner = new Data.Photo(false, id.Id, id.AccessHash, id.FileReference,
            (int)date.ToUnixTimeSeconds(), new List<PhotoSize>(), null, 1);
        var photo = new Photo(photoInner, new[] { user });
        return new ServiceResult<Photo>(photo, true, ErrorMessages.None);
    }

    public async Task<ServiceResult<Photo>> UploadProfilePhoto(long authKeyId, InputFile? photo, InputFile? video, double? videoStartTimestamp)
    {
        UploadedFileInfo? file = null;
        int size = 0;
        IReadOnlyCollection<FilePart> fileParts;
        if (photo != null)
        {
            fileParts = await _store.GetFilePartsAsync(photo.Id);
            if (fileParts.Count != photo.Parts ||
                fileParts.First().PartNum != 0 ||
                fileParts.Last().PartNum != photo.Parts - 1)
            {
                return new ServiceResult<Photo>(null, false, ErrorMessages.FilePartsInvalid);
            }
            foreach (var part in fileParts)
            {
                size += part.PartSize;
            }
            if (size > 5242880)
            {
                return new ServiceResult<Photo>(null, false, ErrorMessages.PhotoFileTooBig);
            }

            var accessHash = _random.NextLong();
            file = new UploadedFileInfo(photo.Id, fileParts.First().PartSize, photo.Parts,
                accessHash, photo.Name, photo.MD5Checksum, DateTimeOffset.Now, photo.IsBigfile);
        } 
        /*else if (video != null)
        {
            fileParts = await _store.GetFilePartsAsync(video.Id);
            if (fileParts.Count != video.Parts ||
                fileParts.First().PartNum != 0 ||
                fileParts.Last().PartNum != video.Parts - 1)
            {
                return new ServiceResult<Photo>(null, false, ErrorMessages.FilePartsInvalid);
            }
            foreach (var part in fileParts)
            {
                size += part.PartSize;
            }
            if (size > 5242880)
            {
                return new ServiceResult<Photo>(null, false, ErrorMessages.PhotoFileTooBig);
            }

            var accessHash = _random.NextLong();
            file = new UploadedFileInfo(video.Id, fileParts.First().PartSize, video.Parts,
                accessHash, photo.Name, video.MD5Checksum, DateTimeOffset.Now, video.IsBigfile);
        }*/
        else
        {
            return new ServiceResult<Photo>(null, false, ErrorMessages.PhotoFileMissing);
        }
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        var user = await _store.GetUserAsync(auth.UserId);
        
        if (file.IsBigFile)
        {
            await _store.SaveBigFileInfoAsync(file);
        }
        else
        {
            await _store.SaveFileInfoAsync(file);
        }
        var date = DateTimeOffset.Now;
        byte[] reference = _random.GetRandomBytes(16);
        await _store.SaveFileReferenceAsync(new FileReference(reference, file.Id, file.IsBigFile));

        using var imageData = UnmanagedMemoryAllocator.Allocate<byte>(size);
        int offset = 0;
        if (!file.IsBigFile)
        {
            foreach (var part in fileParts)
            {
                var partData = await _objectStore.GetFilePart(part.FileId, part.PartNum);
                int read = partData.ReadAtLeast((int)partData.Length,
                    imageData.Span.Slice(offset, part.PartSize));
                offset += part.PartSize;
            }
        }
        else
        {
            foreach (var part in fileParts)
            {
                var partData = await _objectStore.GetBigFilePart(part.FileId, part.PartNum);
                int read = partData.ReadAtLeast((int)partData.Length,
                    imageData.Span.Slice(offset, part.PartSize));
                offset += part.PartSize;
            }
        }
        
        (int w, int h) = _photoProcessor.GetImageSize(imageData.Span);
        if(w == 0 || h == 0)
        {
            return new ServiceResult<Photo>(null, false, ErrorMessages.PhotoFileInvalid);
        }

        List<Thumbnail> thumbnails = new List<Thumbnail>();
        await GenerateThumbnails(w, h, imageData, file, thumbnails);
        List<PhotoSize> photoSizes = new List<PhotoSize>();
        foreach (var thumbnail in thumbnails)
        {
            photoSizes.Add(new PhotoSize(PhotoSizeType.Default, thumbnail.Type, 
                thumbnail.Width, thumbnail.Height, thumbnail.Size, thumbnail.Bytes, thumbnail.Sizes));
        }
        
        await _store.SaveProfilePhotoAsync(auth.UserId, file.Id, file.AccessHash, reference, date);
        var photoInner = new Data.Photo(false, file.Id, file.AccessHash, reference,
            (int)date.ToUnixTimeSeconds(), photoSizes, null, 1);
        var result = new Photo(photoInner, new[] { user });
        return new ServiceResult<Photo>(result, true, ErrorMessages.None);
    }

    private async Task GenerateThumbnails(int w, int h, IUnmanagedMemoryOwner<byte> imageData, 
        UploadedFileInfo file, List<Thumbnail> thumbnails)
    {
        if (w >= 160 && h >= 160)
        {
            await GenerateThumbnail(imageData, file, 160, "a", ImageFilter.Crop, thumbnails);
        }
        else
        {
            await GenerateThumbnail(imageData, file, 100, "s", ImageFilter.Box, thumbnails);
        }

        if (w >= 320 && h >= 320)
        {
            await GenerateThumbnail(imageData, file, 160, "a", ImageFilter.Crop, thumbnails);
        }
        else if (w >= 320 || h >= 320)
        {
            await GenerateThumbnail(imageData, file, 320, "a", ImageFilter.Box, thumbnails);
        }

        if (w >= 640 && h >= 640)
        {
            await GenerateThumbnail(imageData, file, 640, "a", ImageFilter.Crop, thumbnails);
        }

        if (w >= 800 || h >= 800)
        {
            await GenerateThumbnail(imageData, file, 800, "a", ImageFilter.Box, thumbnails);
        }

        if (w >= 1280 && h >= 1280)
        {
            await GenerateThumbnail(imageData, file, 1280, "a", ImageFilter.Crop, thumbnails);
        }

        if (w >= 1280 || h >= 1280)
        {
            await GenerateThumbnail(imageData, file, 1280, "a", ImageFilter.Box, thumbnails);
        }

        if (w >= 2560 || h >= 2560)
        {
            await GenerateThumbnail(imageData, file, 2560, "a", ImageFilter.Box, thumbnails);
        }
    }

    private async Task GenerateThumbnail(IUnmanagedMemoryOwner<byte> imageData,
        UploadedFileInfo file, int w, string type, ImageFilter filter, List<Thumbnail> thumbnails)
    {
        var thumbnail = _photoProcessor.GenerateThumbnail(imageData.Span, w, filter);
        var thumbId = _random.NextLong();
        await _objectStore.SaveFilePart(thumbId, 0, new MemoryStream(thumbnail));
        await _store.SaveFilePartAsync(new FilePart(thumbId, 0, thumbnail.Length));
        await _store.SaveFileInfoAsync(new UploadedFileInfo(thumbId, thumbnail.Length, 1,
            _random.NextLong(), "", null, DateTimeOffset.Now, false));
        var thumb = new Thumbnail(file.Id, thumbId, type,
            thumbnail.Length, w, w, null, null);
        thumbnails.Add(thumb);
        await _store.SaveThumbnailAsync(thumb);
    }

    public async Task<IReadOnlyCollection<long>> DeletePhotos(long authKeyId, IReadOnlyCollection<InputPhoto> photos)
    {
        List<long> result = new();
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        foreach (var photo in photos)
        {
            var reference = await _store.GetFileReferenceAsync(photo.FileReference);
            UploadedFileInfo? file = null;
            if (reference.IsBigfile)
            {
                file = await _store.GetBigFileInfoAsync(reference.FileId);
            }
            else
            {
                file = await _store.GetFileInfoAsync(reference.FileId);
            }

            if (file != null && photo.AccessHash == file.AccessHash &&
                await _store.DeleteProfilePhotoAsync(auth.UserId, reference.FileId))
            {
                result.Add(photo.Id);
            }
        }
        
        return result;
    }
    public async Task<Photos> GetUserPhotos(long authKeyId, long userId, int offset, long maxId, int limit)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        var user = await _store.GetUserAsync(userId);
        if (auth.UserId == user.Id)
        {
            user = user with { Self = true };
        }
        var profilePhotos = await _store.GetProfilePhotosAsync(userId);
        return new Photos(profilePhotos, new List<User> { user });
    }
}