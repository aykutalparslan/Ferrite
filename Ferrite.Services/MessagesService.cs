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
using Ferrite.Data.Repositories;
using PeerSettings = Ferrite.Data.Messages.PeerSettings;

namespace Ferrite.Services;

public class MessagesService : IMessagesService
{
    private readonly IPersistentStore _store;
    private readonly IDistributedCache _cache;
    private readonly IUnitOfWork _unitOfWork;
    public MessagesService(IPersistentStore store)
    {
        _store = store;
    }
    public async Task<ServiceResult<Data.Messages.PeerSettings>> GetPeerSettings(long authKeyId, InputPeer peer)
    {
        if (peer.InputPeerType == InputPeerType.Self)
        {
            var settings = new Data.PeerSettings(false, false, false, 
                false, false, false, 
                false, false, false, 
                null, null, null);
            return new ServiceResult<PeerSettings>(new PeerSettings(settings, new List<Chat>(), new List<User>())
                , true, ErrorMessages.None);
        }
        else if (peer.InputPeerType == InputPeerType.User)
        {
            var settings = new Data.PeerSettings(true, true, true, 
                false, false,  false, 
                false, false, false, 
                null, null, null);
            var users = new List<User>();
            var user = await _store.GetUserAsync(peer.UserId);
            users.Add(user);
            return new ServiceResult<PeerSettings>(new PeerSettings(settings, new List<Chat>(), users)
                , true, ErrorMessages.None);
        }
        return new ServiceResult<PeerSettings>(null, false, ErrorMessages.PeerIdInvalid);
    }

    public async Task<ServiceResult<UpdateBase>> SendMessage(long authKeyId, bool noWebpage, bool silent, bool background, bool clearDraft, bool noForwards,
        InputPeer peer, string message, long randomId, int? replyToMsgId, ReplyMarkup? replyMarkup,
        IReadOnlyCollection<MessageEntity>? entities, int? scheduleDate, InputPeer? sendAs)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        var messageCounter = _cache.GetCounter(auth.UserId + "_in");
        int messageId = (int)await messageCounter.IncrementAndGet();
        if (messageId == 0)
        {
            await messageCounter.IncrementAndGet();
        }
        var from = new Peer(PeerType.User, auth.UserId);
        var to = PeerFromInputPeer(peer);
        var outgoingMessage = new Message()
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
            outgoingMessage.ReplyTo = new MessageReplyHeader((int)replyToMsgId, null, null);
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
        return new ServiceResult<UpdateBase>(new UpdateMessageId(messageId, randomId), 
            true, ErrorMessages.None);
    }

    private Peer PeerFromInputPeer(InputPeer peer, long userId = 0)
    {
        if (peer.InputPeerType == InputPeerType.Self)
        {
            return new Peer(PeerType.User, userId);
        }
        else if(peer.InputPeerType == InputPeerType.User)
        {
            return new Peer(PeerType.User, peer.UserId);
        }
        else if(peer.InputPeerType == InputPeerType.Chat)
        {
            return new Peer(PeerType.Chat, peer.ChatId);
        }
        else if(peer.InputPeerType == InputPeerType.Channel)
        {
            return new Peer(PeerType.Channel, peer.ChannelId);
        }
        else if (peer.InputPeerType == InputPeerType.UserFromMessage)
        {
            return new Peer(PeerType.User, peer.UserId);
        }
        {
            return new Peer(PeerType.Channel, peer.ChannelId);
        }
    }
}