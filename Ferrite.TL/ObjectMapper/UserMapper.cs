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
using Ferrite.TL.currentLayer;

namespace Ferrite.TL.ObjectMapper;

public class UserMapper : ITLObjectMapper<User, UserDTO>
{
    private readonly ITLObjectFactory _factory;
    public UserMapper(ITLObjectFactory factory)
    {
        _factory = factory;
    }
    public UserDTO MapToDTO(User obj)
    {
        throw new NotImplementedException();
    }
    public User MapToTLObject(UserDTO obj)
    {
        var userImpl = _factory.Resolve<UserImpl>();
        userImpl.Id = obj.Id;
        userImpl.FirstName = obj.FirstName;
        userImpl.LastName = obj.LastName;
        userImpl.Phone = obj.Phone;
        userImpl.Self = obj.Self;
        if (obj.Username?.Length > 0)
        {
            userImpl.Username = obj.Username;
        }
        if(obj.Status == null || obj.Status.Status == UserStatusType.Empty)
        {
            userImpl.Status = _factory.Resolve<UserStatusEmptyImpl>();
        }
        else if(obj.Status.Status == UserStatusType.Offline)
        {
            var status = _factory.Resolve<UserStatusOfflineImpl>();
            status.WasOnline = obj.Status.WasOnline ?? 0;
            userImpl.Status = status;

        }
        else if(obj.Status.Status == UserStatusType.Online)
        {
            userImpl.Status = _factory.Resolve<UserStatusOnlineImpl>();
        }
        else if(obj.Status.Status == UserStatusType.Recently)
        {
            userImpl.Status = _factory.Resolve<UserStatusRecentlyImpl>();
        }
        else if(obj.Status.Status == UserStatusType.LastWeek)
        {
            userImpl.Status = _factory.Resolve<UserStatusLastWeekImpl>();
        }
        else if(obj.Status.Status == UserStatusType.LastMonth)
        {
            userImpl.Status = _factory.Resolve<UserStatusLastMonthImpl>();
        }
        if (obj.Photo.Empty)
        {
            userImpl.Photo = _factory.Resolve<UserProfilePhotoEmptyImpl>();
        }
        else
        {
            var photo = _factory.Resolve<UserProfilePhotoImpl>();
            photo.DcId = obj.Photo.DcId;
            photo.PhotoId = obj.Photo.PhotoId;
            photo.HasVideo = obj.Photo.HasVideo;
            if (obj.Photo.StrippedThumb is { Length: > 0 })
            {
                photo.StrippedThumb = obj.Photo.StrippedThumb;
            }
            userImpl.Photo = photo;
        }

        return userImpl;
    }
}