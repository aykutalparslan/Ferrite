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

using System.Buffers;
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
    private readonly IObjectStore _objectStore;
    private readonly IPhotoProcessor _photoProcessor;
    private readonly IRandomGenerator _random;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUploadService _upload;
    public PhotosService(IObjectStore objectStore, 
        IPhotoProcessor photoProcessor, IRandomGenerator random,
        IUnitOfWork unitOfWork, IUploadService upload)
    {
        _objectStore = objectStore;
        _photoProcessor = photoProcessor;
        _random = random;
        _unitOfWork = unitOfWork;
        _upload = upload;
    }
    public async Task<ServiceResult<PhotoDTO>> UpdateProfilePhoto(long authKeyId, InputPhotoDTO id)
    {
        /*var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return new ServiceResult<PhotoDTO>(null, false, ErrorMessages.InvalidAuthKey);
        }
        var user = _unitOfWork.UserRepository.GetUser(auth.UserId);
        if (user == null)
        {
            return new ServiceResult<PhotoDTO>(null, false, ErrorMessages.UserIdInvalid);
        }
        var date = DateTime.Now;
        _unitOfWork.PhotoRepository.PutProfilePhoto(auth.UserId, (long)id.Id, (long)id.AccessHash,id.FileReference, date);
        await _unitOfWork.SaveAsync();
        var photoInner = new Data.PhotoDTO(false, false, (long)id.Id, id.AccessHash, id.FileReference,
            (int)((DateTimeOffset)date).ToUnixTimeSeconds(), new List<PhotoSizeDTO>(), null, 1);
        user.Photo = new UserProfilePhotoDTO()
        {
            DcId = (int)photoInner.DcId,
            PhotoId = photoInner.Id,
        };
        user.ApplyMinPhoto = true;
        user.Min = true;
        user.Self = true;
        await _unitOfWork.SaveAsync();
        var photo = new PhotoDTO(photoInner, new[] { user });
        
        return new ServiceResult<PhotoDTO>(photo, true, ErrorMessages.None);*/
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<PhotoDTO>> UploadProfilePhoto(long authKeyId, InputFileDTO? photo, InputFileDTO? video, double? videoStartTimestamp)
    {
        /*var saveResult = await _upload.SaveFile(photo);
        if (!saveResult.Success)
        {
            return new ServiceResult<PhotoDTO>(null, false, saveResult.ErrorMessage);
        }
        var file = saveResult.Result;
        
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return new ServiceResult<PhotoDTO>(null, false, ErrorMessages.InvalidAuthKey);
        }
        var user = _unitOfWork.UserRepository.GetUser(auth.UserId);
        var date = DateTime.Now;
        var processResult = await ProcessPhoto(file, date);
        if (!processResult.Success || processResult.Result == null)
        {
            return new ServiceResult<PhotoDTO>(null, false, processResult.ErrorMessage);
        } 
        
        _unitOfWork.PhotoRepository.PutProfilePhoto(auth.UserId, file.Id, file.AccessHash, file.FileReference, date);
       
        user.Photo = new UserProfilePhotoDTO()
        {
            DcId = (int)processResult.Result.DcId,
            PhotoId = processResult.Result.Id,
        };
        _unitOfWork.UserRepository.PutUser(user);
        await _unitOfWork.SaveAsync();
        user.ApplyMinPhoto = true;
        user.Min = true;
        user.Self = true;
        var result = new PhotoDTO(processResult.Result, new[] { user });
        return new ServiceResult<PhotoDTO>(result, true, ErrorMessages.None);*/
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<Data.PhotoDTO>> ProcessPhoto(UploadedFileInfoDTO file, DateTime date)
    {
        Data.PhotoDTO result = await ProcessPhotoInternal(file, date);

        return new ServiceResult<Data.PhotoDTO>(result, true, ErrorMessages.None);
    }

    private async Task<Data.PhotoDTO> ProcessPhotoInternal(UploadedFileInfoDTO? file, DateTime date)
    {
        Data.PhotoDTO? result = null;
        var fileParts = _unitOfWork.FileInfoRepository.GetFileParts(file.Id);
        int size = 0;
        foreach (var part in fileParts)
        {
            size += part.PartSize;
        }

        using var imageData = UnmanagedMemoryAllocator.Allocate<byte>(size);
        int offset = 0;
        if (!file.IsBigFile)
        {
            foreach (var part in fileParts)
            {
                var partData = await _objectStore.GetFilePart(part.FileId, part.PartNum);
                offset = ReadFromStream(partData, imageData, offset);
            }
        }
        else
        {
            foreach (var part in fileParts)
            {
                var partData = await _objectStore.GetBigFilePart(part.FileId, part.PartNum);
                offset = ReadFromStream(partData, imageData, offset);
            }
        }

        (int w, int h) = _photoProcessor.GetImageSize(imageData.Span);
        if (w == 0 || h == 0)
        {
            result = new Data.PhotoDTO(true, false, file.Id,
                null, null, null,
                null, null, null);
        }
        else
        {
            List<ThumbnailDTO> thumbnails = new List<ThumbnailDTO>();
            await GenerateThumbnails(w, h, imageData, file, thumbnails);
            List<PhotoSizeDTO> photoSizes = new List<PhotoSizeDTO>();
            foreach (var thumbnail in thumbnails)
            {
                photoSizes.Add(new PhotoSizeDTO(PhotoSizeType.Default, thumbnail.Type,
                    thumbnail.Width, thumbnail.Height, thumbnail.Size, thumbnail.Bytes, thumbnail.Sizes));
            }

            result = new Data.PhotoDTO(false, false, file.Id, file.AccessHash, file.FileReference,
                (int)((DateTimeOffset)date).ToUnixTimeSeconds(), photoSizes, null, 2);
        }

        return result;
    }

    private static int ReadFromStream(Stream partData, IUnmanagedMemoryOwner<byte> imageData, int offset)
    {
        var remaining = (int)partData.Length;
        while (remaining > 0)
        {
            var read = partData.Read(imageData.Span[offset..]);
            offset += read;
            remaining -= read;
        }

        return offset;
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
        /*var thumbnail = _photoProcessor.GenerateThumbnail(imageData.Span, w, filter);
        var thumbId = _random.NextLong();
        await _objectStore.SaveFilePart(thumbId, 0, new MemoryStream(thumbnail));
        _unitOfWork.FileInfoRepository.PutFilePart(new FilePartDTO(thumbId, 0, thumbnail.Length));
        _unitOfWork.FileInfoRepository.PutFileInfo(new UploadedFileInfoDTO(thumbId, thumbnail.Length, 1,
            _random.NextLong(), "", null, DateTimeOffset.Now, false));
        var thumb = new ThumbnailDTO(file.Id, thumbId, type,
            thumbnail.Length, w, w, null, null);
        thumbnails.Add(thumb);
        _unitOfWork.PhotoRepository.PutThumbnail(thumb);
        await _unitOfWork.SaveAsync();*/
        throw new NotImplementedException();
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
                _unitOfWork.PhotoRepository.DeleteProfilePhoto(auth.UserId, reference.FileId))
            {
                result.Add((long)photo.Id);
            }
        }

        await _unitOfWork.SaveAsync();
        return result;
    }
    public async Task<PhotosDTO> GetUserPhotos(long authKeyId, int offset, long maxId, int limit)
    {
        /*var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null) return new PhotosDTO(Array.Empty<Data.PhotoDTO>(), Array.Empty<UserDTO>());
        var user = _unitOfWork.UserRepository.GetUser(auth.UserId);
        if(user == null) return new PhotosDTO(Array.Empty<Data.PhotoDTO>(), Array.Empty<UserDTO>());

        var profilePhotos = _unitOfWork.PhotoRepository.GetProfilePhotos(auth.UserId);
        if (auth.UserId == user.Id)
        {
            user = user with
            {
                Self = true,
                Min = true,
                ApplyMinPhoto = true,
            };
        }
        List<Data.PhotoDTO> photos = new();
        foreach (var p in profilePhotos)
        {
            var thumbs = _unitOfWork.PhotoRepository
                .GetThumbnails(p.Id).Select(t => 
                    new PhotoSizeDTO(PhotoSizeType.Default,
                        t.Type,
                        t.Width,
                        t.Height,
                        t.Size,
                        t.Bytes,
                        t.Sizes)).ToList();
            photos.Add(p with{Sizes = thumbs});
        }
        return new PhotosDTO(photos, new List<UserDTO> { user });*/
        throw new NotImplementedException();
    }

    public async Task<Data.PhotoDTO> GetPhoto(long authKeyId, InputPhotoDTO inputPhoto)
    {
        /*if (inputPhoto.Empty)
        {
            return new Data.PhotoDTO(true, false, (long)inputPhoto.Id!,
                null, null, null, null, 
                null, null);
        }
        var thumbs = _unitOfWork.PhotoRepository
            .GetThumbnails((long)inputPhoto.Id!).Select(t => 
                new PhotoSizeDTO(PhotoSizeType.Default,
                    t.Type,
                    t.Width,
                    t.Height,
                    t.Size,
                    t.Bytes,
                    t.Sizes)).ToList();
        var reference = _unitOfWork.FileInfoRepository.GetFileReference(inputPhoto.FileReference!);
        if (reference == null)
        {
            return new Data.PhotoDTO(true, false, (long)inputPhoto.Id!,
                null, null, null, null, 
                null, null);
        }
        var file = reference.IsBigfile
            ? _unitOfWork.FileInfoRepository.GetBigFileInfo(reference.FileId)
            : _unitOfWork.FileInfoRepository.GetFileInfo(reference.FileId);
        
        if (file == null || file.AccessHash != inputPhoto.AccessHash)
        {
            return new Data.PhotoDTO(true, false, (long)inputPhoto.Id!,
                null, null, null, null, 
                null, null);
        }
        
        return new Data.PhotoDTO(false, false, (long)inputPhoto.Id!,
            file.AccessHash, reference.ReferenceBytes, 
            (int)file.SavedOn.ToUnixTimeSeconds(), thumbs, 
            null, 2);*/
        throw new NotImplementedException();
    }
}