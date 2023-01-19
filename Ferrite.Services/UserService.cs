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
using System.Text;
using Ferrite.Data;
using Ferrite.Data.Repositories;
using Ferrite.Data.Users;
using Ferrite.TL.slim;
using Ferrite.TL.slim.dto;
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
            var (userId, constructor) = GetUserId(user);
            ids.Add(userId);
        }

        return ids;
    }

    private static (long, int) GetUserId(Span<byte> user)
    {
        int constructor = MemoryMarshal.Read<int>(user);
        long userId = 0;
        switch (constructor)
        {
            case Constructors.layer150_InputUser:
                var inputUser = (InputUser)user;
                userId = inputUser.UserId;
                break;
            case Constructors.layer150_InputPeerUserFromMessage:
                var inputUserFromMessage = (InputPeerUserFromMessage)user;
                userId = inputUserFromMessage.UserId;
                break;
        }

        return (userId, constructor);
    }

    public async ValueTask<TLBytes> GetFullUser(long authKeyId, TLBytes q)
    {
        var (userId, constructor) = GetUserId(((GetFullUser)q).Id);
        bool self = false;
        if (constructor == Constructors.layer150_InputUserSelf)
        {
            var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
            userId = ((AuthInfo)auth).UserId;
            self = true;
        }
        var user = await GetUserInternal(userId);
        DeviceType deviceType = GetDeviceType(authKeyId);
        var notifySettings = GetPeerNotifySettings(authKeyId, user, deviceType);

        if (user != null)
        {
            return CreteFullUser(user.Value, notifySettings);
        }

        return RpcErrorGenerator.GenerateError(400, "USER_ID_INVALID"u8);
    }

    private TLBytes CreteFullUser(TLBytes userBytes, TLBytes notifySettings)
    {
        var user = ((User)userBytes);
        var photoConstructor = MemoryMarshal.Read<int>(user.Photo);
        long photoId = photoConstructor == Constructors.layer150_UserProfilePhotoEmpty 
            ? 0 
            : ((UserProfilePhoto)user.Photo).PhotoId;
        var profilePhoto = photoConstructor == Constructors.layer150_UserProfilePhotoEmpty
            ? PhotoEmpty.Builder().Build().TLBytes
            : _unitOfWork.PhotoRepository.GetProfilePhoto(user.Id, photoId);
        var settings = GeneratePeerSettings(user.Self);
        var about = _unitOfWork.UserRepository.GetAbout(user.Id);
        var userfull = UserFull.Builder()
            .Id(user.Id)
            .Blocked(false)
            .Settings(settings.AsSpan())
            .NotifySettings(notifySettings.AsSpan())
            .PhoneCallsAvailable(true)
            .PhoneCallsPrivate(true)
            .CommonChatsCount(0)
            .ProfilePhoto(profilePhoto!.Value.AsSpan());
        if (about != null)
        {
            userfull = userfull.About(Encoding.UTF8.GetBytes(about));
        }

        var result = UsersUserFull.Builder()
            .FullUser(userfull.Build().ToReadOnlySpan())
            .Users(new Vector())
            .Chats(new Vector());
        return result.Build().TLBytes!.Value;
    }

    private static TLBytes GeneratePeerSettings(bool self)
    {
        var settings = self
            ? PeerSettings.Builder()
                .ReportSpam(false)
                .AddContact(false)
                .BlockContact(false)
                .ShareContact(false)
                .NeedContactsException(false)
                .ReportGeo(false)
                .Autoarchived(false)
                .InviteMembers(false)
                .RequestChatBroadcast(false).Build()
            : PeerSettings.Builder()
                .ReportSpam(true)
                .AddContact(true)
                .BlockContact(true)
                .ShareContact(false)
                .NeedContactsException(false)
                .ReportGeo(false)
                .Autoarchived(false)
                .InviteMembers(false)
                .RequestChatBroadcast(false).Build();

        return settings.TLBytes!.Value;
    }

    private TLBytes GetPeerNotifySettings(long authKeyId, TLBytes? user, DeviceType deviceType)
    {
        if (user == null) return PeerNotifySettings.Builder().Build().TLBytes!.Value;
        var settings =
            _unitOfWork.NotifySettingsRepository.GetNotifySettings(authKeyId, (int)InputNotifyPeerType.Peer,
                (int)InputPeerType.User, ((User)user).Id, (int)deviceType);

        var notifySettings = settings.Count == 0
            ? PeerNotifySettings.Builder().Build().TLBytes!.Value
            : settings.First();
        return notifySettings;
    }

    private DeviceType GetDeviceType(long authKeyId)
    {
        DeviceType deviceType = DeviceType.Other;
        var infoBytes = _unitOfWork.AppInfoRepository.GetAppInfo(authKeyId);
        if (infoBytes != null)
        {
            var info = (AppInfo)infoBytes!;
            var langPack = Encoding.UTF8.GetString(info.LangPack).ToLower();
            if (langPack.Contains("android"))
            {
                deviceType = DeviceType.Android;
            }
            else if (langPack.Contains("ios"))
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
        var result = ((User)user).Clone().Status(status.AsSpan()).Build().TLBytes!.Value;
        return result;
    }
}