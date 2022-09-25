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
using Ferrite.Data.Repositories;
using Ferrite.Data.Users;
using UserFullDTO = Ferrite.Data.Users.UserFullDTO;

namespace Ferrite.Services;

public class UserService : IUsersService
{
    private readonly IUnitOfWork _unitOfWork;
    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<ServiceResult<ICollection<UserDTO>>> GetUsers(long authKeyId, ICollection<InputUserDTO> id)
    {
        List<UserDTO> users = new();
        foreach (var u in id)
        {
            if (u.UserId != 0)
            {
                var user = _unitOfWork.UserRepository.GetUser(u.UserId);
                if (user != null)
                {
                    user.Status = _unitOfWork.UserStatusRepository.GetUserStatus(user.Id);
                    users.Add(user);
                }
            }
        }

        return new ServiceResult<ICollection<UserDTO>>(users, true, ErrorMessages.None);
    }

    public async Task<ServiceResult<UserFullDTO>> GetFullUser(long authKeyId, InputUserDTO id)
    {
        var userId = id.UserId;
        bool self = false;
        if (id.InputUserType == InputUserType.Self)
        {
            var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
            userId = auth.UserId;
            self = true;
        }
        var user = _unitOfWork.UserRepository.GetUser(userId);
        user.Status = _unitOfWork.UserStatusRepository.GetUserStatus(user.Id);
        var info = _unitOfWork.AppInfoRepository.GetAppInfo(authKeyId);
        DeviceType deviceType = DeviceType.Other;
        if (info.LangPack.ToLower().Contains("android"))
        {
            deviceType = DeviceType.Android;
        }
        else if (info.LangPack.ToLower().Contains("ios"))
        {
            deviceType = DeviceType.iOS;
        }

        var settings = _unitOfWork.NotifySettingsRepository.GetNotifySettings(authKeyId, new InputNotifyPeerDTO
        {
            NotifyPeerType = InputNotifyPeerType.Peer,
            Peer = new InputPeerDTO
            {
                UserId = user.Id,
                AccessHash = user.AccessHash,
                InputPeerType = InputPeerType.User
            }
        });
        PeerNotifySettingsDTO notifySettings = null;
        if (settings.Count == 0)
        {
            notifySettings = new PeerNotifySettingsDTO();
        }
        else
        {
            notifySettings = settings.First(_ => _.DeviceType == deviceType);
        }

        if (user != null)
        {
            var profilePhoto = _unitOfWork.PhotoRepository.GetProfilePhoto(user.Id, user.Photo.PhotoId);
            var fullUser = new Ferrite.Data.UserFullDTO
            {
                About = user.About,
                Blocked = false,
                Id = user.Id,
                Settings = new PeerSettingsDTO(true, true, true, false, false,
                    false, false, false, false, null, null, null),
                NotifySettings = notifySettings,
                PhoneCallsAvailable = true,
                PhoneCallsPrivate = true,
                CommonChatsCount = 0,
                ProfilePhoto = profilePhoto,
            };
            return new ServiceResult<UserFullDTO>(new UserFullDTO(fullUser, new List<ChatDTO>(), 
                new List<UserDTO>(){user with{Self = self}}), true, ErrorMessages.None);
        }

        return new ServiceResult<UserFullDTO>(null, false, ErrorMessages.UserIdInvalid);
    }

    public async Task<ServiceResult<bool>> SetSecureValueErrors(long authKeyId, InputUserDTO id, ICollection<SecureValueErrorDTO> errors)
    {
        throw new NotImplementedException();
    }
}