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

using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Data.Account;
using Ferrite.Data.Repositories;
using Ferrite.Services.Gateway;
using Ferrite.TL.slim;
using Ferrite.TL.slim.dto;
using Ferrite.TL.slim.layer150;
using Ferrite.TL.slim.layer150.account;
using Ferrite.TL.slim.layer150.auth;
using xxHash;
using TLAuthorization = Ferrite.TL.slim.layer150.auth.TLAuthorization;

namespace Ferrite.Services;

public class AccountService : IAccountService
{
    private readonly ISearchEngine _search;
    private readonly IRandomGenerator _random;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVerificationGateway _verificationGateway;
    private static Regex UsernameRegex = new Regex("(^[a-zA-Z0-9_]{5,32}$)", RegexOptions.Compiled);
    private const int PhoneCodeTimeout = 60;//seconds
    public AccountService(ISearchEngine search, IRandomGenerator random, IUnitOfWork unitOfWork, IVerificationGateway verificationGateway)
    {
        _search = search;
        _random = random;
        _unitOfWork = unitOfWork;
        _verificationGateway = verificationGateway;
    }
    public async ValueTask<TLBool> RegisterDevice(long authKeyId, TLBytes q)
    {
        var deviceInfo = GetDeviceInfo(authKeyId, q);
        _unitOfWork.DeviceInfoRepository.PutDeviceInfo(deviceInfo);
        var result = await _unitOfWork.SaveAsync();
        return result ? new BoolTrue() : new BoolFalse();
    }

    public async ValueTask<TLBool> RegisterDeviceL57(long authKeyId, TLBytes q)
    {
        var deviceInfo = GetDeviceInfoL57(authKeyId, q);
        _unitOfWork.DeviceInfoRepository.PutDeviceInfo(deviceInfo);
        var result = await _unitOfWork.SaveAsync();
        return result ? new BoolTrue() : new BoolFalse();
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

    public async ValueTask<TLBool> UnregisterDevice(long authKeyId, TLBytes q)
    {
        var unregisterReq = GetUnregisterDeviceParameters(q);
        _unitOfWork.DeviceInfoRepository.DeleteDeviceInfo(authKeyId, unregisterReq.Token, unregisterReq.OtherUserIds);
        var result = await _unitOfWork.SaveAsync();
        return result ? new BoolTrue() : new BoolFalse();
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

    public async ValueTask<TLBool> UpdateNotifySettings(long authKeyId, TLBytes q)
    {
        var info = _unitOfWork.AppInfoRepository.GetAppInfo(authKeyId);
        DeviceType deviceType = GetDeviceType(info);
        using var notifySettingsParameters = GetUpdateNotifySettingsParameters(deviceType, q);
        _unitOfWork.NotifySettingsRepository.PutNotifySettings(authKeyId, 
            notifySettingsParameters.NotifyPeerType, 
            notifySettingsParameters.PeerType, 
            notifySettingsParameters.PeerId,
            (int)deviceType, notifySettingsParameters.PeerNotifySettings);
        var result = await _unitOfWork.SaveAsync();
        return result ? new BoolTrue() : new BoolFalse();
    }

    private DeviceType GetDeviceType(TLBytes? info)
    {
        DeviceType deviceType = DeviceType.Other;
        var langPack = info != null 
            ? Encoding.UTF8.GetString(((AppInfo)info).LangPack).ToLower()
            : "";
        if (langPack.Contains("android"))
        {
            deviceType = DeviceType.Android;
        }
        else if (langPack.Contains("ios"))
        {
            deviceType = DeviceType.iOS;
        }

        return deviceType;
    }

    private readonly record struct UpdateNotifySettingsParameters(int NotifyPeerType, int PeerType, long PeerId,
        TLPeerNotifySettings PeerNotifySettings) : IDisposable
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
            (int)peerType, peerId, settingsBuilder.Build());
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

    public async ValueTask<TLPeerNotifySettings> GetNotifySettings(long authKeyId, TLBytes q)
    {
        (var peerId, InputNotifyPeerType notifyPeerType, InputPeerType peerType) = 
            GetNotifyPeerInfo(new GetNotifySettings(q.AsSpan()).Peer);
        var info = _unitOfWork.AppInfoRepository.GetAppInfo(authKeyId);
        DeviceType deviceType = GetDeviceType(info);

        var settings = _unitOfWork
            .NotifySettingsRepository.GetNotifySettings(authKeyId, 
                (int)notifyPeerType, (int)peerType, peerId, (int)deviceType);
        if (settings.Count == 0)
        {
            return PeerNotifySettings.Builder().Build();
        }
        return (TLPeerNotifySettings)settings.First();
    }

    public async ValueTask<TLBool> ResetNotifySettings(long authKeyId)
    {
        _unitOfWork.NotifySettingsRepository.DeleteNotifySettings(authKeyId);
        var result = await _unitOfWork.SaveAsync();
        return result ? new BoolTrue() : new BoolFalse();
    }

    public async ValueTask<TLUser> UpdateProfile(long authKeyId, string? firstName, string? lastName, string? about)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth != null)
        {
            using var u = _unitOfWork.UserRepository.GetUser(auth.Value.AsAuthInfo().UserId);
            if (u == null) return (TLUser)RpcErrorGenerator.GenerateError(400, "USER_ID_INVALID"u8);
            var user = ModifyUser(u.Value, firstName, lastName);
            _unitOfWork.UserRepository.PutUser(user);
            var userInfo = GetUserInfo(user);
            if (about != null) _unitOfWork.UserRepository.PutAbout(userInfo.UserId, about);
            await _unitOfWork.SaveAsync();
            await _search.IndexUser(new Data.Search.UserSearchModel(userInfo.UserId, userInfo.Username, 
                userInfo.FirstName, userInfo.LastName, userInfo.Phone));
            return user;
        }

