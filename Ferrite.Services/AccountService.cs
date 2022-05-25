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

using System.Text.RegularExpressions;
using Ferrite.Data;
using Ferrite.Data.Account;

namespace Ferrite.Services;

public class AccountService : IAccountService
{
    private readonly IDistributedCache _cache;
    private readonly IPersistentStore _store;
    private readonly Regex UsernameRegex = new Regex("(^[a-zA-Z0-9_]{5,32}$)", RegexOptions.Compiled);
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
        var result = await _store.GetNotifySettingsAsync(authKeyId, peer) ?? 
                     new InputPeerNotifySettings()
                     {
                         Sound = ""
                     };
        return result;
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

    public async Task<bool> ReportPeer(long authKeyId, InputPeer peer, ReportReason reason)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return false;
        }
        return await _store.SavePeerReportReasonAsync(auth.UserId, peer, reason);
    }

    public async Task<bool> CheckUsername(string username)
    {
        if (!UsernameRegex.IsMatch(username))
        {
            return false;
        }

        var user = await _store.GetUserByUsernameAsync(username);
        if (user != null)
        {
            return false;
        }

        return true;
    }

    public async Task<User?> UpdateUsername(long authKeyId, string username)
    {
        if (!UsernameRegex.IsMatch(username))
        {
            return null;
        }
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return null;
        }
        var user = await _store.GetUserByUsernameAsync(username);
        if (user == null)
        {
            await _store.UpdateUsernameAsync(auth.UserId, username);
        }
        return await _store.GetUserAsync(auth.UserId);
    }

    public async Task<PrivacyRules?> SetPrivacy(long authKeyId, InputPrivacyKey key, ICollection<PrivacyRule> rules)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return null;
        }

        await _store.SavePrivacyRulesAsync(auth.UserId, key, rules);
        var savedRules = await _store.GetPrivacyRulesAsync(auth.UserId, key);
        List<PrivacyRule> privacyRules = new();
        List<User> users = new();
        List<Chat> chats = new();
        foreach (var r in savedRules)
        {
            privacyRules.Add(r);
            if (r.PrivacyRuleType is PrivacyRuleType.AllowUsers or PrivacyRuleType.DisallowUsers)
            {
                foreach (var id in r.Peers)
                {
                    if (await _store.GetUserAsync(id) is { } user)
                    {
                        users.Add(user);
                    }  
                }
            } 
            else if (r.PrivacyRuleType is PrivacyRuleType.AllowChatParticipants
                       or PrivacyRuleType.DisallowChatParticipants)
            {
                foreach (var id in r.Peers)
                {
                    if (await _store.GetChatAsync(id) is { } chat)
                    {
                        chats.Add(chat);
                    }  
                }
            }
        }
        
        PrivacyRules result = new PrivacyRules()
        {
            Rules = privacyRules,
            Users = users,
            Chats = chats
        };
        return result;
    }

    public async Task<PrivacyRules?> GetPrivacy(long authKeyId, InputPrivacyKey key)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return null;
        }
        
        var savedRules = await _store.GetPrivacyRulesAsync(auth.UserId, key);
        List<PrivacyRule> privacyRules = new();
        List<User> users = new();
        List<Chat> chats = new();
        foreach (var r in savedRules)
        {
            privacyRules.Add(r);
            if (r.PrivacyRuleType is PrivacyRuleType.AllowUsers or PrivacyRuleType.DisallowUsers)
            {
                foreach (var id in r.Peers)
                {
                    if (await _store.GetUserAsync(id) is { } user)
                    {
                        users.Add(user);
                    }  
                }
            }
            else if (r.PrivacyRuleType is PrivacyRuleType.AllowChatParticipants
                     or PrivacyRuleType.DisallowChatParticipants)
            {
                foreach (var id in r.Peers)
                {
                    if (await _store.GetChatAsync(id) is { } chat)
                    {
                        chats.Add(chat);
                    }  
                }
            }
        }
        
        PrivacyRules result = new PrivacyRules()
        {
            Rules = privacyRules,
            Users = users,
            Chats = chats
        };
        return result;
    }

    public async Task<bool> DeleteAccount(long authKeyId)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        var authorizations = await _store.GetAuthorizationsAsync(auth.Phone);
        var user = await _store.GetUserAsync(auth.UserId);

        foreach (var a in authorizations)
        {
            await _store.DeleteAuthorizationAsync(a.AuthKeyId);
            var device = await _store.GetDeviceInfoAsync(a.AuthKeyId);
            await _store.DeleteDeviceInfoAsync(a.AuthKeyId, device.Token, device.OtherUserIds);
            await _store.DeleteNotifySettingsAsync(a.AuthKeyId);
        }

        await _store.DeletePrivacyRulesAsync(user.Id);

        await _store.DeleteUserAsync(user);

        return true;
    }
}