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

using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using DotNext;
using DotNext.Buffers;
using DotNext.Threading;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Data.Account;
using Ferrite.Data.Auth;
using Ferrite.Data.Repositories;
using Ferrite.TL.slim;
using Ferrite.TL.slim.layer150;
using Ferrite.TL.slim.layer150.account;
using xxHash;

namespace Ferrite.Services;

public partial class AccountService : IAccountService
{
    private readonly ISearchEngine _search;
    private readonly IRandomGenerator _random;
    private readonly IUnitOfWork _unitOfWork;
    private static Regex UsernameRegex = new Regex("(^[a-zA-Z0-9_]{5,32}$)", RegexOptions.Compiled);
    private const int PhoneCodeTimeout = 60;//seconds
    public AccountService(ISearchEngine search, IRandomGenerator random, IUnitOfWork unitOfWork)
    {
        _search = search;
        _random = random;
        _unitOfWork = unitOfWork;
    }
    public async ValueTask<TLBytes> RegisterDevice(long authKeyId, TLBytes q)
    {
        var deviceInfo = GetDeviceInfo(authKeyId, q);
        _unitOfWork.DeviceInfoRepository.PutDeviceInfo(deviceInfo);
        var result = await _unitOfWork.SaveAsync();
        return result ? BoolTrue.Builder().Build().TLBytes!.Value : 
            BoolFalse.Builder().Build().TLBytes!.Value;
    }

    public async ValueTask<TLBytes> RegisterDeviceL57(long authKeyId, TLBytes q)
    {
        var deviceInfo = GetDeviceInfoL57(authKeyId, q);
        _unitOfWork.DeviceInfoRepository.PutDeviceInfo(deviceInfo);
        var result = await _unitOfWork.SaveAsync();
        return result ? BoolTrue.Builder().Build().TLBytes!.Value : 
            BoolFalse.Builder().Build().TLBytes!.Value;
    }
    
    private static DeviceInfoDTO GetDeviceInfoL57(long authKeyId, TLBytes q)
    {
        var registerDevice = new RegisterDeviceL57(q.AsSpan());
        var token = Encoding.UTF8.GetString(registerDevice.Token);
        return new DeviceInfoDTO()
        {
            AuthKeyId = authKeyId,
            Token = token,
            Secret = Array.Empty<byte>(),
            TokenType = registerDevice.TokenType,
            OtherUserIds = Array.Empty<long>(),
        };
    }

    private static DeviceInfoDTO GetDeviceInfo(long authKeyId, TLBytes q)
    {
        var registerDevice = new RegisterDevice(q.AsSpan());
        var token = Encoding.UTF8.GetString(registerDevice.Token);
        var uids = new long[registerDevice.OtherUids.Count];
        for (int i = 0; i < registerDevice.OtherUids.Count; i++)
        {
            uids[i] = registerDevice.OtherUids[i];
        }
        return new DeviceInfoDTO()
        {
            AuthKeyId = authKeyId,
            Token = token,
            Secret = registerDevice.Secret.ToArray(),
            AppSandbox = registerDevice.AppSandbox,
            NoMuted = registerDevice.NoMuted,
            TokenType = registerDevice.TokenType,
            OtherUserIds = uids,
        };
    }

    public async ValueTask<TLBytes> UnregisterDevice(long authKeyId, TLBytes q)
    {
        var unregisterReq = GetUnregisterDeviceParameters(q);
        _unitOfWork.DeviceInfoRepository.DeleteDeviceInfo(authKeyId, unregisterReq.Token, unregisterReq.OtherUserIds);
        var result = await _unitOfWork.SaveAsync();
        return result ? BoolTrue.Builder().Build().TLBytes!.Value : 
            BoolFalse.Builder().Build().TLBytes!.Value;
    }

    private readonly record struct UnregisterDeviceParameters(int TokenType, string Token, ICollection<long> OtherUserIds);

    private static UnregisterDeviceParameters GetUnregisterDeviceParameters(TLBytes q)
    {
        var unregister = new UnregisterDevice(q.AsSpan());
        var token = Encoding.UTF8.GetString(unregister.Token);
        var uids = new long[unregister.OtherUids.Count];
        for (int i = 0; i < unregister.OtherUids.Count; i++)
        {
            uids[i] = unregister.OtherUids[i];
        }

        return new UnregisterDeviceParameters(unregister.TokenType, token, uids);
    }

