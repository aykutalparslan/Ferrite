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

using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Data.Photos;
using Photo = Ferrite.Data.Photos.Photo;

namespace Ferrite.Services;

public class PhotosService : IPhotosService
{
    private readonly IPersistentStore _store;
    private readonly IRandomGenerator _random;
    public PhotosService(IPersistentStore store, IRandomGenerator random)
    {
        _store = store;
        _random = random;
    }
    public async Task<ServiceResult<Photo>> UpdateProfilePhoto(long authKeyId, InputPhoto id)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        var user = await _store.GetUserAsync(auth.UserId);
        var date = DateTimeOffset.Now;
        await _store.SaveProfilePhotoAsync(auth.UserId, id.Id, id.FileReference, date);
        var photoInner = new Data.Photo(false, id.Id, id.AccessHash, id.FileReference,
            (int)date.ToUnixTimeSeconds(), new List<PhotoSize>(), null, 1);
        var photo = new Photo(photoInner, new[] { user });
        return new ServiceResult<Photo>(photo, true, ErrorMessages.None);
    }

    public async Task<ServiceResult<Photo>> UploadProfilePhoto(long authKeyId, UploadedFileInfo? photo, UploadedFileInfo? video, double? videoStartTimestamp)
    {
        UploadedFileInfo? file = null;
        if (photo != null)
        {
            file = photo;
        } 
        else if (video != null)
        {
            file = video;
        }

        if (file == null)
        {
            return new ServiceResult<Photo>(null, false, ErrorMessages.PhotoFileMissing);
        }
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        var user = await _store.GetUserAsync(auth.UserId);
        var fileParts = await _store.GetFilePartsAsync(file.Id);
        if (fileParts.Count != file.Parts ||
            fileParts.First().PartNum != 0 ||
            fileParts.Last().PartNum != file.Parts - 1)
        {
            return new ServiceResult<Photo>(null, false, ErrorMessages.FilePartsInvalid);
        }
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
        await _store.SaveProfilePhotoAsync(auth.UserId, file.Id, reference, date);
        var photoInner = new Data.Photo(false, file.Id, file.AccessHash, reference,
            (int)date.ToUnixTimeSeconds(), new List<PhotoSize>(), null, 1);
        var result = new Photo(photoInner, new[] { user });
        return new ServiceResult<Photo>(result, true, ErrorMessages.None);
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

    public async Task<Photos> GetUserPhotos(long userId, int offset, long maxId, int limit)
    {
        throw new NotImplementedException();
    }
}