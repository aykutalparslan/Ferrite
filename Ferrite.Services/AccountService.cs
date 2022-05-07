//
//  Project Ferrite is an Implementation of the Telegram Server API
//  Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Affero General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Affero General Public License for more details.
//
//  You should have received a copy of the GNU Affero General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using Ferrite.Data;

namespace Ferrite.Services;

public class AccountService : IAccountService
{
    private readonly IDistributedCache _cache;
    private readonly IPersistentStore _store;
    public AccountService(IDistributedCache cache, IPersistentStore store)
    {
        _cache = cache;
        _store = store;
    }
    public async Task<bool> RegisterDevice(DeviceInfo deviceInfo)
    {
        return await _store.SaveDeviceInfoAsync(deviceInfo);
    }

    public async Task<bool> UnregisterDevice(long authKeyId, string token, ICollection<long> otherUserIds)
    {
        return await _store.DeleteDeviceInfoAsync(authKeyId, token, otherUserIds);
    }

    public async Task<bool> UpdateNotifySettings(long authKeyId, InputNotifyPeer peer, InputPeerNotifySettings settings)
    {
        return await _store.SaveNotifySettingsAsync(authKeyId, peer, settings);
    }

    public async Task<InputPeerNotifySettings> GetNotifySettings(long authKeyId, InputNotifyPeer peer)
    {
        var settings = await _store.GetNotifySettingsAsync(authKeyId, peer) ?? new InputPeerNotifySettings();

        return settings;
    }

    public async Task<bool> ResetNotifySettings(long authKeyId)
    {
        return await _store.DeleteNotifySettingsAsync(authKeyId);
    }

    public async Task<User?> UpdateProfile(long authKeyId, string? firstName, string? lastName, string? about)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        if (auth != null && await _store.GetUserAsync(auth.UserId) is { } user )
        {
            var userNew = new User()
            {
                AccessHash = user.AccessHash,
                FirstName = firstName ?? user.FirstName,
                LastName = lastName ?? user.LastName,
                About = about ?? user.About,
                Phone = user.Phone
            };
            await _store.SaveUserAsync(userNew);
            return userNew;
        }

        return null;
    }

    public async Task<bool> UpdateStatus(long authKeyId, bool status)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        if (auth != null)
        {
            return await _cache.PutUserStatusAsync(auth.UserId, status);
        }

        return false;
    }
}