    public async ValueTask<TLBytes> UpdateNotifySettings(long authKeyId, TLBytes q)
    {
        var info = _unitOfWork.AppInfoRepository.GetAppInfo(authKeyId);
        DeviceType deviceType = DeviceType.Other;
        if (info != null && info.LangPack.ToLower().Contains("android"))
        {
            deviceType = DeviceType.Android;
        }
        else if (info != null && info.LangPack.ToLower().Contains("ios"))
        {
            deviceType = DeviceType.iOS;
        }
        using var notifySettingsParameters = GetUpdateNotifySettingsParameters(deviceType, q);
        _unitOfWork.NotifySettingsRepository.PutNotifySettings(authKeyId, 
            notifySettingsParameters.NotifyPeerType, 
            notifySettingsParameters.PeerType, 
            notifySettingsParameters.PeerId,
            (int)deviceType, notifySettingsParameters.PeerNotifySettings);
        var result = await _unitOfWork.SaveAsync();
        return result ? BoolTrue.Builder().Build().TLBytes!.Value : 
            BoolFalse.Builder().Build().TLBytes!.Value;
    }

    private readonly record struct UpdateNotifySettingsParameters(int NotifyPeerType, int PeerType, long PeerId,
        TLBytes PeerNotifySettings) : IDisposable
    {
        public void Dispose()
        {
            PeerNotifySettings.Dispose();
        }
    }

    private static UpdateNotifySettingsParameters GetUpdateNotifySettingsParameters(DeviceType deviceType, TLBytes q)
    {
        var settings = new UpdateNotifySettings(q.AsSpan());
        (var peerId, InputNotifyPeerType notifyPeerType, InputPeerType peerType) = GetNotifyPeerInfo(settings.Peer);

        var inputSettings = new InputPeerNotifySettings(settings.NotifySettings);
        var settingsBuilder = PeerNotifySettings
            .Builder()
            .Silent(inputSettings.Silent)
            .MuteUntil(inputSettings.MuteUntil)
            .ShowPreviews(inputSettings.ShowPreviews);
        if (inputSettings.Flags[3])
        {
            switch (deviceType)
            {
                case DeviceType.Android:
                    settingsBuilder = settingsBuilder.AndroidSound(inputSettings.Sound);
                    break;
                case DeviceType.iOS:
                    settingsBuilder = settingsBuilder.IosSound(inputSettings.Sound);
                    break;
                default:
                    settingsBuilder = settingsBuilder.OtherSound(inputSettings.Sound);
                    break;
            }
        }

        return new UpdateNotifySettingsParameters((int)notifyPeerType, 
            (int)peerType, peerId, settingsBuilder.Build().TLBytes!.Value);
    }

    private static (long peerId, InputNotifyPeerType notifyPeerType, InputPeerType peerType) GetNotifyPeerInfo(
        Span<byte> peer)
    {
        int peerConstructor = MemoryMarshal.Read<int>(peer);
        long peerId = 0;
        InputNotifyPeerType notifyPeerType = InputNotifyPeerType.Users;
        ;
        InputPeerType peerType = InputPeerType.Empty;
        switch (peerConstructor)
        {
            case Constructors.layer150_InputNotifyUsers:
                notifyPeerType = InputNotifyPeerType.Users;
                break;
            case Constructors.layer150_InputNotifyChats:
                notifyPeerType = InputNotifyPeerType.Chats;
                break;
            case Constructors.layer150_InputNotifyBroadcasts:
                notifyPeerType = InputNotifyPeerType.Broadcasts;
                break;
            case Constructors.layer150_InputNotifyPeer:
                notifyPeerType = InputNotifyPeerType.Peer;
                var notifyPeer = new InputNotifyPeer(peer);
                var (inputPeerType, id, accessHash) = GetPeerTypeAndId(notifyPeer.Peer);
                peerType = inputPeerType;
                peerId = id;
                break;
        }

        return (peerId, notifyPeerType, peerType);
    }

    private static (InputPeerType, long, long) GetPeerTypeAndId(Span<byte> bytes)
    {
        InputPeerType inputPeerType = InputPeerType.Empty;
        int peerConstructor = MemoryMarshal.Read<int>(bytes);
        long peerId = 0;
        long accessHash = 0;
        switch (peerConstructor)
        {
            case Constructors.layer150_InputPeerSelf:
                inputPeerType = InputPeerType.Self;
                break;
            case Constructors.layer150_InputPeerChat:
                inputPeerType = InputPeerType.Chat;
                var chat = new InputPeerChat(bytes);
                peerId = chat.ChatId;
                break;
            case Constructors.layer150_InputPeerUser:
                inputPeerType = InputPeerType.User;
                var user = new InputPeerUser(bytes);
                peerId = user.UserId;
                accessHash = user.AccessHash;
                break;
            case Constructors.layer150_InputPeerChannel:
                inputPeerType = InputPeerType.Channel;
                var channel = new InputPeerChannel(bytes);
                peerId = channel.ChannelId;
                accessHash = channel.AccessHash;
                break;
            case Constructors.layer150_InputPeerUserFromMessage:
                inputPeerType = InputPeerType.User;
                var userFromMessage = new InputPeerUserFromMessage(bytes);
                var peer = new InputPeerUser(userFromMessage.Peer);
                peerId = peer.UserId;
                accessHash = peer.AccessHash;
                break;
            case Constructors.layer150_InputPeerChannelFromMessage:
                inputPeerType = InputPeerType.User;
                var channelFromMessage = new InputPeerUserFromMessage(bytes);
                var peerChannel = new InputPeerChannel(channelFromMessage.Peer);
                peerId = peerChannel.ChannelId;
                accessHash = peerChannel.AccessHash;
                break;
            case Constructors.layer150_InputPeerEmpty:
                inputPeerType = InputPeerType.Empty;
                break;
        }

        return (inputPeerType, peerId, accessHash);
    }

