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

    public async ValueTask<TLBytes> ResetNotifySettings(long authKeyId)
    {
        _unitOfWork.NotifySettingsRepository.DeleteNotifySettings(authKeyId);
        var result = await _unitOfWork.SaveAsync();
        return result ? BoolTrue.Builder().Build().TLBytes!.Value : 
            BoolFalse.Builder().Build().TLBytes!.Value;
    }

    public async ValueTask<TLBytes> UpdateProfile(long authKeyId, string? firstName, string? lastName, string? about)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth != null && _unitOfWork.UserRepository.GetUser(auth.UserId) is { } u )
        {
            var user = ModifyUser(u, firstName, lastName);
            _unitOfWork.UserRepository.PutUser(user);
            var userInfo = GetUserInfo(user);
            if (about != null) _unitOfWork.UserRepository.PutAbout(userInfo.UserId, about);
            await _unitOfWork.SaveAsync();
            await _search.IndexUser(new Data.Search.UserSearchModel(userInfo.UserId, userInfo.Username, 
                userInfo.FirstName, userInfo.LastName, userInfo.Phone));
            return user;
        }

        return RpcErrorGenerator.GenerateError(400,"FIRSTNAME_INVALID"u8);
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

    private static TLBytes ModifyUser(TLBytes u, string? firstName, string? lastName)
    {
        var user = new User(u.AsSpan()).Clone();
        if (firstName != null) user = user.FirstName(Encoding.UTF8.GetBytes(firstName));
        if (lastName != null) user = user.LastName(Encoding.UTF8.GetBytes(lastName));
        var userBytes = user.Build().ToReadOnlySpan().ToArray();
        return new TLBytes(userBytes, 0, userBytes.Length);
    }

    public async ValueTask<TLBytes> UpdateStatus(long authKeyId, bool status)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth != null)
        {
            var result = _unitOfWork.UserStatusRepository.PutUserStatus(auth.UserId, status);
            return result ? BoolTrue.Builder().Build().TLBytes!.Value : 
                BoolFalse.Builder().Build().TLBytes!.Value;
        }

        return BoolFalse.Builder().Build().TLBytes!.Value;
    }

    public async ValueTask<TLBytes> ReportPeer(long authKeyId, TLBytes q)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return RpcErrorGenerator.GenerateError(400, "AUTH_KEY_INVALID"u8);
        }

        using var reason = GetReportPeerParameters( q);
        _unitOfWork.ReportReasonRepository.PutPeerReportReason(auth.UserId, reason.PeerType, reason.PeerId, reason.Reason);
        var result = await _unitOfWork.SaveAsync();
        return result ? BoolTrue.Builder().Build().TLBytes!.Value : 
            BoolFalse.Builder().Build().TLBytes!.Value;
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

    public ValueTask<TLBytes> CheckUsername(string username)
    {
        if (!UsernameRegex.IsMatch(username))
        {
            return ValueTask.FromResult(BoolFalse.Builder().Build().TLBytes!.Value);
        }

        var user = _unitOfWork.UserRepository.GetUserByUsername(username);
        if (user != null)
        {
            return ValueTask.FromResult(BoolFalse.Builder().Build().TLBytes!.Value);
        }

        return ValueTask.FromResult(BoolTrue.Builder().Build().TLBytes!.Value);
    }

    public async ValueTask<TLBytes> UpdateUsername(long authKeyId, string username)
    {
        if (!UsernameRegex.IsMatch(username))
        {
            return RpcErrorGenerator.GenerateError(400, "USERNAME_INVALID"u8);
        }
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return RpcErrorGenerator.GenerateError(400, "AUTH_KEY_INVALID"u8);
        }
        var user = _unitOfWork.UserRepository.GetUserByUsername(username);
        if (user == null)
        {
            _unitOfWork.UserRepository.UpdateUsername(auth.UserId, username);
        }
        else
        {
            return RpcErrorGenerator.GenerateError(400, "USERNAME_OCCUPIED"u8);
        }

        await _unitOfWork.SaveAsync();
        user = _unitOfWork.UserRepository.GetUser(auth.UserId);
        if(user == null) return RpcErrorGenerator.GenerateError(400, "USERNAME_NOT_MODIFIED"u8);
        var userInfo = GetUserInfo(user.Value);
        await _search.IndexUser(new Data.Search.UserSearchModel(userInfo.UserId,
            userInfo.Username, userInfo.FirstName, userInfo.LastName, userInfo.Phone));
        return user.Value;
    }

    public async ValueTask<TLBytes> SetPrivacy(long authKeyId, TLBytes q)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return RpcErrorGenerator.GenerateError(400, "AUTH_KEY_INVALID"u8);
        }
        
        int keyConstructor = MemoryMarshal.Read<int>(((SetPrivacy)q).Key);
        var key = GetPrivacyKey(keyConstructor);
        _unitOfWork.PrivacyRulesRepository.PutPrivacyRules(auth.UserId, 
            key, ToPrivacyRuleVector(((SetPrivacy)q).Rules));
        await _unitOfWork.SaveAsync();
        return await GetPrivacyRulesInternal(auth, key);
    }

    private async Task<TLBytes> GetPrivacyRulesInternal(AuthInfoDTO auth, InputPrivacyKey key)
    {
        var savedRules = await _unitOfWork.PrivacyRulesRepository.GetPrivacyRulesAsync(auth.UserId, key);
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
            .Rules(GenerateVector(savedRules))
            .Users(GenerateVector(users))
            .Chats(GenerateVector(chats))
            .Build().TLBytes!.Value;
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

    private Vector GenerateVector(ICollection<TLBytes> rules)
    {
        var vec = new Vector();
        foreach (var r in rules)
        {
            vec.AppendTLObject(r.AsSpan());
        }

        return vec;
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

    public async ValueTask<TLBytes> GetPrivacy(long authKeyId, TLBytes q)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return RpcErrorGenerator.GenerateError(400, "AUTH_KEY_INVALID"u8);
        }
        
        int keyConstructor = MemoryMarshal.Read<int>(((GetPrivacy)q).Key);
        var key = GetPrivacyKey(keyConstructor);
        return await GetPrivacyRulesInternal(auth, key);
    }

    public async ValueTask<TLBytes> DeleteAccount(long authKeyId)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var authorizations = await _unitOfWork.AuthorizationRepository.GetAuthorizationsAsync(auth.Phone);
        var user = _unitOfWork.UserRepository.GetUser(auth.UserId);
        if(user == null) return BoolFalse.Builder().Build().TLBytes!.Value;

        foreach (var a in authorizations)
        {
            _unitOfWork.AuthorizationRepository.DeleteAuthorization(a.AuthKeyId);
            var device = _unitOfWork.DeviceInfoRepository.GetDeviceInfo(a.AuthKeyId);
            if (device != null)
            {
                _unitOfWork.DeviceInfoRepository.DeleteDeviceInfo(a.AuthKeyId, device.Token, device.OtherUserIds);
            }
            _unitOfWork.NotifySettingsRepository.DeleteNotifySettings(a.AuthKeyId);
            await _unitOfWork.SaveAsync();
        }

        _unitOfWork.PrivacyRulesRepository.DeletePrivacyRules(((User)user).Id);
        _unitOfWork.UserRepository.DeleteUser(((User)user).Id);
        await _unitOfWork.SaveAsync();
        await _search.DeleteUser(((User)user).Id);
        
        return BoolTrue.Builder().Build().TLBytes!.Value;
    }

    public async ValueTask<TLBytes> SetAccountTTL(long authKeyId, int accountDaysTtl)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return BoolFalse.Builder().Build().TLBytes!.Value;
        }

        _unitOfWork.UserRepository.UpdateAccountTtl(auth.UserId, accountDaysTtl);
        var result = await _unitOfWork.SaveAsync();
        return result ? BoolTrue.Builder().Build().TLBytes!.Value : 
            BoolFalse.Builder().Build().TLBytes!.Value;
    }

    public async ValueTask<TLBytes> GetAccountTTL(long authKeyId)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return RpcErrorGenerator.GenerateError(400, "AUTH_KEY_INVALID"u8);
        }

        var ttlDays = _unitOfWork.UserRepository.GetAccountTtl(auth.UserId);
        return AccountDaysTTL.Builder().Days(ttlDays).Build().TLBytes!.Value;
    }

    public async ValueTask<TLBytes> SendChangePhoneCode(long authKeyId, TLBytes q)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (DateTime.Now - auth.LoggedInAt < new TimeSpan(1, 0, 0))
        {
            return RpcErrorGenerator.GenerateError(406, "FRESH_CHANGE_PHONE_FORBIDDEN"u8);
        }
        var phoneNumber = Encoding.UTF8.GetString(((SendChangePhoneCode)q).PhoneNumber);
        var user = _unitOfWork.UserRepository.GetUser(phoneNumber);
        if (user != null)
        {
            return RpcErrorGenerator.GenerateError(406, "PHONE_NUMBER_OCCUPIED"u8);
        }
        
        var code = await _verificationGateway.SendSms(phoneNumber);
        Console.WriteLine("auth.sentCode=>" + code.ToString());
        var hash = GeneratePhoneCodeHash(code);
        
        _unitOfWork.PhoneCodeRepository.PutPhoneCode(phoneNumber, hash, code.ToString(),
            new TimeSpan(0, 0, PhoneCodeTimeout*2));
        await _unitOfWork.SaveAsync();
        return SentCode.Builder()
            .Type(SentCodeTypeSms.Builder().Build().ToReadOnlySpan())
            .PhoneCodeHash(Encoding.UTF8.GetBytes(hash))
            .Timeout(PhoneCodeTimeout)
            .Build().TLBytes!.Value;
    }
    
    private string GeneratePhoneCodeHash(string code)
    {
        var codeBytes = Encoding.UTF8.GetBytes(code);
        return codeBytes.GetXxHash64(1071).ToString("x");
    }

    public async ValueTask<TLBytes> ChangePhone(long authKeyId, TLBytes q)
    {
        var phoneNumber = Encoding.UTF8.GetString(((ChangePhone)q).PhoneNumber);
        var phoneCodeHash = Encoding.UTF8.GetString(((ChangePhone)q).PhoneCodeHash);
        var phoneCode = Encoding.UTF8.GetString(((ChangePhone)q).PhoneCode);
        var code = _unitOfWork.PhoneCodeRepository.GetPhoneCode(phoneNumber, phoneCodeHash);
        if (phoneCode != code)
        {
            return RpcErrorGenerator.GenerateError(400, "PHONE_CODE_EXPIRED"u8);
        }

        var user = _unitOfWork.UserRepository.GetUser(phoneNumber);
        if (user != null)
        {
            return RpcErrorGenerator.GenerateError(400, "PHONE_NUMBER_OCCUPIED"u8);
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
        return user!.Value;
    }

    public async ValueTask<TLBytes> UpdateDeviceLocked(long authKeyId, int period)
    {
         _unitOfWork.DeviceLockedRepository.PutDeviceLocked(authKeyId, TimeSpan.FromSeconds(period));
         var result = await _unitOfWork.SaveAsync();
         return result ? BoolTrue.Builder().Build().TLBytes!.Value : 
             BoolFalse.Builder().Build().TLBytes!.Value;
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