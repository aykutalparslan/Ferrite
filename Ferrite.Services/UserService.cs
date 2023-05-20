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
using Ferrite.TL.slim.baseLayer;
using Ferrite.TL.slim.baseLayer.users;
using TLUserFull = Ferrite.TL.slim.baseLayer.users.TLUserFull;
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
            case Constructors.baseLayer_InputUser:
                var inputUser = (InputUser)user;
                userId = inputUser.UserId;
                break;
            case Constructors.baseLayer_InputPeerUserFromMessage:
                var inputUserFromMessage = (InputPeerUserFromMessage)user;
                userId = inputUserFromMessage.UserId;
                break;
        }

        return (userId, constructor);
    }

    public async ValueTask<TLUserFull> GetFullUser(long authKeyId, TLBytes q)
    {
        var (userId, constructor) = GetUserId(((GetFullUser)q).Id);
        bool self = false;
        if (constructor == Constructors.baseLayer_InputUserSelf)
        {
            var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
            if(auth == null) return (TLUserFull)RpcErrorGenerator.GenerateError(400, "AUTH_KEY_INVALID"u8);
            userId = auth.Value.AsAuthInfo().UserId;
            self = true;
        }
        using var user = await GetUserInternal(userId);
        DeviceType deviceType = GetDeviceType(authKeyId);
        using var notifySettings = GetPeerNotifySettings(authKeyId, user, deviceType);

        if (user != null)
        {
            return CreteFullUser(user.Value, notifySettings);
        }

        return (TLUserFull)RpcErrorGenerator.GenerateError(400, "USER_ID_INVALID"u8);
    }

    private TLUserFull CreteFullUser(TLUser userBytes, TLPeerNotifySettings notifySettings)
    {
        var user = userBytes.AsUser();
        var photoConstructor = MemoryMarshal.Read<int>(user.Photo);
        long photoId = photoConstructor == Constructors.baseLayer_UserProfilePhotoEmpty 
            ? 0 
            : ((UserProfilePhoto)user.Photo).PhotoId;
        var profilePhoto = photoConstructor == Constructors.baseLayer_UserProfilePhotoEmpty
            ? PhotoEmpty.Builder().Build().TLBytes
            : _unitOfWork.PhotoRepository.GetProfilePhoto(user.Id, photoId);
        using var settings = GeneratePeerSettings(user.Self);
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

        using var finalUser = userfull.Build();
        var result = UsersUserFull.Builder()
            .FullUser(finalUser.ToReadOnlySpan())
            .Users(new Vector())
            .Chats(new Vector());
        return result.Build();
    }

    private static TLPeerSettings GeneratePeerSettings(bool self)
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

        return settings;
    }

    private TLPeerNotifySettings GetPeerNotifySettings(long authKeyId, TLBytes? user, DeviceType deviceType)
    {
        if (user == null) return PeerNotifySettings.Builder().Build();
        var settings =
            _unitOfWork.NotifySettingsRepository.GetNotifySettings(authKeyId, (int)InputNotifyPeerType.Peer,
                (int)InputPeerType.User, ((User)user).Id, (int)deviceType);

        var notifySettings = settings.Count == 0
            ? PeerNotifySettings.Builder().Build()
            : settings.First();
        return notifySettings;
    }

    private DeviceType GetDeviceType(long authKeyId)
    {
        DeviceType deviceType = DeviceType.Other;
        using var infoBytes = _unitOfWork.AppInfoRepository.GetAppInfo(authKeyId);
        if (infoBytes != null)
        {
            var info = infoBytes.Value.AsAppInfo();
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

    private async ValueTask<TLUser?> GetUserInternal(long userId)
    {
        using var user = _unitOfWork.UserRepository.GetUser(userId);
        if (user == null) return null;
        var status = await _unitOfWork.UserStatusRepository.GetUserStatusAsync(user.Value.AsUser().Id);
        return user.Value.AsUser().Clone().Status(status.AsSpan()).Build();
    }
}