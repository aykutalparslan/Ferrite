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
            if (u.UserId == 0) continue;
            var user = GetUserInternal(u.UserId);
            if (user != null)
            {
                users.Add(user);
            }
        }

        return new ServiceResult<ICollection<UserDTO>>(users, true, ErrorMessages.None);
    }
    public UserDTO? GetUser(long userId)
    {
        return GetUserInternal(userId);
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
        UserDTO? user = GetUserInternal(userId);
        DeviceType deviceType = GetDeviceType(authKeyId);
        PeerNotifySettingsDTO notifySettings = GetPeerNotifySettings(authKeyId, user, deviceType);

        if (user != null)
        {
            Data.UserFullDTO fullUser = CreteFullUser(id, user, notifySettings);
            return new ServiceResult<UserFullDTO>(new UserFullDTO(fullUser, new List<ChatDTO>(), 
                new List<UserDTO>(){user with
                {
                    Self = self
                }}), true, ErrorMessages.None);
        }

        return new ServiceResult<UserFullDTO>(null, false, ErrorMessages.UserIdInvalid);
    }

    private Data.UserFullDTO CreteFullUser(InputUserDTO id, UserDTO user, PeerNotifySettingsDTO notifySettings)
    {
        var profilePhoto = _unitOfWork.PhotoRepository.GetProfilePhoto(user.Id, user.Photo.PhotoId);
        if (profilePhoto != null)
        {
            profilePhoto = profilePhoto with
            {
                Sizes = _unitOfWork.PhotoRepository
                    .GetThumbnails(profilePhoto.Id).Select(t =>
                        new PhotoSizeDTO(PhotoSizeType.Default,
                            t.Type,
                            t.Width,
                            t.Height,
                            t.Size,
                            t.Bytes,
                            t.Sizes)).ToList()
            };
        }
        
        PeerSettingsDTO settingsDto = GeneratePeerSettings(id);
        var fullUser = new Ferrite.Data.UserFullDTO
        {
            About = user.About,
            Blocked = false,
            Id = user.Id,
            Settings = settingsDto,
            NotifySettings = notifySettings,
            PhoneCallsAvailable = true,
            PhoneCallsPrivate = true,
            CommonChatsCount = 0,
            ProfilePhoto = profilePhoto,
        };
        return fullUser;
    }

    private static PeerSettingsDTO GeneratePeerSettings(InputUserDTO id)
    {
        PeerSettingsDTO settingsDto = new PeerSettingsDTO(true, true, true, false, false,
            false, false, false, false, null, null, null);
        if (id.InputUserType == InputUserType.Self)
        {
            settingsDto = new PeerSettingsDTO(false, false, false, false, false,
                false, false, false, false, null, null, null);
        }

        return settingsDto;
    }

    private PeerNotifySettingsDTO GetPeerNotifySettings(long authKeyId, UserDTO? user, DeviceType deviceType)
    {
        if (user == null) return new PeerNotifySettingsDTO();
        var settings = 
            _unitOfWork.NotifySettingsRepository.GetNotifySettings(authKeyId, new InputNotifyPeerDTO
        {
            NotifyPeerType = InputNotifyPeerType.Peer,
            Peer = new InputPeerDTO
            {
                UserId = user.Id,
                AccessHash = user.AccessHash,
                InputPeerType = InputPeerType.User
            }
        });
        PeerNotifySettingsDTO notifySettings = notifySettings = settings.Count == 0
            ? new PeerNotifySettingsDTO()
            : settings.First(_ => _.DeviceType == deviceType);
        return notifySettings;
    }

    private DeviceType GetDeviceType(long authKeyId)
    {
        DeviceType deviceType = DeviceType.Other;
        var info = _unitOfWork.AppInfoRepository.GetAppInfo(authKeyId);
        if (info != null)
        {
            if (info.LangPack.ToLower().Contains("android"))
            {
                deviceType = DeviceType.Android;
            }
            else if (info.LangPack.ToLower().Contains("ios"))
            {
                deviceType = DeviceType.iOS;
            }
        }

        return deviceType;
    }

    private UserDTO? GetUserInternal(long userId)
    {
        var user = _unitOfWork.UserRepository.GetUser(userId);
        if (user != null)
        {
            user.Status = _unitOfWork.UserStatusRepository.GetUserStatus(user.Id);
        }
        return user;
    }

    public async Task<ServiceResult<bool>> SetSecureValueErrors(long authKeyId, InputUserDTO id, ICollection<SecureValueErrorDTO> errors)
    {
        throw new NotImplementedException();
    }
}