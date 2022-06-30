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
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Data.Account;
using Ferrite.Data.Auth;
using xxHash;

namespace Ferrite.Services;

public partial class AccountService : IAccountService
{
    private readonly IDistributedCache _cache;
    private readonly IPersistentStore _store;
    private readonly ISearchEngine _search;
    private readonly IRandomGenerator _random;
    [RegexGenerator("(^[a-zA-Z0-9_]{5,32}$)", RegexOptions.Compiled)]
    private static partial Regex UsernameRegex();
    private const int PhoneCodeTimeout = 60;//seconds
    public AccountService(IDistributedCache cache, IPersistentStore store, 
        ISearchEngine search, IRandomGenerator random)
    {
        _cache = cache;
        _store = store;
        _search = search;
        _random = random;
    }
    public async Task<bool> RegisterDevice(DeviceInfo deviceInfo)
    {
        return await _store.SaveDeviceInfoAsync(deviceInfo);
    }

    public async Task<bool> UnregisterDevice(long authKeyId, string token, ICollection<long> otherUserIds)
    {
        return await _store.DeleteDeviceInfoAsync(authKeyId, token, otherUserIds);
    }

    public async Task<bool> UpdateNotifySettings(long authKeyId, InputNotifyPeer peer, PeerNotifySettings settings)
    {
        return await _store.SaveNotifySettingsAsync(authKeyId, peer, settings);
    }

    public async Task<PeerNotifySettings> GetNotifySettings(long authKeyId, InputNotifyPeer peer)
    {
        var info = await _store.GetAppInfoAsync(authKeyId);
        DeviceType deviceType = DeviceType.Other;
        if (info.LangPack.ToLower().Contains("android"))
        {
            deviceType = DeviceType.Android;
        }
        else if (info.LangPack.ToLower().Contains("ios"))
        {
            deviceType = DeviceType.iOS;
        }
        var settings = await _store.GetNotifySettingsAsync(authKeyId, peer);
        if (settings.Count == 0)
        {
            return new PeerNotifySettings();
        }
        return settings.First(_ => _.DeviceType == deviceType);
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
            var userNew = user with
            {
                FirstName = firstName ?? user.FirstName,
                LastName = lastName ?? user.LastName,
                About = about ?? user.About,
            };
            await _store.SaveUserAsync(userNew);
            await _search.IndexUser(new Data.Search.User(userNew.Id, userNew.Username, 
                userNew.FirstName, userNew.LastName, userNew.Phone));
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
        if (!UsernameRegex().IsMatch(username))
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
        if (!UsernameRegex().IsMatch(username))
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
        user = await _store.GetUserAsync(auth.UserId);
        await _search.IndexUser(new Data.Search.User(user.Id, user.Username, 
                user.FirstName, user.LastName, user.Phone));
        return user;
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

        await _search.DeleteUser(user.Id);
        
        return true;
    }

    public async Task<bool> SetAccountTTL(long authKeyId, int accountDaysTTL)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return false;
        }

        return await _store.UpdateAccountTTLAsync(auth.UserId, accountDaysTTL);
    }

    public async Task<int> GetAccountTTL(long authKeyId)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return 0;
        }

        return await _store.GetAccountTTLAsync(auth.UserId);
    }

    public async Task<ServiceResult<SentCode>> SendChangePhoneCode(long authKeyId, string phoneNumber, CodeSettings settings)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        if (DateTime.Now - auth.LoggedInAt < new TimeSpan(1, 0, 0))
        {
            return new ServiceResult<SentCode>(null, false, 
                ErrorMessages.FreshChangePhoneForbidden);
        }
        var user = await _store.GetUserAsync(phoneNumber);
        if (user != new User())
        {
            return new ServiceResult<SentCode>(null, false, 
                ErrorMessages.PhoneNumberOccupied);
        }
        
        var code = _random.GetNext(10000, 99999);
        Console.WriteLine("auth.sentCode=>" + code.ToString());
        var codeBytes = BitConverter.GetBytes(code);
        var hash = codeBytes.GetXxHash64(1071).ToString("x");
        await _cache.PutPhoneCodeAsync(phoneNumber, hash, code.ToString(),
            new TimeSpan(0, 0, PhoneCodeTimeout*2));
        
        var result = new SentCode()
        {
            CodeType = SentCodeType.Sms,
            CodeLength = 5,
            Timeout = PhoneCodeTimeout,
            PhoneCodeHash = hash
        };
        return new ServiceResult<SentCode>(result, true, ErrorMessages.None);
    }

    public async Task<ServiceResult<User>> ChangePhone(long authKeyId, string phoneNumber, string phoneCodeHash, string phoneCode)
    {
        var code = await _cache.GetPhoneCodeAsync(phoneNumber, phoneCodeHash);
        if (phoneCode != code)
        {
            return new ServiceResult<User>(null, false, ErrorMessages.None);
        }

        var user = await _store.GetUserAsync(phoneNumber);
        if (user != null)
        {
            return new ServiceResult<User>(null, false, ErrorMessages.PhoneNumberOccupied);
        }
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        var authorizations = await _store.GetAuthorizationsAsync(auth.Phone);
        foreach (var authorization in authorizations)
        {
            await _store.SaveAuthorizationAsync(authorization with { Phone = phoneNumber });
        }
        await _store.UpdateUserPhoneAsync(auth.UserId, phoneNumber);
        user = await _store.GetUserAsync(phoneNumber);
        return new ServiceResult<User>(user, true, ErrorMessages.None);
    }

    public async Task<bool> UpdateDeviceLocked(long authKeyId, int period)
    {
        return await _cache.PutDeviceLockedAsync(authKeyId, period);
    }

    public async Task<Authorizations> GetAuthorizations(long authKeyId)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        var authorizations = await _store.GetAuthorizationsAsync(auth.Phone);
        List<AppInfo> auths = new();
        foreach (var a in authorizations)
        {
            auths.Add(await _store.GetAppInfoAsync(a.AuthKeyId));
        }

        return new Authorizations(await _store.GetAccountTTLAsync(auth.UserId), auths);
    }

    public async Task<ServiceResult<bool>> ResetAuthorization(long authKeyId, long hash)
    {
        var sessAuthKeyId = await _store.GetAuthKeyIdByAppHashAsync(hash);
        if (sessAuthKeyId == null)
        {
            return new ServiceResult<bool>(false, false, ErrorMessages.HashInvalid);
        }
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        if (DateTime.Now - auth.LoggedInAt < new TimeSpan(1, 0, 0))
        {
            return new ServiceResult<bool>(false, false, ErrorMessages.FreshResetAuthorizationForbidden);
        }
        var info = await _store.GetAuthorizationAsync((long)sessAuthKeyId);
        if(info == null)
        {
            return new ServiceResult<bool>(false, false, ErrorMessages.HashInvalid);
        }
        await _store.SaveAuthorizationAsync(info with
        {
            Phone = "",
            UserId = 0,
            LoggedIn = false
        });
        return new ServiceResult<bool>(true, true, ErrorMessages.None);
    }

    public async Task<bool> SetContactSignUpNotification(long authKeyId, bool silent)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        return await _store.SaveSignUoNotificationAsync(auth.UserId, silent);
    }

    public async Task<bool> GetContactSignUpNotification(long authKeyId)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        return await _store.GetSignUoNotificationAsync(auth.UserId);
    }
}