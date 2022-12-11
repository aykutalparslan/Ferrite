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
    private readonly IMessagePipe _pipe;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUpdatesContextFactory _updatesContextFactory;
    private readonly ILogger _log;

    public UpdatesService(IMTProtoTime time, ISessionService sessions, IMessagePipe pipe,
        IUnitOfWork unitOfWork, IUpdatesContextFactory updatesContextFactory,
        ILogger log)
    {
        _time = time;
        _sessions = sessions;
        _pipe = pipe;
        _unitOfWork = unitOfWork;
        _updatesContextFactory = updatesContextFactory;
        _log = log;
    }

    public async Task<StateDTO> GetState(long authKeyId)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var updatesCtx = _updatesContextFactory.GetUpdatesContext(authKeyId, auth.UserId);
        StateDTO state = await GetStateInternal(updatesCtx);
        _log.Debug($"/// State is {state} ///");
        return state;
    }

    private async Task<StateDTO> GetStateInternal(IUpdatesContext updatesCtx)
    {
        var state = new StateDTO()
        {
            Date = (int)_time.GetUnixTimeInSeconds(),
            Pts = await updatesCtx.Pts(),
            Seq = await updatesCtx.Seq(),
            Qts = await updatesCtx.Qts(),
            UnreadCount = await updatesCtx.UnreadMessages(),
        };
        return state;
    }

    public async Task<ServiceResult<DifferenceDTO>> GetDifference(long authKeyId, int pts, int date, 
        int qts, int? ptsTotalLimit = null)
    {
        /*var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null) return new ServiceResult<DifferenceDTO>(null, false, ErrorMessages.InvalidAuthKey);
        var updatesCtx = _updatesContextFactory.GetUpdatesContext(authKeyId, auth.UserId);
        int currentPts = await updatesCtx.Pts();
        StateDTO state = await GetStateInternal(updatesCtx);
        var messages = await _unitOfWork.MessageRepository.GetMessagesAsync(auth.UserId,
            pts, currentPts, DateTimeOffset.FromUnixTimeSeconds(date));
        List<UserDTO> users = new List<UserDTO>();
        foreach (var message in messages)
        {
            if (message.Out && message.PeerId.PeerType == PeerType.User)
            {
                var user = _unitOfWork.UserRepository.GetUser(message.PeerId.PeerId);
                if(user != null) users.Add(user);
            }
            else if (!message.Out && message.FromId is { PeerType: PeerType.User })
            {
                var user = _unitOfWork.UserRepository.GetUser(message.FromId.PeerId);
                if(user != null) users.Add(user);
            }
        }

        var difference = new DifferenceDTO(messages, Array.Empty<EncryptedMessageDTO>(),
            Array.Empty<UpdateBase>(), Array.Empty<ChatDTO>(),
            users, state);
        return new ServiceResult<DifferenceDTO>(difference, true, ErrorMessages.None);*/
        throw new NotImplementedException();
    }

    public async Task<bool> EnqueueUpdate(long userId, UpdateBase update)
    {
        /*var user = _unitOfWork.UserRepository.GetUser(userId);
        var authorizations = await _unitOfWork.AuthorizationRepository.GetAuthorizationsAsync(user.Phone);
        //TODO: we should be able to get the active sessions by the userId
        foreach (var a in authorizations)
        {
            var updatesCtx = _updatesContextFactory.GetUpdatesContext(a.AuthKeyId, a.UserId);
            int seq = await updatesCtx.IncrementSeq();
            var updateList = new List<UpdateBase>() { update };
            List<UserDTO> userList = new();
            List<ChatDTO> chatList = new();
            UpdatesBase updates = null;
            if (update is UpdateReadHistoryInboxDTO readHistoryInbox)
            {
                var peerUser = _unitOfWork.UserRepository.GetUser(readHistoryInbox.Peer.PeerId);
                if (peerUser != null) userList.Add(peerUser);
                updates = new UpdatesDTO(updateList, userList, chatList, 
                    (int)DateTimeOffset.Now.ToUnixTimeSeconds(), seq);
            }
            else if (update is UpdateReadHistoryOutboxDTO readHistoryOutbox)
            {
                var peerUser = _unitOfWork.UserRepository.GetUser(readHistoryOutbox.Peer.PeerId);
                if (peerUser != null) userList.Add(peerUser);
                updates = new UpdatesDTO(updateList, userList, chatList, 
                    (int)DateTimeOffset.Now.ToUnixTimeSeconds(), seq);
            }
            else if (update is UpdateNewMessageDTO messageNotification &&
                     messageNotification.Message.FromId is { PeerType: PeerType.User })
            {
                var peerUser = _unitOfWork.UserRepository.GetUser(messageNotification.Message.FromId.PeerId);
                if (peerUser != null) userList.Add(peerUser);
                updates = new UpdatesDTO(updateList, userList, chatList, 
                    (int)DateTimeOffset.Now.ToUnixTimeSeconds(), seq);
            }
            else if (update is UpdateUserTypingDTO userTyping)
            {
                updates = new UpdateShortDTO(update, (int)DateTimeOffset.Now.ToUnixTimeSeconds());
            }

            if (updates == null)
            {
                return false;
            }
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
                await _pipe.WriteMessageAsync(s.NodeId.ToString(), bytes);
            }
        }

        return true;*/
        return false;
    }

    public async Task<int> IncrementUpdatesSequence(long authKeyId)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var updatesCtx = _updatesContextFactory.GetUpdatesContext(authKeyId, auth.UserId);
        return await updatesCtx.IncrementSeq();
    }
}

