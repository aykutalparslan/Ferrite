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
using DotNext.Threading;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Data.Account;
using Ferrite.Data.Auth;
using Ferrite.Data.Repositories;
using xxHash;

namespace Ferrite.Services;

public partial class AccountService : IAccountService
{
    private readonly IPersistentStore _store;
    private readonly ISearchEngine _search;
    private readonly IRandomGenerator _random;
    private readonly IUnitOfWork _unitOfWork;
    private static Regex UsernameRegex = new Regex("(^[a-zA-Z0-9_]{5,32}$)", RegexOptions.Compiled);
    private const int PhoneCodeTimeout = 60;//seconds
    public AccountService(IPersistentStore store, 
        ISearchEngine search, IRandomGenerator random, IUnitOfWork unitOfWork)
    {
        _store = store;
        _search = search;
        _random = random;
        _unitOfWork = unitOfWork;
    }
    public async Task<bool> RegisterDevice(DeviceInfoDTO deviceInfo)
    {
        _unitOfWork.DeviceInfoRepository.PutDeviceInfo(deviceInfo);
        return await _unitOfWork.SaveAsync();
    }

    public async Task<bool> UnregisterDevice(long authKeyId, string token, ICollection<long> otherUserIds)
    {
        _unitOfWork.DeviceInfoRepository.DeleteDeviceInfo(authKeyId, token, otherUserIds);
        return await _unitOfWork.SaveAsync();
    }

    public async Task<bool> UpdateNotifySettings(long authKeyId, InputNotifyPeerDTO peer, PeerNotifySettingsDTO settings)
    {
        _unitOfWork.NotifySettingsRepository.PutNotifySettings(authKeyId, peer, settings);
        return await _unitOfWork.SaveAsync();
    }

    public async Task<PeerNotifySettingsDTO> GetNotifySettings(long authKeyId, InputNotifyPeerDTO peer)
    {
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
        var settings = _unitOfWork.NotifySettingsRepository.GetNotifySettings(authKeyId, peer);
        if (settings.Count == 0)
        {
            return new PeerNotifySettingsDTO();
        }
        return settings.First(_ => _.DeviceType == deviceType);
    }

    public async Task<bool> ResetNotifySettings(long authKeyId)
    {
        _unitOfWork.NotifySettingsRepository.DeleteNotifySettings(authKeyId);
        return await _unitOfWork.SaveAsync();
    }

