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
using PeerSettingsDTO = Ferrite.Data.Messages.PeerSettingsDTO;

namespace Ferrite.Services;

public class MessagesService : IMessagesService
{
    private readonly IPersistentStore _store;
    private readonly IDistributedCache _cache;
    private readonly IUnitOfWork _unitOfWork;
    public MessagesService(IPersistentStore store, IDistributedCache cache, IUnitOfWork unitOfWork)
    {
        _store = store;
        _cache = cache;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<MessagesDTO>> GetMessagesAsync(long authKeyId, IReadOnlyCollection<InputMessageDTO> id)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
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
            }
        }

        return new ServiceResult<MessagesDTO>(new MessagesDTO(MessagesType.Messages, messages, 
            Array.Empty<ChatDTO>(), users), true, ErrorMessages.None);
    }

    public async Task<ServiceResult<Data.Messages.PeerSettingsDTO>> GetPeerSettings(long authKeyId, InputPeerDTO peer)
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
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        var messageCounter = _cache.GetCounter(auth.UserId + "_message_id");
        int messageId = (int)await messageCounter.IncrementAndGet();
        if (messageId == 0)
        {
            await messageCounter.IncrementAndGet();
        }
        var from = new PeerDTO(PeerType.User, auth.UserId);
        var to = PeerFromInputPeer(peer);
        var outgoingMessage = new MessageDTO()
        {
            Id = messageId,
            Out = true,
            Silent = silent,
            FromId = from,
            PeerId = to,
            MessageText = message,
            ReplyMarkup = replyMarkup,
            Entities = entities,
        };
        if (replyToMsgId != null)
        {
            outgoingMessage.ReplyTo = new MessageReplyHeaderDTO((int)replyToMsgId, null, null);
        }
        var incomingMessage = outgoingMessage with
        {
            Out = false,
            FromId = to,
            PeerId = from,
        };
        _unitOfWork.MessageRepository.PutMessage(outgoingMessage);
        _unitOfWork.MessageRepository.PutMessage(incomingMessage);
        await _unitOfWork.SaveAsync();
        var userPts = _cache.GetCounter(auth.UserId + "_pts");
        var pts = await userPts.IncrementAndGet();
        if (pts == 0)
        {
            await userPts.IncrementAndGet();
        }
        return new ServiceResult<UpdateShortSentMessageDTO>(new UpdateShortSentMessageDTO(true, messageId,
                (int)pts, 1, (int)DateTimeOffset.Now.ToUnixTimeSeconds(), null, null, null), 
            true, ErrorMessages.None);
    }

    public async Task<ServiceResult<AffectedMessagesDTO>> ReadHistory(long authKeyId, InputPeerDTO peer, int maxId)
    {
        if (peer.InputPeerType == InputPeerType.Self)
        {
            var auth = await _store.GetAuthorizationAsync(authKeyId);
            var counter = _cache.GetCounter(auth.UserId + "_pts");
            return new ServiceResult<AffectedMessagesDTO>(
                new AffectedMessagesDTO((int)await counter.Get(), 0), true, ErrorMessages.None);
        }
        else if(peer.InputPeerType == InputPeerType.User)
        {
            var counter = _cache.GetCounter(peer.UserId + "_pts");
            return new ServiceResult<AffectedMessagesDTO>(
                new AffectedMessagesDTO((int)await counter.Get(), 0), true, ErrorMessages.None);
        }
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<DialogsDTO>> GetDialogs(long authKeyId, int offsetDate, int offsetId, 
        InputPeerDTO offsetPeer, int limit, long hash, bool? excludePinned = null,
        int? folderId = null)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        var messages = await _unitOfWork.MessageRepository.GetMessagesAsync(auth.UserId);
        Dictionary<string, DialogDTO> dialogList = new();
        Dictionary<string, UserDTO> userList = new();
        foreach (var m in messages)
        {
            if (m.Out)
            {
                InputNotifyPeerDTO peer = new InputNotifyPeerDTO()
                {
                    NotifyPeerType = InputNotifyPeerType.Peer,
                    Peer = offsetPeer
                };
                //TODO: investigate if we should use this PTS or a dialog specific one
                var counter = _cache.GetCounter(auth.UserId + "_pts");
                var settings = await _store.GetNotifySettingsAsync(authKeyId, peer);
                dialogList.Add(m.PeerId.PeerType + "-" + m.PeerId.PeerId,
                    new DialogDTO(DialogType.Dialog, false, false,
                        m.PeerId, m.Id, 0, m.Id,
                        0, 0, 0, settings.FirstOrDefault(),
                        0, null, null, null, 0, 0,
                        0, 0));
                if (m.PeerId.PeerType == PeerType.User)
                {
                    userList.Add(m.PeerId.PeerId.ToString(), await _store.GetUserAsync(m.PeerId.PeerId));
                }
            }
            else
            {
                InputNotifyPeerDTO peer = new InputNotifyPeerDTO()
                {
                    NotifyPeerType = InputNotifyPeerType.Peer,
                    Peer = offsetPeer
                };
                //TODO: investigate if we should use this PTS or a dialog specific one
                var counter = _cache.GetCounter(auth.UserId + "_pts");
                var settings = await _store.GetNotifySettingsAsync(authKeyId, peer);
                dialogList.Add(m.FromId.PeerType + "-" + m.FromId.PeerId,
                    new DialogDTO(DialogType.Dialog, false, false,
                        m.FromId, m.Id, 0, m.Id,
                        0, 0, 0, settings.FirstOrDefault(),
                        0, null, null, null, 0, 0,
                        0, 0));
                if (m.FromId.PeerType == PeerType.User)
                {
                    userList.Add(m.FromId.PeerId.ToString(), await _store.GetUserAsync(m.FromId.PeerId));
                }
            }
        }

        var dialogs = new DialogsDTO(DialogsType.Dialogs, dialogList.Values, 
            messages, Array.Empty<ChatDTO>(),
            userList.Values, null);
        return new ServiceResult<DialogsDTO>(dialogs, true, ErrorMessages.None);
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