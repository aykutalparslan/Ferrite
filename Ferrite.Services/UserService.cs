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
using Ferrite.Data;
using Ferrite.Data.Repositories;
using Ferrite.Data.Users;
using Ferrite.TL.slim;
using Ferrite.TL.slim.layer150;
using Ferrite.TL.slim.layer150.users;
using UserFullDTO = Ferrite.Data.Users.UserFullDTO;

namespace Ferrite.Services;

public class UserService : IUsersService
{
    private readonly IUnitOfWork _unitOfWork;
    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async ValueTask<TLBytes> GetUsers(long authKeyId, TLBytes q)
    {
        List<long> userIds = GetUserIds(q);
        List<TLBytes> users = await GetUsersFromRepo(userIds);
        var usersBytes = users.ToVector().ToReadOnlySpan().ToArray();
        return new TLBytes(usersBytes, 0, usersBytes.Length);
    }

    private async ValueTask<List<TLBytes>> GetUsersFromRepo(List<long> userIds)
    {
        List<TLBytes> result = new();
        foreach (var u in userIds)
        {
            var user = await GetUserInternal(u);
            if (user != null)
            {
                result.Add(user.Value);
            }
        }

        return result;
    }

    private List<long> GetUserIds(TLBytes q)
    {
        List<long> ids = new();
        var users = ((GetUsers)q).Id;
        for (int i = 0; i < users.Count; i++)
        {
            var user = users.ReadTLObject();
            int constructor = MemoryMarshal.Read<int>(user);
            switch (constructor)
            {
                case Constructors.layer150_InputUser:
                    var inputUser = (InputUser)user;
                    ids.Add(inputUser.UserId);
                    break;
                case Constructors.layer150_InputPeerUserFromMessage:
                    var inputUserFromMessage = (InputPeerUserFromMessage)user;
                    ids.Add(inputUserFromMessage.UserId);
                    break;
            }
        }

        return ids;
    }

    public async Task<ServiceResult<UserFullDTO>> GetFullUser(long authKeyId, InputUserDTO id)
    {
        /*var userId = id.UserId;
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

        return new ServiceResult<UserFullDTO>(null, false, ErrorMessages.UserIdInvalid);*/
        throw new NotImplementedException();
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
        /*if (user == null) return new PeerNotifySettingsDTO();
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
        return notifySettings;*/
        throw new NotImplementedException();
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

    private async ValueTask<TLBytes?> GetUserInternal(long userId)
    {
        var user = _unitOfWork.UserRepository.GetUser(userId);
        if (user == null) return null;
        var status = await _unitOfWork.UserStatusRepository.GetUserStatusAsync(((User)user).Id);
        return ((User)user).Clone().Status(status.AsSpan()).Build().TLBytes!.Value;
    }

    public async Task<ServiceResult<bool>> SetSecureValueErrors(long authKeyId, InputUserDTO id, ICollection<SecureValueErrorDTO> errors)
    {
        throw new NotImplementedException();
    }
}