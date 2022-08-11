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
using System;
using System.Collections.ObjectModel;
using Ferrite.Data;
using Ferrite.Data.Repositories;
using Ferrite.Data.Updates;
using Ferrite.Utils;
using MessagePack;

namespace Ferrite.Services;

public class UpdatesService : IUpdatesService
{
    private readonly IMTProtoTime _time;
    private readonly ISessionService _sessions;
    private readonly IDistributedPipe _pipe;
    private readonly IPersistentStore _store;
    private readonly IDistributedCache _cache;
    private readonly IUnitOfWork _unitOfWork;
    public UpdatesService(IMTProtoTime time, ISessionService sessions, IDistributedPipe pipe,
        IPersistentStore store, IDistributedCache cache, IUnitOfWork unitOfWork)
    {
        _time = time;
        _sessions = sessions;
        _pipe = pipe;
        _store = store;
        _cache = cache;
        _unitOfWork = unitOfWork;
    }

    public async Task<StateDTO> GetState(long authKeyId)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        var updatesCtx = _cache.GetUpdatesContext(authKeyId, auth.UserId);
        return new StateDTO()
        {
            Date = (int)_time.GetUnixTimeInSeconds(),
            Pts = await updatesCtx.Pts(),
            Seq = await updatesCtx.Seq(),
        };
    }

    public async Task<ServiceResult<DifferenceDTO>> GetDifference(long authKeyId, int pts, int date, 
        int qts, int? ptsTotalLimit = null)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        var updatesCtx = _cache.GetUpdatesContext(authKeyId, auth.UserId);
        int currentPts = await updatesCtx.Pts();
        var state = new StateDTO()
        {
            Date = (int)_time.GetUnixTimeInSeconds(),
            Pts = currentPts,
            Seq = await updatesCtx.Seq(),
        };
        var messages = await _unitOfWork.MessageRepository.GetMessagesAsync(auth.UserId,
            pts, currentPts, DateTimeOffset.FromUnixTimeSeconds(date));
        List<UserDTO> users = new List<UserDTO>();
        foreach (var message in messages)
        {
            if (message.Out && message.PeerId.PeerType == PeerType.User 
                            && message.PeerId.PeerId != auth.UserId)
            {
                var user = await _store.GetUserAsync(message.PeerId.PeerId);
                users.Add(user);
            }
            else if (!message.Out && message.FromId.PeerType == PeerType.User
                                  && message.PeerId.PeerId != auth.UserId)
            {
                var user = await _store.GetUserAsync(message.FromId.PeerId);
                users.Add(user);
            }
        }

        var difference = new DifferenceDTO(messages, Array.Empty<EncryptedMessageDTO>(),
            Array.Empty<UpdateBase>(), Array.Empty<ChatDTO>(),
            users, state);
        return new ServiceResult<DifferenceDTO>(difference, true, ErrorMessages.None);
    }

    public async Task<bool> EnqueueUpdate(long userId, UpdateBase update)
    {
        var user = await _store.GetUserAsync(userId);
        var authorizations = await _store.GetAuthorizationsAsync(user.Phone);
        //TODO: we should be able to get the active sessions by the userId
        foreach (var a in authorizations)
        {
            var updatesCtx = _cache.GetUpdatesContext(a.AuthKeyId, a.UserId);
            int seq = await updatesCtx.IncrementSeq();
            var updateList = new List<UpdateBase>() { update };
            List<UserDTO> userList = new();
            List<ChatDTO> chatList = new();
            if (update is UpdateReadHistoryInboxDTO readHistoryInbox)
            {
                var peerUser = await _store.GetUserAsync(readHistoryInbox.Peer.PeerId);
                if (peerUser != null) userList.Add(peerUser);
            }
            else if (update is UpdateReadHistoryOutboxDTO readHistoryOutbox)
            {
                var peerUser = await _store.GetUserAsync(readHistoryOutbox.Peer.PeerId);
                if (peerUser != null) userList.Add(peerUser);
            }
            else if (update is UpdateNewMessageDTO messageNotification &&
                     messageNotification.Message.FromId is { PeerType: PeerType.User })
            {
                var peerUser = await _store.GetUserAsync(messageNotification.Message.FromId.PeerId);
                if (peerUser != null) userList.Add(peerUser);
            }

            
            var updates = new UpdatesDTO(updateList, userList, chatList, 
                (int)DateTimeOffset.Now.ToUnixTimeSeconds(), seq);
            var sessions = await _sessions.GetSessionsAsync(a.AuthKeyId);
            foreach (var s in sessions)
            {
                byte[] data = MessagePackSerializer.Typeless.Serialize(updates);
                MTProtoMessage message = new MTProtoMessage
                {
                    Data = data,
                    SessionId = s.SessionId,
                    IsContentRelated = true,
                    IsResponse = false,
                    MessageType = MTProtoMessageType.Updates,
                };
                var bytes = MessagePackSerializer.Serialize(message);
                _ = _pipe.WriteMessageAsync(s.NodeId.ToString(), bytes);
            }
        }

        return true;
    }

    public async Task<int> IncrementUpdatesSequence(long authKeyId)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        var updatesCtx = _cache.GetUpdatesContext(authKeyId, auth.UserId);
        return await updatesCtx.IncrementSeq();
    }
}

