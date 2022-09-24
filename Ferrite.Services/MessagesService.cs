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
using Ferrite.Data.Messages;
using Ferrite.Data.Repositories;
using Ferrite.Data.Search;
using PeerSettingsDTO = Ferrite.Data.Messages.PeerSettingsDTO;

namespace Ferrite.Services;

public class MessagesService : IMessagesService
{
    private readonly IPersistentStore _store;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISearchEngine _search;
    private readonly IUpdatesService _updates;
    private readonly IUpdatesContextFactory _updatesContextFactory;

    public MessagesService(IPersistentStore store, IUnitOfWork unitOfWork, 
        ISearchEngine search, IUpdatesService updates, IUpdatesContextFactory updatesContextFactory)
    {
        _store = store;
        _unitOfWork = unitOfWork;
        _search = search;
        _updates = updates;
        _updatesContextFactory = updatesContextFactory;
    }

    public async Task<ServiceResult<MessagesDTO>> GetMessagesAsync(long authKeyId, IReadOnlyCollection<InputMessageDTO> id)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
            return new ServiceResult<MessagesDTO>(null, false, ErrorMessages.InvalidAuthKey);
        List<MessageDTO> messages = new List<MessageDTO>();
        List<UserDTO> users = new List<UserDTO>();
        foreach (var input in id)
        {
            if (input.InputMessageType == InputMessageType.Id)
            {
                var message = await _unitOfWork.MessageRepository.GetMessageAsync(auth.UserId, (int)input.Id);
                messages.Add(message);
                if (message.Out && message.PeerId.PeerType == PeerType.User)
                {
                    var user = await _store.GetUserAsync(message.PeerId.PeerId);
                    users.Add(user);
                }
                else if (!message.Out && message.FromId.PeerType == PeerType.User)
                {
                    var user = await _store.GetUserAsync(message.FromId.PeerId);
                    users.Add(user);
                }
            }
        }