    public async Task<UserDTO?> UpdateProfile(long authKeyId, string? firstName, string? lastName, string? about)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth != null && _unitOfWork.UserRepository.GetUser(auth.UserId) is { } user )
        {
            var userNew = user with
            {
                FirstName = firstName ?? user.FirstName,
                LastName = lastName ?? user.LastName,
                About = about ?? user.About,
            };
            _unitOfWork.UserRepository.PutUser(userNew);
            await _unitOfWork.SaveAsync();
            await _search.IndexUser(new Data.Search.UserSearchModel(userNew.Id, userNew.Username, 
                userNew.FirstName, userNew.LastName, userNew.Phone));
            return userNew;
        }

        return null;
    }

    public async Task<bool> UpdateStatus(long authKeyId, bool status)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth != null)
        {
            return _unitOfWork.UserStatusRepository.PutUserStatus(auth.UserId, status);
        }

        return false;
    }

    public async Task<bool> ReportPeer(long authKeyId, InputPeerDTO peer, ReportReason reason)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return false;
        }
        _unitOfWork.ReportReasonRepository.PutPeerReportReason(auth.UserId, peer, reason);
        return await _unitOfWork.SaveAsync();
    }

    public async Task<bool> CheckUsername(string username)
    {
        if (!UsernameRegex.IsMatch(username))
        {
            return false;
        }

        var user = _unitOfWork.UserRepository.GetUserByUsername(username);
        if (user != null)
        {
            return false;
        }

        return true;
    }

    public async Task<UserDTO?> UpdateUsername(long authKeyId, string username)
    {
        if (!UsernameRegex.IsMatch(username))
        {
            return null;
        }
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return null;
        }
        var user = _unitOfWork.UserRepository.GetUserByUsername(username);
        if (user == null)
        {
            _unitOfWork.UserRepository.UpdateUsername(auth.UserId, username);
        }

        await _unitOfWork.SaveAsync();
        user = _unitOfWork.UserRepository.GetUser(auth.UserId);
        await _search.IndexUser(new Data.Search.UserSearchModel(user.Id, user.Username, 
                user.FirstName, user.LastName, user.Phone));
        return user;
    }

    public async Task<PrivacyRulesDTO?> SetPrivacy(long authKeyId, InputPrivacyKey key, ICollection<PrivacyRuleDTO> rules)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return null;
        }

        _unitOfWork.PrivacyRulesRepository.PutPrivacyRules(auth.UserId, key, rules);
        await _unitOfWork.SaveAsync();
        var savedRules = _unitOfWork.PrivacyRulesRepository.GetPrivacyRules(auth.UserId, key);
        List<PrivacyRuleDTO> privacyRules = new();
        List<UserDTO> users = new();
        List<ChatDTO> chats = new();
        foreach (var r in savedRules)
        {
            privacyRules.Add(r);
            if (r.PrivacyRuleType is PrivacyRuleType.AllowUsers or PrivacyRuleType.DisallowUsers)
            {
                foreach (var id in r.Peers)
                {
                    if (_unitOfWork.UserRepository.GetUser(id) is { } user)
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
                    if (_unitOfWork.ChatRepository.GetChat(id) is { } chat)
                    {
                        chats.Add(chat);
                    }  
                }
            }
        }
        
        PrivacyRulesDTO result = new PrivacyRulesDTO()
        {
            Rules = privacyRules,
            Users = users,
            Chats = chats
        };
        return result;
    }

    public async Task<PrivacyRulesDTO?> GetPrivacy(long authKeyId, InputPrivacyKey key)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return null;
        }
        
        var savedRules = _unitOfWork.PrivacyRulesRepository.GetPrivacyRules(auth.UserId, key);
        List<PrivacyRuleDTO> privacyRules = new();
        List<UserDTO> users = new();
        List<ChatDTO> chats = new();
        foreach (var r in savedRules)
        {
            privacyRules.Add(r);
            if (r.PrivacyRuleType is PrivacyRuleType.AllowUsers or PrivacyRuleType.DisallowUsers)
            {
                foreach (var id in r.Peers)
                {
                    if (_unitOfWork.UserRepository.GetUser(id) is { } user)
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
                    if (_unitOfWork.ChatRepository.GetChat(id) is { } chat)
                    {
                        chats.Add(chat);
                    }  
                }
            }
        }
        
        PrivacyRulesDTO result = new PrivacyRulesDTO()
        {
            Rules = privacyRules,
            Users = users,
            Chats = chats
        };
        return result;
    }

    public async Task<bool> DeleteAccount(long authKeyId)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var authorizations = await _unitOfWork.AuthorizationRepository.GetAuthorizationsAsync(auth.Phone);
        var user = _unitOfWork.UserRepository.GetUser(auth.UserId);

        foreach (var a in authorizations)
        {
            _unitOfWork.AuthorizationRepository.DeleteAuthorization(a.AuthKeyId);
            var device = _unitOfWork.DeviceInfoRepository.GetDeviceInfo(a.AuthKeyId);
             _unitOfWork.DeviceInfoRepository.DeleteDeviceInfo(a.AuthKeyId, device.Token, device.OtherUserIds);
            _unitOfWork.NotifySettingsRepository.DeleteNotifySettings(a.AuthKeyId);
            await _unitOfWork.SaveAsync();
        }

        _unitOfWork.PrivacyRulesRepository.DeletePrivacyRules(user.Id);
        _unitOfWork.UserRepository.DeleteUser(user);
        await _unitOfWork.SaveAsync();
        await _search.DeleteUser(user.Id);
        
        return true;
    }

    public async Task<bool> SetAccountTTL(long authKeyId, int accountDaysTTL)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return false;
        }

        _unitOfWork.UserRepository.UpdateAccountTTL(auth.UserId, accountDaysTTL);
        return await _unitOfWork.SaveAsync();
    }

    public async Task<int> GetAccountTTL(long authKeyId)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return 0;
        }

        return _unitOfWork.UserRepository.GetAccountTTL(auth.UserId);
    }

    public async Task<ServiceResult<SentCodeDTO>> SendChangePhoneCode(long authKeyId, string phoneNumber, CodeSettingsDTO settings)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (DateTime.Now - auth.LoggedInAt < new TimeSpan(1, 0, 0))
        {
            return new ServiceResult<SentCodeDTO>(null, false, 
                ErrorMessages.FreshChangePhoneForbidden);
        }
        var user = _unitOfWork.UserRepository.GetUser(phoneNumber);
        if (user != new UserDTO())
        {
            return new ServiceResult<SentCodeDTO>(null, false, 
                ErrorMessages.PhoneNumberOccupied);
        }
        
        var code = _random.GetNext(10000, 99999);
        Console.WriteLine("auth.sentCode=>" + code.ToString());
        var codeBytes = BitConverter.GetBytes(code);
        var hash = codeBytes.GetXxHash64(1071).ToString("x");
        _unitOfWork.PhoneCodeRepository.PutPhoneCode(phoneNumber, hash, code.ToString(),
            new TimeSpan(0, 0, PhoneCodeTimeout*2));
        await _unitOfWork.SaveAsync();
        var result = new SentCodeDTO()
        {
            CodeType = SentCodeType.Sms,
            CodeLength = 5,
            Timeout = PhoneCodeTimeout,
            PhoneCodeHash = hash
        };
        return new ServiceResult<SentCodeDTO>(result, true, ErrorMessages.None);
    }

    public async Task<ServiceResult<UserDTO>> ChangePhone(long authKeyId, string phoneNumber, string phoneCodeHash, string phoneCode)
    {
        var code = _unitOfWork.PhoneCodeRepository.GetPhoneCode(phoneNumber, phoneCodeHash);
        if (phoneCode != code)
        {
            return new ServiceResult<UserDTO>(null, false, ErrorMessages.None);
        }

        var user = _unitOfWork.UserRepository.GetUser(phoneNumber);
        if (user != null)
        {
            return new ServiceResult<UserDTO>(null, false, ErrorMessages.PhoneNumberOccupied);
        }
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var authorizations = await _unitOfWork.AuthorizationRepository.GetAuthorizationsAsync(auth.Phone);
        foreach (var authorization in authorizations)
        {
            _unitOfWork.AuthorizationRepository.PutAuthorization(authorization with { Phone = phoneNumber });
        }
        _unitOfWork.UserRepository.UpdateUserPhone(auth.UserId, phoneNumber);
        await _unitOfWork.SaveAsync();
        user = _unitOfWork.UserRepository.GetUser(phoneNumber);
        await _unitOfWork.SaveAsync();
        return new ServiceResult<UserDTO>(user, true, ErrorMessages.None);
    }

    public async Task<bool> UpdateDeviceLocked(long authKeyId, int period)
    {
         _unitOfWork.DeviceLockedRepository.PutDeviceLocked(authKeyId, TimeSpan.FromSeconds(period));
         return await _unitOfWork.SaveAsync();
    }

    public async Task<AuthorizationsDTO> GetAuthorizations(long authKeyId)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var authorizations = await _unitOfWork.AuthorizationRepository.GetAuthorizationsAsync(auth.Phone);
        List<AppInfoDTO> auths = new();
        foreach (var a in authorizations)
        {
            auths.Add(_unitOfWork.AppInfoRepository.GetAppInfo(a.AuthKeyId));
        }

        return new AuthorizationsDTO(_unitOfWork.UserRepository.GetAccountTTL(auth.UserId), auths);
    }

    public async Task<ServiceResult<bool>> ResetAuthorization(long authKeyId, long hash)
    {
        var sessAuthKeyId = _unitOfWork.AppInfoRepository.GetAuthKeyIdByAppHash(hash);
        if (sessAuthKeyId == null)
        {
            return new ServiceResult<bool>(false, false, ErrorMessages.HashInvalid);
        }
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (DateTime.Now - auth.LoggedInAt < new TimeSpan(1, 0, 0))
        {
            return new ServiceResult<bool>(false, false, ErrorMessages.FreshResetAuthorizationForbidden);
        }
        var info = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync((long)sessAuthKeyId);
        if(info == null)
        {
            return new ServiceResult<bool>(false, false, ErrorMessages.HashInvalid);
        }
        _unitOfWork.AuthorizationRepository.PutAuthorization(info with
        {
            Phone = "",
            UserId = 0,
            LoggedIn = false
        });
        await _unitOfWork.SaveAsync();
        return new ServiceResult<bool>(true, true, ErrorMessages.None);
    }

    public async Task<bool> SetContactSignUpNotification(long authKeyId, bool silent)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        _unitOfWork.SignUpNotificationRepository.PutSignUpNotification(auth.UserId, silent);
        return await _unitOfWork.SaveAsync();
    }

    public async Task<bool> GetContactSignUpNotification(long authKeyId)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        return _unitOfWork.SignUpNotificationRepository.GetSignUpNotification(auth.UserId);
    }

    public async Task<ServiceResult<bool>> ChangeAuthorizationSettings(long authKeyId, long hash, 
        bool encryptedRequestsDisabled, bool callRequestsDisabled)
    {
        var appAuthKeyId = _unitOfWork.AppInfoRepository.GetAuthKeyIdByAppHash(hash);
        if(appAuthKeyId == null)
        {
            return new ServiceResult<bool>(false, false, ErrorMessages.HashInvalid);
        }
        var info = _unitOfWork.AppInfoRepository.GetAppInfo((long)appAuthKeyId);
        var success = _unitOfWork.AppInfoRepository.PutAppInfo(info with
        {
            EncryptedRequestsDisabled = encryptedRequestsDisabled,
            CallRequestsDisabled = callRequestsDisabled
        });
        await _unitOfWork.SaveAsync();
        return new ServiceResult<bool>(success, success, ErrorMessages.None);
    }
}