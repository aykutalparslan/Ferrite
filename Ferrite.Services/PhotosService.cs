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
using Ferrite.Data.Repositories;
using PhotoDTO = Ferrite.Data.Photos.PhotoDTO;

namespace Ferrite.Services;

public class PhotosService : IPhotosService
{
    private readonly IPersistentStore _store;
    private readonly IObjectStore _objectStore;
    private readonly IPhotoProcessor _photoProcessor;
    private readonly IRandomGenerator _random;
    private readonly IUnitOfWork _unitOfWork;
    public PhotosService(IPersistentStore store, IObjectStore objectStore, 
        IPhotoProcessor photoProcessor, IRandomGenerator random,
        IUnitOfWork unitOfWork)
    {
        _store = store;
        _objectStore = objectStore;
        _photoProcessor = photoProcessor;
        _random = random;
        _unitOfWork = unitOfWork;
    }
    public async Task<ServiceResult<PhotoDTO>> UpdateProfilePhoto(long authKeyId, InputPhotoDTO id)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var user = _unitOfWork.UserRepository.GetUser(auth.UserId);
        var date = DateTime.Now;
        await _store.SaveProfilePhotoAsync(auth.UserId, id.Id, id.AccessHash,id.FileReference, date);
        var photoInner = new Data.PhotoDTO(false, id.Id, id.AccessHash, id.FileReference,
            (int)((DateTimeOffset)date).ToUnixTimeSeconds(), new List<PhotoSizeDTO>(), null, 1);
        var photo = new PhotoDTO(photoInner, new[] { user });
        return new ServiceResult<PhotoDTO>(photo, true, ErrorMessages.None);
    }

    public async Task<ServiceResult<PhotoDTO>> UploadProfilePhoto(long authKeyId, InputFileDTO? photo, InputFileDTO? video, double? videoStartTimestamp)
    {
        UploadedFileInfoDTO? file = null;
        int size = 0;
        IReadOnlyCollection<FilePartDTO> fileParts;
        if (photo != null)
        {
            fileParts = _unitOfWork.FileInfoRepository.GetFileParts(photo.Id);
            if (fileParts.Count != photo.Parts ||
                fileParts.First().PartNum != 0 ||
                fileParts.Last().PartNum != photo.Parts - 1)
            {
                return new ServiceResult<PhotoDTO>(null, false, ErrorMessages.FilePartsInvalid);
            }
            foreach (var part in fileParts)
            {
                size += part.PartSize;
            }
            if (size > 5242880)
            {
                return new ServiceResult<PhotoDTO>(null, false, ErrorMessages.PhotoFileTooBig);
            }

            var accessHash = _random.NextLong();
            file = new UploadedFileInfoDTO(photo.Id, fileParts.First().PartSize, photo.Parts,
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
            return new ServiceResult<PhotoDTO>(null, false, ErrorMessages.PhotoFileMissing);
        }
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var user = _unitOfWork.UserRepository.GetUser(auth.UserId);
        
        if (file.IsBigFile)
        {
            _unitOfWork.FileInfoRepository.PutBigFileInfo(file);
        }
        else
        {
            _unitOfWork.FileInfoRepository.PutFileInfo(file);
        }
        var date = DateTime.Now;
        byte[] reference = _random.GetRandomBytes(16);
        _unitOfWork.FileInfoRepository.PutFileReference(new FileReferenceDTO(reference, file.Id, file.IsBigFile));
        await _unitOfWork.SaveAsync();
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
            return new ServiceResult<PhotoDTO>(null, false, ErrorMessages.PhotoFileInvalid);
        }

        List<ThumbnailDTO> thumbnails = new List<ThumbnailDTO>();
        await GenerateThumbnails(w, h, imageData, file, thumbnails);
        List<PhotoSizeDTO> photoSizes = new List<PhotoSizeDTO>();
        foreach (var thumbnail in thumbnails)
        {
            photoSizes.Add(new PhotoSizeDTO(PhotoSizeType.Default, thumbnail.Type, 
                thumbnail.Width, thumbnail.Height, thumbnail.Size, thumbnail.Bytes, thumbnail.Sizes));
        }
        
        await _store.SaveProfilePhotoAsync(auth.UserId, file.Id, file.AccessHash, reference, date);
        var photoInner = new Data.PhotoDTO(false, file.Id, file.AccessHash, reference,
            (int)((DateTimeOffset)date).ToUnixTimeSeconds(), photoSizes, null, 2);
        var result = new PhotoDTO(photoInner, new[] { user });
        return new ServiceResult<PhotoDTO>(result, true, ErrorMessages.None);
    }

    private async Task GenerateThumbnails(int w, int h, IUnmanagedMemoryOwner<byte> imageData, 
        UploadedFileInfoDTO file, List<ThumbnailDTO> thumbnails)
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
            await GenerateThumbnail(imageData, file, 320, "b", ImageFilter.Crop, thumbnails);
        }
        else if (w >= 320 || h >= 320)
        {
            await GenerateThumbnail(imageData, file, 320, "m", ImageFilter.Box, thumbnails);
        }

        if (w >= 640 && h >= 640)
        {
            await GenerateThumbnail(imageData, file, 640, "c", ImageFilter.Crop, thumbnails);
        }

        if (w >= 800 || h >= 800)
        {
            await GenerateThumbnail(imageData, file, 800, "x", ImageFilter.Box, thumbnails);
        }

        if (w >= 1280 && h >= 1280)
        {
            await GenerateThumbnail(imageData, file, 1280, "d", ImageFilter.Crop, thumbnails);
        }

        if (w >= 1280 || h >= 1280)
        {
            await GenerateThumbnail(imageData, file, 1280, "y", ImageFilter.Box, thumbnails);
        }

        if (w >= 2560 || h >= 2560)
        {
            await GenerateThumbnail(imageData, file, 2560, "w", ImageFilter.Box, thumbnails);
        }
    }

    private async Task GenerateThumbnail(IUnmanagedMemoryOwner<byte> imageData,
        UploadedFileInfoDTO file, int w, string type, ImageFilter filter, List<ThumbnailDTO> thumbnails)
    {
        var thumbnail = _photoProcessor.GenerateThumbnail(imageData.Span, w, filter);
        var thumbId = _random.NextLong();
        await _objectStore.SaveFilePart(thumbId, 0, new MemoryStream(thumbnail));
        _unitOfWork.FileInfoRepository.PutFilePart(new FilePartDTO(thumbId, 0, thumbnail.Length));
        _unitOfWork.FileInfoRepository.PutFileInfo(new UploadedFileInfoDTO(thumbId, thumbnail.Length, 1,
            _random.NextLong(), "", null, DateTimeOffset.Now, false));
        var thumb = new ThumbnailDTO(file.Id, thumbId, type,
            thumbnail.Length, w, w, null, null);
        thumbnails.Add(thumb);
        await _store.SaveThumbnailAsync(thumb);
        await _unitOfWork.SaveAsync();
    }

    public async Task<IReadOnlyCollection<long>> DeletePhotos(long authKeyId, IReadOnlyCollection<InputPhotoDTO> photos)
    {
        List<long> result = new();
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        foreach (var photo in photos)
        {
            var reference = _unitOfWork.FileInfoRepository.GetFileReference(photo.FileReference);
            UploadedFileInfoDTO? file = null;
            if (reference.IsBigfile)
            {
                file = _unitOfWork.FileInfoRepository.GetBigFileInfo(reference.FileId);
            }
            else
            {
                file = _unitOfWork.FileInfoRepository.GetFileInfo(reference.FileId);
            }

            if (file != null && photo.AccessHash == file.AccessHash &&
                await _store.DeleteProfilePhotoAsync(auth.UserId, reference.FileId))
            {
                result.Add(photo.Id);
            }
        }
        
        return result;
    }
    public async Task<PhotosDTO> GetUserPhotos(long authKeyId, int offset, long maxId, int limit)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var user = _unitOfWork.UserRepository.GetUser(auth.UserId);
        if (auth.UserId == user.Id)
        {
            user = user with { Self = true };
        }
        var profilePhotos = await _store.GetProfilePhotosAsync(auth.UserId);
        return new PhotosDTO(profilePhotos, new List<UserDTO> { user });
    }
}