        return new ServiceResult<MessagesDTO>(new MessagesDTO(MessagesType.Messages, messages, 
            Array.Empty<ChatDTO>(), users), true, ErrorMessages.None);
    }

    public async Task<ServiceResult<PeerSettingsDTO>> GetPeerSettings(long authKeyId, InputPeerDTO peer)
    {
        if (peer.InputPeerType == InputPeerType.Self)
        {
            var settings = new Data.PeerSettingsDTO(false, false, false, 
                false, false, false, 
                false, false, false, 
                null, null, null);
            return new ServiceResult<PeerSettingsDTO>(new PeerSettingsDTO(settings, new List<ChatDTO>(), new List<UserDTO>())
                , true, ErrorMessages.None);
        }
        else if (peer.InputPeerType == InputPeerType.User)
        {
            var settings = new Data.PeerSettingsDTO(true, true, true, 
                false, false,  false, 
                false, false, false, 
                null, null, null);
            var users = new List<UserDTO>();
            var user = await _store.GetUserAsync(peer.UserId);
            users.Add(user);
            return new ServiceResult<PeerSettingsDTO>(new PeerSettingsDTO(settings, new List<ChatDTO>(), users)
                , true, ErrorMessages.None);
        }
        return new ServiceResult<PeerSettingsDTO>(null, false, ErrorMessages.PeerIdInvalid);
    }

    public async Task<ServiceResult<UpdateShortSentMessageDTO>> SendMessage(long authKeyId, bool noWebpage, bool silent, bool background, bool clearDraft, bool noForwards,
        InputPeerDTO peer, string message, long randomId, int? replyToMsgId, ReplyMarkupDTO? replyMarkup,
        IReadOnlyCollection<MessageEntityDTO>? entities, int? scheduleDate, InputPeerDTO? sendAs)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var senderCtx = _updatesContextFactory.GetUpdatesContext(authKeyId, auth.UserId);
        int senderMessageId = (int)await senderCtx.NextMessageId();
        var from = new PeerDTO(PeerType.User, auth.UserId);
        var to = PeerFromInputPeer(peer);
        var outgoingMessage = new MessageDTO()
        {
            Id = senderMessageId,
            Out = true,
            Silent = silent,
            FromId = from,
            PeerId = to,
            MessageText = message,
            ReplyMarkup = replyMarkup,
            Entities = entities,
            Date = (int)DateTimeOffset.Now.ToUnixTimeSeconds()
        };
        if (replyToMsgId != null)
        {
            outgoingMessage.ReplyTo = new MessageReplyHeaderDTO((int)replyToMsgId, null, null);
        }

        if (to.PeerId != from.PeerId)
        {
            var receiverCtx = _updatesContextFactory.GetUpdatesContext(null, to.PeerId);
            int receiverMessageId = await receiverCtx.NextMessageId();
            var incomingMessage = outgoingMessage with
            {
                Id = receiverMessageId,
                Out = false,
                PeerId = from
            };
            int ptsPeer = await receiverCtx.IncrementPts();
            _unitOfWork.MessageRepository.PutMessage(to.PeerId, incomingMessage, ptsPeer);
            UpdateNewMessageDTO updateNewMessage = new UpdateNewMessageDTO(incomingMessage, ptsPeer, 1);
            await _updates.EnqueueUpdate(to.PeerId, updateNewMessage);
        }
        
        int pts = await senderCtx.IncrementPts();
       
        _unitOfWork.MessageRepository.PutMessage(auth.UserId,
             outgoingMessage, pts);
       
        /*var searchModelOutgoing = new MessageSearchModel(
            from.PeerId + "_" + outgoingMessage.Id,
            from.PeerId, (int)from.PeerType, from.PeerId,
            (int)to.PeerType, to.PeerId,outgoingMessage.Id,
            null, outgoingMessage.MessageText, 
            outgoingMessage.Date);
        await _search.IndexMessage(searchModelOutgoing);
        var searchModelIncoming = new MessageSearchModel(
            to.PeerId + "_" + incomingMessage.Id,
            to.PeerId, (int)to.PeerType, to.PeerId,
            (int)from.PeerType, from.PeerId,incomingMessage.Id,
            null, incomingMessage.MessageText, 
            incomingMessage.Date);
        await _search.IndexMessage(searchModelIncoming);*/
        await _unitOfWork.SaveAsync();

        return new ServiceResult<UpdateShortSentMessageDTO>(new UpdateShortSentMessageDTO(true, senderMessageId,
                (int)pts, 1, (int)DateTimeOffset.Now.ToUnixTimeSeconds(), null, null, null), 
            true, ErrorMessages.None);
    }

    public async Task<ServiceResult<AffectedMessagesDTO>> ReadHistory(long authKeyId, InputPeerDTO peer, int maxId)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var userCtx = _updatesContextFactory.GetUpdatesContext(authKeyId, auth.UserId);
        var peerDto = PeerFromInputPeer(peer);
        if (peerDto.PeerType == PeerType.User)
        {
            var peerCtx = _updatesContextFactory.GetUpdatesContext(null, peer.UserId);
            var unread = await userCtx.ReadMessages(peerDto, maxId);
            int userPts = await userCtx.IncrementPts();
            var updateInbox = new UpdateReadHistoryInboxDTO(peerDto, maxId, unread, userPts, 1);
            var updateOutbox = new UpdateReadHistoryOutboxDTO(new PeerDTO(PeerType.User, auth.UserId), maxId,
                await peerCtx.Pts(), 1);
            _updates.EnqueueUpdate(auth.UserId, updateInbox);
            _updates.EnqueueUpdate(peerDto.PeerId, updateOutbox);
            return new ServiceResult<AffectedMessagesDTO>(
                new AffectedMessagesDTO(userPts
                    , 1), true, ErrorMessages.None);
        }

        throw new NotSupportedException();
    }

    public async Task<ServiceResult<AffectedHistoryDTO>> DeleteHistory(long authKeyId, InputPeerDTO peer, 
        int maxId, int? minDate = null, int? maxDate = null,
        bool justClear = false, bool revoke = false)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var userCtx = _updatesContextFactory.GetUpdatesContext(authKeyId, auth.UserId);
        var peerDto = PeerFromInputPeer(peer);
        if (peerDto.PeerType == PeerType.User)
        {
            var messages = _unitOfWork.MessageRepository.GetMessages(auth.UserId, peerDto);
            List<int> deletedIds = new();
            foreach (var m in messages)
            {
                if (m.Id < maxId && (minDate != null && m.Date > minDate) &&
                    (maxDate != null && m.Date < maxDate))
                {
                    deletedIds.Add(m.Id);
                    _unitOfWork.MessageRepository.DeleteMessage(auth.UserId, m.Id);
                }
            }

            int pts = await userCtx.IncrementPts();
            var deleteMessages = new UpdateDeleteMessagesDTO(deletedIds, pts, 1);
            _updates.EnqueueUpdate(auth.UserId, deleteMessages);
            
            if (!justClear)
            {
                var peerCtx = _updatesContextFactory.GetUpdatesContext(null, peerDto.PeerId);
                var peerMessages =
                    _unitOfWork.MessageRepository.GetMessages(peerDto.PeerId, new PeerDTO(PeerType.User, auth.UserId));
                List<int> peerDeletedIds = new();
                foreach (var m in peerMessages)
                {
                    if (m.Id < maxId && (minDate > 0 && m.Date > minDate) &&
                        (maxDate > 0 && m.Date < maxDate))
                    {
                        peerDeletedIds.Add(m.Id);
                        _unitOfWork.MessageRepository.DeleteMessage(peerDto.PeerId, m.Id);
                    }
                }
                int peerPts = await userCtx.IncrementPts();
                var peerDeleteMessages = new UpdateDeleteMessagesDTO(peerDeletedIds, peerPts, 1);
                _updates.EnqueueUpdate(peerDto.PeerId, peerDeleteMessages);
            }

            return new ServiceResult<AffectedHistoryDTO>(new AffectedHistoryDTO(pts, 
                    1, 0), true,
                ErrorMessages.None);
        }

        throw new NotSupportedException();
    }

    public async Task<ServiceResult<AffectedMessagesDTO>> DeleteMessages(long authKeyId, ICollection<int> id, bool revoke = false)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var userCtx = _updatesContextFactory.GetUpdatesContext(authKeyId, auth.UserId);
        foreach (var m in id)
        {
            _unitOfWork.MessageRepository.DeleteMessage(auth.UserId, m);
        }
        int pts = await userCtx.IncrementPts();
        var deleteMessages = new UpdateDeleteMessagesDTO(id.ToList(), pts, 1);
        _updates.EnqueueUpdate(auth.UserId, deleteMessages);
        
        return new ServiceResult<AffectedMessagesDTO>(new AffectedMessagesDTO(pts, 1), true,
            ErrorMessages.None);
    }

    public async Task<ServiceResult<DialogsDTO>> GetDialogs(long authKeyId, int offsetDate, int offsetId, 
        InputPeerDTO offsetPeer, int limit, long hash, bool? excludePinned = null,
        int? folderId = null)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var userCtx = _updatesContextFactory.GetUpdatesContext(authKeyId, auth.UserId);
        var messages = await _unitOfWork.MessageRepository.GetMessagesAsync(auth.UserId);
        List<DialogDTO> userDialogs = new();
        Dictionary<long, UserDTO> userList = new();
        Dictionary<long, PeerDTO> peerList = new();
        Dictionary<long, int> topMessages = new();
        //TODO: investigate if we should use this PTS or a dialog specific one
        int pts = await userCtx.Pts();
        int unread = await userCtx.UnreadMessages();
        foreach (var m in messages)
        {
            if (m.Out)
            {
                if (m.PeerId.PeerType == PeerType.User)
                {
                    long userId = m.PeerId.PeerId;
                    await PopulateLists(userList, userId, m, peerList, topMessages);
                }
            }
            else
            {
                if (m.FromId.PeerType == PeerType.User)
                {
                    long userId = m.FromId.PeerId;
                    await PopulateLists(userList, userId, m, peerList, topMessages);
                }
            }
        }
        
        foreach (var p in peerList.Values)
        {
            if (p.PeerType == PeerType.User)
            {
                var peerContext = _updatesContextFactory.GetUpdatesContext(null, p.PeerId);
                int unreadFromPeer = await peerContext.UnreadMessages(p);
                int incomingReadMax = await userCtx.ReadMessagesMaxId(p);
                int outgoingReadMax = await peerContext.ReadMessagesMaxId(new PeerDTO(PeerType.User, auth.UserId));
                InputNotifyPeerDTO peer = new InputNotifyPeerDTO()
                {
                    NotifyPeerType = InputNotifyPeerType.Peer,
                    Peer = new InputPeerDTO{InputPeerType = InputPeerType.User, UserId = p.PeerId}
                };
                var settings = (await _store.GetNotifySettingsAsync(authKeyId, peer)).FirstOrDefault();
                var dialog = new DialogDTO
                {
                    DialogType = DialogType.Dialog,
                    Peer = p,
                    Pts = pts,
                    TopMessage = topMessages[p.PeerId],
                    UnreadCount = unreadFromPeer,
                    ReadInboxMaxId = incomingReadMax,
                    ReadOutboxMaxId = outgoingReadMax,
                    NotifySettings = settings ?? new PeerNotifySettingsDTO()
                };
                userDialogs.Add(dialog);
            }
            
        }
        var dialogs = new DialogsDTO(DialogsType.Dialogs, userDialogs, 
            messages, Array.Empty<ChatDTO>(),
            userList.Values, null);
        return new ServiceResult<DialogsDTO>(dialogs, true, ErrorMessages.None);
    }

    private async Task PopulateLists(Dictionary<long, UserDTO> userList, long userId, MessageDTO m, Dictionary<long, PeerDTO> peerList,
        Dictionary<long, int> topMessages)
    {
        if (!userList.ContainsKey(userId))
        {
            userList.Add(userId, await _store.GetUserAsync(m.PeerId.PeerId));
        }

        if (!peerList.ContainsKey(userId))
        {
            peerList.Add(userId, m.PeerId);
        }

        if (!topMessages.ContainsKey(userId))
        {
            topMessages.Add(userId, m.Id);
        }
        else if (topMessages[userId] < m.Id)
        {
            topMessages[userId] = m.Id;
        }
    }

    public async Task<ServiceResult<MessagesDTO>> GetHistory(long authKeyId, InputPeerDTO peer, int offsetId, 
        int offsetDate, int addOffset, int limit, long maxId,
        long minId, long hash)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var messages = await _unitOfWork.MessageRepository.GetMessagesAsync(auth.UserId, 
            PeerFromInputPeer(peer, peer.InputPeerType == InputPeerType.Self ? auth.UserId : 0));
        List<MessageDTO> messagesList = new();
        Dictionary<string, UserDTO> userList = new();
        foreach (var m in messages)
        {
            if (m.Id > offsetId && m.Date < offsetDate &&
                --addOffset < 0 && messagesList.Count < limit &&
                (maxId > 0 && m.Id <= maxId) && (minId > 0 || m.Id >= minId))
            {
                messagesList.Add(m);
                if (!m.Out && m.FromId.PeerType == PeerType.User)
                {
                    userList.Add(m.FromId.PeerId.ToString(), await _store.GetUserAsync(m.FromId.PeerId));
                }
            }
        }

        var messagesResult = new MessagesDTO(MessagesType.Messages,
            messagesList, Array.Empty<ChatDTO>(), userList.Values);
        return new ServiceResult<MessagesDTO>(messagesResult, true, ErrorMessages.None);
    }

    public async Task<ServiceResult<MessagesDTO>> Search(long authKeyId, InputPeerDTO peer, string q, 
        InputPeerDTO? fromId, int? topMessageId, MessagesFilterType filter, int minDate, int maxDate, 
        int offsetId, int addOffset, int limit, long maxId, long minId, long hash)
    {
        //TODO: debug and fix this
        var searchResults =  new List<MessageSearchModel>();//await _search.SearchMessages(q);
        //TODO: implement a proper search with pagination
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
            return new ServiceResult<MessagesDTO>(null, false, ErrorMessages.InvalidAuthKey);
        List<MessageDTO> messages = new List<MessageDTO>();
        List<UserDTO> users = new List<UserDTO>();
        foreach (var r in searchResults)
        {
            var message = await _unitOfWork.MessageRepository.GetMessageAsync(r.UserId, r.MessageId);
            messages.Add(message);
            if (message.Out && message.PeerId.PeerType == PeerType.User)
            {
                var user = await _store.GetUserAsync(message.PeerId.PeerId);
                users.Add(user);
            }
        }

        return new ServiceResult<MessagesDTO>(new MessagesDTO(MessagesType.Messages, messages, 
            Array.Empty<ChatDTO>(), users), true, ErrorMessages.None);
    }

    public async Task<ServiceResult<bool>> SetTyping(long authKeyId, InputPeerDTO peer, SendMessageActionDTO action, int? topMessageId = null)
    {
        var peerDTO = PeerFromInputPeer(peer);
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (peerDTO.PeerType == PeerType.User)
        {
            var update = new UpdateUserTypingDTO(auth.UserId, action);
            _updates.EnqueueUpdate(peerDTO.PeerId, update);
            return new ServiceResult<bool>(true, true, ErrorMessages.None);
        }
        throw new NotImplementedException();
    }

    private PeerDTO PeerFromInputPeer(InputPeerDTO peer, long userId = 0)
    {
        if (peer.InputPeerType == InputPeerType.Self)
        {
            return new PeerDTO(PeerType.User, userId);
        }
        else if(peer.InputPeerType == InputPeerType.User)
        {
            return new PeerDTO(PeerType.User, peer.UserId);
        }
        else if(peer.InputPeerType == InputPeerType.Chat)
        {
            return new PeerDTO(PeerType.Chat, peer.ChatId);
        }
        else if(peer.InputPeerType == InputPeerType.Channel)
        {
            return new PeerDTO(PeerType.Channel, peer.ChannelId);
        }
        else if (peer.InputPeerType == InputPeerType.UserFromMessage)
        {
            return new PeerDTO(PeerType.User, peer.UserId);
        }
        {
            return new PeerDTO(PeerType.Channel, peer.ChannelId);
        }
    }
}