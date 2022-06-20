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

using Ferrite.Data;
using Ferrite.Data.Photos;
using Photo = Ferrite.Data.Photos.Photo;

namespace Ferrite.Services;

public class PhotosService : IPhotosService
{
    private readonly IPersistentStore _store;
    public PhotosService(IPersistentStore store)
    {
        _store = store;
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
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyCollection<long>> DeletePhotos(long authKeyId, IReadOnlyCollection<InputPhoto> photos)
    {
        throw new NotImplementedException();
    }

    public async Task<Photos> GetUserPhotos(long userId, int offset, long maxId, int limit)
    {
        throw new NotImplementedException();
    }
}