        return (TLUser)RpcErrorGenerator.GenerateError(400,"FIRSTNAME_INVALID"u8);
    }

    private readonly record struct UserInfo(long UserId, string? Username, 
        string? FirstName, string? LastName, string Phone);

    private static UserInfo GetUserInfo(TLBytes u)
    {
        var user = new User(u.AsSpan());
        string? username = user.Username.Length > 0 ? Encoding.UTF8.GetString(user.Username) : null;
        string? firstname = user.FirstName.Length > 0 ? Encoding.UTF8.GetString(user.FirstName) : null;
        string? lastname = user.LastName.Length > 0 ? Encoding.UTF8.GetString(user.LastName) : null;
        string phone = Encoding.UTF8.GetString(user.Phone);
        return new UserInfo(user.Id, username, firstname, lastname, phone);
    }

    private static TLUser ModifyUser(TLBytes u, string? firstName, string? lastName)
    {
        var user = new User(u.AsSpan()).Clone();
        if (firstName != null) user = user.FirstName(Encoding.UTF8.GetBytes(firstName));
        if (lastName != null) user = user.LastName(Encoding.UTF8.GetBytes(lastName));
        return user.Build();
    }

    public async ValueTask<TLBool> UpdateStatus(long authKeyId, bool status)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth != null)
        {
            var result = _unitOfWork.UserStatusRepository.PutUserStatus(auth.Value.AsAuthInfo().UserId, status);
            return result ? new BoolTrue() : new BoolFalse();
        }

        return new BoolFalse();
    }

    public async ValueTask<TLBool> ReportPeer(long authKeyId, TLBytes q)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return (TLBool)RpcErrorGenerator.GenerateError(400, "AUTH_KEY_INVALID"u8);
        }

        using var reason = GetReportPeerParameters( q);
        _unitOfWork.ReportReasonRepository.PutPeerReportReason(auth.Value.AsAuthInfo().UserId, reason.PeerType, reason.PeerId, reason.Reason);
        var result = await _unitOfWork.SaveAsync();
        return result ? new BoolTrue() : new BoolFalse();
    }

    private readonly record struct ReportPeerParameters(int PeerType, long PeerId, TLBytes Reason): IDisposable
    {
        public void Dispose()
        {
            Reason.Dispose();
        }
    }

    private static ReportPeerParameters GetReportPeerParameters(TLBytes q)
    {
        var reportPeer = new ReportPeer(q.AsSpan());
        var (type, id, hash) = GetPeerTypeAndId(reportPeer.Peer);
        var reportReason = ReportReasonWithMessage.Builder()
            .ReportReason(reportPeer.Reason)
            .Message(reportPeer.Message)
            .Build();
        return new ReportPeerParameters((int)type, id, reportReason.TLBytes!.Value);
    }

    public ValueTask<TLBool> CheckUsername(string username)
    {
        if (!UsernameRegex.IsMatch(username))
        {
            return ValueTask.FromResult((TLBool)new BoolFalse());
        }

        var user = _unitOfWork.UserRepository.GetUserByUsername(username);
        if (user != null)
        {
            return ValueTask.FromResult((TLBool)new BoolFalse());
        }
        
        return ValueTask.FromResult((TLBool)new BoolTrue());
    }

    public async ValueTask<TLUser> UpdateUsername(long authKeyId, string username)
    {
        if (!UsernameRegex.IsMatch(username))
        {
            return (TLUser)RpcErrorGenerator.GenerateError(400, "USERNAME_INVALID"u8);
        }
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return (TLUser)RpcErrorGenerator.GenerateError(400, "AUTH_KEY_INVALID"u8);
        }
        var user = _unitOfWork.UserRepository.GetUserByUsername(username);
        if (user == null)
        {
            _unitOfWork.UserRepository.UpdateUsername(auth.Value.AsAuthInfo().UserId, username);
        }
        else
        {
            return (TLUser)RpcErrorGenerator.GenerateError(400, "USERNAME_OCCUPIED"u8);
        }

        await _unitOfWork.SaveAsync();
        user = _unitOfWork.UserRepository.GetUser(auth.Value.AsAuthInfo().UserId);
        if(user == null) return (TLUser)RpcErrorGenerator.GenerateError(400, "USERNAME_NOT_MODIFIED"u8);
        var userInfo = GetUserInfo(user.Value);
        await _search.IndexUser(new Data.Search.UserSearchModel(userInfo.UserId,
            userInfo.Username, userInfo.FirstName, userInfo.LastName, userInfo.Phone));
        return user.Value;
    }

    public async ValueTask<TLPrivacyRules> SetPrivacy(long authKeyId, TLBytes q)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return (TLPrivacyRules)RpcErrorGenerator.GenerateError(400, "AUTH_KEY_INVALID"u8);
        }
        
        int keyConstructor = MemoryMarshal.Read<int>(((SetPrivacy)q).Key);
        var key = GetPrivacyKey(keyConstructor);
        _unitOfWork.PrivacyRulesRepository.PutPrivacyRules(auth.Value.AsAuthInfo().UserId, 
            key, ToPrivacyRuleVector(((SetPrivacy)q).Rules));
        await _unitOfWork.SaveAsync();
        return await GetPrivacyRulesInternal(auth.Value, key);
    }

    private async Task<TLPrivacyRules> GetPrivacyRulesInternal(TLBytes auth, InputPrivacyKey key)
    {
        var savedRules = await _unitOfWork.PrivacyRulesRepository.GetPrivacyRulesAsync(((AuthInfo)auth).UserId, key);
        List<TLBytes> users = new();
        foreach (var id in GetUserIds(savedRules))
        {
            if (_unitOfWork.UserRepository.GetUser(id) is { } user)
            {
                users.Add(user);
            }
        }

        List<TLBytes> chats = new();
        foreach (var id in GetChatIds(savedRules))
        {
            if (await _unitOfWork.ChatRepository.GetChatAsync(id) is { } chat)
            {
                chats.Add(chat);
            }
        }

        return PrivacyRules.Builder()
            .Rules(savedRules.ToVector())
            .Users(users.ToVector())
            .Chats(chats.ToVector())
            .Build();
    }

    private Vector ToPrivacyRuleVector(Vector rules)
    {
        Vector result = new Vector();
        for (int i = 0; i < rules.Count; i++)
        {
            var rule = ToPrivacyValue(rules.ReadTLObject());
            result.AppendTLObject(rule);
        }

        return result;
    }
    
    private ReadOnlySpan<byte> ToPrivacyValue(Span<byte> inputPrivacyValue)
    {
        int constructor = MemoryMarshal.Read<int>(inputPrivacyValue);
        switch (constructor)
        {
            case Constructors.layer150_InputPrivacyValueAllowContacts:
                return PrivacyValueAllowContacts.Builder().Build().ToReadOnlySpan();
            case Constructors.layer150_InputPrivacyValueAllowAll:
                return PrivacyValueAllowAll.Builder().Build().ToReadOnlySpan();
            case Constructors.layer150_InputPrivacyValueAllowUsers:
                var allowUsers = (InputPrivacyValueAllowUsers)inputPrivacyValue;
                var userVector = allowUsers.Users;
                VectorOfLong userIds = new();
                for (int i = 0; i < userVector.Count; i++)
                {
                    var user = userVector.ReadTLObject();
                    userIds.Append(GetUserId(user));
                }
                return PrivacyValueAllowUsers.Builder().Users(userIds).Build().ToReadOnlySpan();
            case Constructors.layer150_InputPrivacyValueDisallowContacts:
                return PrivacyValueDisallowContacts.Builder().Build().ToReadOnlySpan();
            case Constructors.layer150_InputPrivacyValueDisallowAll:
                return PrivacyValueDisallowAll.Builder().Build().ToReadOnlySpan();
            case Constructors.layer150_InputPrivacyValueDisallowUsers:
                var disallowUsers = (InputPrivacyValueDisallowUsers)inputPrivacyValue;
                var userVector2 = disallowUsers.Users;
                VectorOfLong userIds2 = new();
                for (int i = 0; i < userVector2.Count; i++)
                {
                    var user = userVector2.ReadTLObject();
                    userIds2.Append(GetUserId(user));
                }
                return PrivacyValueDisallowUsers.Builder().Users(userIds2).Build().ToReadOnlySpan();
            case Constructors.layer150_InputPrivacyValueAllowChatParticipants:
                var chats = ((InputPrivacyValueAllowChatParticipants)inputPrivacyValue).Chats;
                return PrivacyValueAllowChatParticipants.Builder().Chats(chats).Build().ToReadOnlySpan();
            case Constructors.layer150_InputPrivacyValueDisallowChatParticipants:
                var chats2 = ((InputPrivacyValueDisallowChatParticipants)inputPrivacyValue).Chats;
                return PrivacyValueDisallowChatParticipants.Builder().Chats(chats2).Build().ToReadOnlySpan();
            default:
                throw new ArgumentException();
        }
    }
    
    private ICollection<long> GetUserIds(ICollection<TLBytes> rules)
    {
        List<long> users = new();
        foreach (var r in rules)
        {
            switch (r.Constructor)
            {
                case Constructors.layer150_PrivacyValueAllowUsers:
                    var v = ((PrivacyValueAllowUsers)r).Users;
                    for (int i = 0; i < v.Count; i++)
                    {
                        users.Add(v[i]);
                    }
                    break;
                case Constructors.layer150_PrivacyValueDisallowUsers:
                    var v2 = ((PrivacyValueDisallowUsers)r).Users;
                    for (int i = 0; i < v2.Count; i++)
                    {
                        users.Add(v2[i]);
                    }
                    break;
            }
        }

        return users;
    }
    
    private ICollection<long> GetChatIds(ICollection<TLBytes> rules)
    {
        List<long> chats = new();
        foreach (var r in rules)
        {
            switch (r.Constructor)
            {
                case Constructors.layer150_PrivacyValueAllowChatParticipants:
                    var v = ((PrivacyValueAllowChatParticipants)r).Chats;
                    for (int i = 0; i < v.Count; i++)
                    {
                        chats.Add(v[i]);
                    }
                    break;
                case Constructors.layer150_PrivacyValueDisallowChatParticipants:
                    var v2 = ((PrivacyValueDisallowChatParticipants)r).Chats;
                    for (int i = 0; i < v2.Count; i++)
                    {
                        chats.Add(v2[i]);
                    }
                    break;
            }
        }

        return chats;
    }

    private long GetUserId(Span<byte> inputUser)
    {
        int constructor = MemoryMarshal.Read<int>(inputUser);
        switch (constructor)
        {
            case Constructors.layer150_InputUser:
                return ((InputUser)inputUser).UserId;
            case Constructors.layer150_InputPeerUserFromMessage:
                return ((InputPeerUserFromMessage)inputUser).UserId;
            default:
                return 0;
        }
    }

    private InputPrivacyKey GetPrivacyKey(int constructor) => constructor switch
    {
        Constructors.layer150_InputPrivacyKeyStatusTimestamp => Data.InputPrivacyKey.StatusTimestamp,
        Constructors.layer150_InputPrivacyKeyChatInvite => Data.InputPrivacyKey.ChatInvite,
        Constructors.layer150_InputPrivacyKeyPhoneCall => Data.InputPrivacyKey.PhoneCall,
        Constructors.layer150_InputPrivacyKeyPhoneP2P => Data.InputPrivacyKey.PhoneP2P,
        Constructors.layer150_InputPrivacyKeyForwards => Data.InputPrivacyKey.Forwards,
        Constructors.layer150_InputPrivacyKeyProfilePhoto => Data.InputPrivacyKey.ProfilePhoto,
        Constructors.layer150_InputPrivacyKeyPhoneNumber => Data.InputPrivacyKey.PhoneNumber,
        _ => Data.InputPrivacyKey.AddedByPhone
    };

    public async ValueTask<TLPrivacyRules> GetPrivacy(long authKeyId, TLBytes q)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return (TLPrivacyRules)RpcErrorGenerator.GenerateError(400, "AUTH_KEY_INVALID"u8);
        }
        
        int keyConstructor = MemoryMarshal.Read<int>(((GetPrivacy)q).Key);
        var key = GetPrivacyKey(keyConstructor);
        return await GetPrivacyRulesInternal(auth.Value, key);
    }

    public async ValueTask<TLBool> DeleteAccount(long authKeyId)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null) return new BoolFalse();
        var authorizations = await _unitOfWork
            .AuthorizationRepository
            .GetAuthorizationsAsync(Encoding.UTF8.GetString(auth.Value.AsAuthInfo().Phone));
        var user = _unitOfWork.UserRepository.GetUser(auth.Value.AsAuthInfo().UserId);
        if(user == null) return new BoolFalse();

        foreach (var a in authorizations)
        {
            var keyId = a.AsAuthInfo().AuthKeyId;
            _unitOfWork.AuthorizationRepository.DeleteAuthorization(keyId);
            var device = _unitOfWork.DeviceInfoRepository.GetDeviceInfo(keyId);
            if (device != null)
            {
                _unitOfWork.DeviceInfoRepository.DeleteDeviceInfo(keyId, device.Token, device.OtherUserIds);
            }
            _unitOfWork.NotifySettingsRepository.DeleteNotifySettings(keyId);
            await _unitOfWork.SaveAsync();
        }

        _unitOfWork.PrivacyRulesRepository.DeletePrivacyRules(user.Value.AsUser().Id);
        _unitOfWork.UserRepository.DeleteUser(user.Value.AsUser().Id);
        await _unitOfWork.SaveAsync();
        await _search.DeleteUser(user.Value.AsUser().Id);
        
        return new BoolTrue();
    }

    public async ValueTask<TLBool> SetAccountTTL(long authKeyId, int accountDaysTtl)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return new BoolFalse();
        }

        _unitOfWork.UserRepository.UpdateAccountTtl(auth.Value.AsAuthInfo().UserId, accountDaysTtl);
        var result = await _unitOfWork.SaveAsync();
        return result ? new BoolTrue() : new BoolFalse();
    }

    public async ValueTask<TLAccountDaysTTL> GetAccountTTL(long authKeyId)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return (TLAccountDaysTTL)RpcErrorGenerator.GenerateError(400, "AUTH_KEY_INVALID"u8);
        }

        var ttlDays = _unitOfWork.UserRepository.GetAccountTtl(auth.Value.AsAuthInfo().UserId);
        return AccountDaysTTL.Builder().Days(ttlDays).Build();
    }

    public async ValueTask<TLSentCode> SendChangePhoneCode(long authKeyId, TLBytes q)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if(auth == null) return (TLSentCode)RpcErrorGenerator.GenerateError(400, "AUTH_KEY_INVALID"u8);
        if (DateTime.Now - DateTimeOffset.FromUnixTimeSeconds(auth.Value.AsAuthInfo().LoggedInAt) < new TimeSpan(1, 0, 0))
        {
            return (TLSentCode)RpcErrorGenerator.GenerateError(406, "FRESH_CHANGE_PHONE_FORBIDDEN"u8);
        }
        var phoneNumber = Encoding.UTF8.GetString(((SendChangePhoneCode)q).PhoneNumber);
        var user = _unitOfWork.UserRepository.GetUser(phoneNumber);
        if (user != null)
        {
            return (TLSentCode)RpcErrorGenerator.GenerateError(406, "PHONE_NUMBER_OCCUPIED"u8);
        }
        
        var code = await _verificationGateway.SendSms(phoneNumber);
        Console.WriteLine("auth.sentCode=>" + code.ToString());
        var hash = GeneratePhoneCodeHash(code);
        
        _unitOfWork.PhoneCodeRepository.PutPhoneCode(phoneNumber, hash, code.ToString(),
            new TimeSpan(0, 0, PhoneCodeTimeout*2));
        await _unitOfWork.SaveAsync();
        
        return GenerateSentCode(hash);
    }

    private static TLSentCode GenerateSentCode(string hash)
    {
        using var codeType = SentCodeTypeSms.Builder().Build();
        TLSentCode sentCode = SentCode.Builder()
            .Type(codeType.ToReadOnlySpan())
            .PhoneCodeHash(Encoding.UTF8.GetBytes(hash))
            .Timeout(PhoneCodeTimeout)
            .Build();
        return sentCode;
    }
    
    private string GeneratePhoneCodeHash(string code)
    {
        var codeBytes = Encoding.UTF8.GetBytes(code);
        return codeBytes.GetXxHash64(1071).ToString("x");
    }

    public async ValueTask<TLUser> ChangePhone(long authKeyId, TLBytes q)
    {
        var phoneNumber = Encoding.UTF8.GetString(((ChangePhone)q).PhoneNumber);
        var phoneCodeHash = Encoding.UTF8.GetString(((ChangePhone)q).PhoneCodeHash);
        var phoneCode = Encoding.UTF8.GetString(((ChangePhone)q).PhoneCode);
        var code = _unitOfWork.PhoneCodeRepository.GetPhoneCode(phoneNumber, phoneCodeHash);
        if (phoneCode != code)
        {
            return (TLUser)RpcErrorGenerator.GenerateError(400, "PHONE_CODE_EXPIRED"u8);
        }

        var user = _unitOfWork.UserRepository.GetUser(phoneNumber);
        if (user != null)
        {
            return (TLUser)RpcErrorGenerator.GenerateError(400, "PHONE_NUMBER_OCCUPIED"u8);
        }
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if(auth == null) return (TLUser)RpcErrorGenerator.GenerateError(400, "AUTH_KEY_INVALID"u8);
        var authorizations = await _unitOfWork
            .AuthorizationRepository.GetAuthorizationsAsync(Encoding.UTF8.GetString(auth.Value.AsAuthInfo().Phone));
        foreach (var authorization in authorizations)
        {
            _unitOfWork.AuthorizationRepository.PutAuthorization(authorization.AsAuthInfo()
                .Clone()
                .Phone(Encoding.UTF8.GetBytes(phoneNumber))
                .Build());
        }
        _unitOfWork.UserRepository.UpdateUserPhone(auth.Value.AsAuthInfo().UserId, phoneNumber);
        await _unitOfWork.SaveAsync();
        user = _unitOfWork.UserRepository.GetUser(phoneNumber);
        await _unitOfWork.SaveAsync();
        return user!.Value;
    }

    public async ValueTask<TLBool> UpdateDeviceLocked(long authKeyId, int period)
    {
         _unitOfWork.DeviceLockedRepository.PutDeviceLocked(authKeyId, TimeSpan.FromSeconds(period));
         var result = await _unitOfWork.SaveAsync();
         return result ? new BoolTrue() : new BoolFalse();
    }

    public async ValueTask<TLAuthorizations> GetAuthorizations(long authKeyId)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if(auth == null) return (TLAuthorizations)RpcErrorGenerator.GenerateError(400, "AUTH_KEY_INVALID"u8);
        var authorizations = await _unitOfWork
            .AuthorizationRepository.GetAuthorizationsAsync(Encoding.UTF8.GetString(auth.Value.AsAuthInfo().Phone));
        List<TLAppInfo> infos = new();
        foreach (var a in authorizations)
        {
            if(!a.AsAuthInfo().LoggedIn) continue;
            var authorization = _unitOfWork.AppInfoRepository.GetAppInfo(a.AsAuthInfo().AuthKeyId);
            if (authorization != null) infos.Add(authorization.Value);
        }

        return GenerateAuthorizations(_unitOfWork.UserRepository.GetAccountTtl(auth.Value.AsAuthInfo().UserId), infos);
    }

    private TLAuthorizations GenerateAuthorizations(int ttl, List<TLAppInfo> infos)
    {
        Vector authVector = new();
        foreach (var info in infos)
        {
            var a = info.AsAppInfo();
            using var auth = Authorization.Builder()
                .Hash(a.Hash)
                .DeviceModel(a.DeviceModel)
                .Platform("Unknown"u8)
                .SystemVersion(a.SystemVersion)
                .ApiId(a.ApiId)
                .AppName("Unknown"u8)
                .AppVersion(a.AppVersion)
                .DateCreated((int)DateTimeOffset.Now.ToUnixTimeSeconds())
                .DateActive((int)DateTimeOffset.Now.ToUnixTimeSeconds())
                .Ip(a.Ip)
                .Country("Turkey"u8)
                .Region("Unknown"u8)
                .Build();
            authVector.AppendTLObject(auth.ToReadOnlySpan());
        }
        return Authorizations.Builder()
            .AuthorizationTtlDays(ttl)
            .AuthorizationsProperty(authVector)
            .Build();
    }

    public async ValueTask<TLBool> ResetAuthorization(long authKeyId, long hash)
    {
        var sessAuthKeyId = _unitOfWork.AppInfoRepository.GetAuthKeyIdByAppHash(hash);
        if (sessAuthKeyId == null)
        {
            return (TLBool)RpcErrorGenerator.GenerateError(400, "HASH_INVALID"u8);
        }
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if(auth == null) return (TLBool)RpcErrorGenerator.GenerateError(400, "AUTH_KEY_INVALID"u8);
        if (DateTime.Now - DateTimeOffset.FromUnixTimeSeconds(auth.Value.AsAuthInfo().LoggedInAt) < new TimeSpan(1, 0, 0))
        {
            return (TLBool)RpcErrorGenerator.GenerateError(406, "FRESH_RESET_AUTHORISATION_FORBIDDEN"u8);
        }
        var info = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync((long)sessAuthKeyId);
        if(info == null)
        {
            return (TLBool)RpcErrorGenerator.GenerateError(400, "HASH_INVALID"u8);
        }

        _unitOfWork.AuthorizationRepository.DeleteAuthorization(sessAuthKeyId.Value);
        var result = await _unitOfWork.SaveAsync();
        return result ? new BoolTrue() : new BoolFalse();
    }

    public async ValueTask<TLBool> SetContactSignUpNotification(long authKeyId, bool silent)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if(auth == null) return (TLBool)RpcErrorGenerator.GenerateError(400, "AUTH_KEY_INVALID"u8);
        _unitOfWork.SignUpNotificationRepository.PutSignUpNotification(auth.Value.AsAuthInfo().UserId, silent);
        var result = await _unitOfWork.SaveAsync();
        return result ? new BoolTrue() : new BoolFalse();
    }

    public async ValueTask<TLBool> GetContactSignUpNotification(long authKeyId)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if(auth == null) return (TLBool)RpcErrorGenerator.GenerateError(400, "AUTH_KEY_INVALID"u8);
        var result = !_unitOfWork.SignUpNotificationRepository.GetSignUpNotification(auth.Value.AsAuthInfo().UserId);
        return result ? new BoolTrue() : new BoolFalse();
    }

    public async ValueTask<TLBool> ChangeAuthorizationSettings(long authKeyId, long hash, 
        bool encryptedRequestsDisabled, bool callRequestsDisabled)
    {
        var appAuthKeyId = _unitOfWork.AppInfoRepository.GetAuthKeyIdByAppHash(hash);
        if(appAuthKeyId == null)
        {
            return (TLBool)RpcErrorGenerator.GenerateError(400, "HASH_INVALID"u8);
        }
        var info = _unitOfWork.AppInfoRepository.GetAppInfo((long)appAuthKeyId);
        if (info == null) return new BoolFalse();
        var success = _unitOfWork.AppInfoRepository.PutAppInfo(info.Value.AsAppInfo().Clone()
            .EncryptedRequestsDisabled(encryptedRequestsDisabled)
            .CallRequestsDisabled(callRequestsDisabled)
            .Build());
        var result = success && await _unitOfWork.SaveAsync();
        return result ? new BoolTrue() : new BoolFalse();

    }
}