    public async ValueTask<TLBytes> GetNotifySettings(long authKeyId, TLBytes q)
    {
        (var peerId, InputNotifyPeerType notifyPeerType, InputPeerType peerType) = 
            GetNotifyPeerInfo(new GetNotifySettings(q.AsSpan()).Peer);
        var info = _unitOfWork.AppInfoRepository.GetAppInfo(authKeyId);
        DeviceType deviceType = DeviceType.Other;
        if (info != null && info.LangPack.ToLower().Contains("android"))
        {
            deviceType = DeviceType.Android;
        }
        else if (info != null && info.LangPack.ToLower().Contains("ios"))
        {
            deviceType = DeviceType.iOS;
        }

        var settings = _unitOfWork
            .NotifySettingsRepository.GetNotifySettings(authKeyId, 
                (int)notifyPeerType, (int)peerType, peerId, (int)deviceType);
        if (settings.Count == 0)
        {
            return PeerNotifySettings.Builder().Build().TLBytes!.Value;
        }
        return settings.First();
    }

    public async Task<bool> ResetNotifySettings(long authKeyId)
    {
        _unitOfWork.NotifySettingsRepository.DeleteNotifySettings(authKeyId);
        return await _unitOfWork.SaveAsync();
    }

    public async Task<UserDTO?> UpdateProfile(long authKeyId, string? firstName, string? lastName, string? about)
    {
        /*var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
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
        }*/

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
        /*if (!UsernameRegex.IsMatch(username))
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
        return user;*/
        return null;
    }

    public async Task<PrivacyRulesDTO?> SetPrivacy(long authKeyId, InputPrivacyKey key, ICollection<PrivacyRuleDTO> rules)
    {
        /*var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
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
        return result;*/
        return null;
    }

    public async Task<PrivacyRulesDTO?> GetPrivacy(long authKeyId, InputPrivacyKey key)
    {
        /*var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
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
        return result;*/
        return null;
    }

    public async Task<bool> DeleteAccount(long authKeyId)
    {
        /*var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
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
        
        return true;*/
        return false;
    }

    public async Task<bool> SetAccountTTL(long authKeyId, int accountDaysTTL)
    {
        /*var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return false;
        }

        _unitOfWork.UserRepository.UpdateAccountTTL(auth.UserId, accountDaysTTL);
        return await _unitOfWork.SaveAsync();*/
        return false;
    }

    public async Task<int> GetAccountTTL(long authKeyId)
    {
        /*var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return 0;
        }

        return _unitOfWork.UserRepository.GetAccountTTL(auth.UserId);*/
        return 0;
    }

    public async Task<ServiceResult<SentCodeDTO>> SendChangePhoneCode(long authKeyId, string phoneNumber, CodeSettingsDTO settings)
    {
        /*var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
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
        return new ServiceResult<SentCodeDTO>(result, true, ErrorMessages.None);*/
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<UserDTO>> ChangePhone(long authKeyId, string phoneNumber, string phoneCodeHash, string phoneCode)
    {
        /*var code = _unitOfWork.PhoneCodeRepository.GetPhoneCode(phoneNumber, phoneCodeHash);
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
        return new ServiceResult<UserDTO>(user, true, ErrorMessages.None);*/
        throw new NotImplementedException();
    }

    public async Task<bool> UpdateDeviceLocked(long authKeyId, int period)
    {
         _unitOfWork.DeviceLockedRepository.PutDeviceLocked(authKeyId, TimeSpan.FromSeconds(period));
         return await _unitOfWork.SaveAsync();
    }

    public async Task<AuthorizationsDTO> GetAuthorizations(long authKeyId)
    {
        /*var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var authorizations = await _unitOfWork.AuthorizationRepository.GetAuthorizationsAsync(auth.Phone);
        List<AppInfoDTO> auths = new();
        foreach (var a in authorizations)
        {
            auths.Add(_unitOfWork.AppInfoRepository.GetAppInfo(a.AuthKeyId));
        }

        return new AuthorizationsDTO(_unitOfWork.UserRepository.GetAccountTTL(auth.UserId), auths);*/
        throw new NotImplementedException();
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