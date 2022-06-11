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
using UserFull = Ferrite.Data.Users.UserFull;

namespace Ferrite.Services;

public class UserService : IUsersService
{
    private readonly IPersistentStore _store;

    public UserService(IPersistentStore store)
    {
        _store = store;
    }
    public async Task<ServiceResult<ICollection<User>>> GetUsers(long authKeyId, ICollection<InputUser> id)
    {
        List<User> users = new();
        foreach (var u in id)
        {
            if (u.UserId != 0)
            {
                var user = await _store.GetUserAsync(u.UserId);
                if (user != null)
                {
                    users.Add(user);
                }
            }
        }

        return new ServiceResult<ICollection<User>>(users, true, ErrorMessages.None);
    }

    public async Task<ServiceResult<UserFull>> GetFullUser(long authKeyId, InputUser id)
    {
        var user = await _store.GetUserAsync(id.UserId);
        var notifySettings = await _store.GetNotifySettingsAsync(authKeyId, new InputNotifyPeer
        {
            NotifyPeerType = InputNotifyPeerType.Peer,
            Peer = new InputPeer
            {
                UserId = id.UserId,
                AccessHash = id.AccessHash,
                InputPeerType = InputPeerType.User
            }
        });
        if (user != null)
        {
            var fullUser = new Ferrite.Data.UserFull
            {
                About = user.About,
                Blocked = false,
                Id = user.Id,
                Settings = new PeerSettings(true, true, true, false, false,
                    false, false, false, false, null, null, null),
                NotifySettings = notifySettings,
                PhoneCallsAvailable = true,
                PhoneCallsPrivate = true,
                CommonChatsCount = 0,
            };
            return new ServiceResult<UserFull>(new UserFull(fullUser, new List<Chat>(), 
                new List<User>()), true, ErrorMessages.None);
        }

        return new ServiceResult<UserFull>(null, false, ErrorMessages.UserIdInvalid);
    }

    public async Task<ServiceResult<bool>> SetSecureValueErrors(long authKeyId, InputUser id, ICollection<SecureValueError> errors)
    {
        throw new NotImplementedException();
    }
}