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

using System.Text;
using Ferrite.Data;
using Ferrite.Data.Repositories;
using Ferrite.Data.Search;
using Ferrite.TL.slim;
using Ferrite.TL.slim.baseLayer;
using Ferrite.TL.slim.baseLayer.dto;
using Ferrite.TL.slim.baseLayer.messages;
using Ferrite.Utils;

namespace Ferrite.Services;

public class MessagesService : IMessagesService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISearchEngine _search;
    private readonly IUpdatesService _updates;
    private readonly IUpdatesContextFactory _updatesContextFactory;
    private readonly ILogger _log;
    private readonly IUploadService _upload;
    private readonly IPhotosService _photos;

    public MessagesService(IUnitOfWork unitOfWork,ISearchEngine search, 
        IUpdatesService updates, IUpdatesContextFactory updatesContextFactory,
        ILogger log, IUploadService upload, IPhotosService photos)
    {
        _unitOfWork = unitOfWork;
        _search = search;
        _updates = updates;
        _updatesContextFactory = updatesContextFactory;
        _log = log;
        _upload = upload;
        _photos = photos;
    }
/*
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
                    var user = _unitOfWork.UserRepository.GetUser(message.PeerId.PeerId);
                    users.Add(user);
                }
                else if (!message.Out && message.FromId.PeerType == PeerType.User)
                {
                    var user = _unitOfWork.UserRepository.GetUser(message.FromId.PeerId);
                    users.Add(user);
                }
            }
        }

        return new ServiceResult<MessagesDTO>(new MessagesDTO(MessagesType.Messages, messages, 
            Array.Empty<ChatDTO>(), users), true, ErrorMessages.None);
        throw new NotImplementedException();
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
            var user = _unitOfWork.UserRepository.GetUser(peer.UserId);
            users.Add(user);
            return new ServiceResult<PeerSettingsDTO>(new PeerSettingsDTO(settings, new List<ChatDTO>(), users)
                , true, ErrorMessages.None);
        }
        return new ServiceResult<PeerSettingsDTO>(null, false, ErrorMessages.PeerIdInvalid);
        throw new NotImplementedException();
    }
*/
    public async Task<TLUpdates> SendMessage(long authKeyId, TLBytes q)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var userId = auth.Value.AsAuthInfo().UserId;
        var senderCtx = _updatesContextFactory.GetUpdatesContext(authKeyId, userId);
        int senderMessageId = (int)await senderCtx.NextMessageId();
        using TLPeer from = new PeerUser( userId);
        using var to = PeerFromInputPeer(((SendMessage)q).Get_Peer());
        var outgoingMessage = GenerateOutgoingMessage(q, senderMessageId, from, to);

        var pts = await SaveMessage(senderCtx, auth.Value, outgoingMessage, from, to);
        
        if (GetPeerId(to) != GetPeerId(from))
        {
            await SaveIncomingMessage(to, outgoingMessage, from);
        }
        
        await _unitOfWork.SaveAsync();

        return UpdateShortSentMessage.Builder()
            .OutProperty(true)
            .Id(senderMessageId)
            .Pts(pts)
            .PtsCount(1)
            .Date((int)DateTimeOffset.Now.ToUnixTimeSeconds())
            .Build();
    }
    private static TLMessage GenerateOutgoingMessage(in TLBytes sendMessage, int senderMessageId, 
        in TLPeer from, in TLPeer to)
    {
        var builder = Message.Builder()
            .Id(senderMessageId)
            .OutProperty(true)
            .Silent(((SendMessage)sendMessage).Silent)
            .FromId(from.AsSpan())
            .PeerId(to.AsSpan())
            .MessageProperty(((SendMessage)sendMessage).Message)
            .Date((int)DateTimeOffset.Now.ToUnixTimeSeconds());

        var sendMessageFlags = ((SendMessage)sendMessage).Flags;
        
        if (!sendMessageFlags[0]) return builder.Build();
        using var replyToHeader = MessageReplyHeader.Builder()
            .ReplyToMsgId(((SendMessage)sendMessage).ReplyToMsgId).Build();
        builder = builder.ReplyTo(replyToHeader.ToReadOnlySpan());
        
        if (!sendMessageFlags[2]) return builder.Build();
        builder = builder.ReplyMarkup(((SendMessage)sendMessage).ReplyMarkup);
        
        if (!sendMessageFlags[3]) return builder.Build();
        builder = builder.Entities(((SendMessage)sendMessage).Entities);

        return builder.Build();
    }
    private async Task SaveIncomingMessage(TLPeer to, TLMessage outgoingMessage, TLPeer from)
    {
        var receiverCtx = _updatesContextFactory.GetUpdatesContext(null, GetPeerId(to));
        int receiverMessageId = await receiverCtx.NextMessageId();
        using TLMessage incomingMessage = outgoingMessage.AsMessage().Clone()
            .Id(receiverMessageId)
            .OutProperty(false)
            .PeerId(from.AsSpan())
            .Build();
        
        int ptsPeer = await receiverCtx.IncrementPtsForMessage((int)from.Type, GetPeerId(from), receiverMessageId);
        _unitOfWork.MessageRepository.PutMessage(GetPeerId(to), incomingMessage, ptsPeer);
        TLUpdate updateNewMessage = UpdateNewMessage.Builder()
            .Message(incomingMessage.AsSpan())
            .Pts(ptsPeer)
            .PtsCount(1)
            .Build();
        
        await _updates.EnqueueUpdate(GetPeerId(to), updateNewMessage);
        var searchModelIncoming = new MessageSearchModel(
            GetPeerId(to) + "_" + incomingMessage.AsMessage().Id,
            GetPeerId(to), (int)to.Type, GetPeerId(to),
            (int)from.Type, GetPeerId(from), incomingMessage.AsMessage().Id,
            null,  Encoding.UTF8.GetString(incomingMessage.AsMessage().MessageProperty),
            incomingMessage.AsMessage().Date);
        await _search.IndexMessage(searchModelIncoming);
    }
    private async Task<int> SaveMessage(IUpdatesContext senderCtx, 
        TLAuthInfo auth, TLMessage outgoingMessage, TLPeer from, TLPeer to)
    {
        int previousPts = await senderCtx.Pts();
        int pts = await senderCtx.IncrementPts();
        _unitOfWork.MessageRepository.PutMessage(auth.AsAuthInfo().UserId,
            outgoingMessage, pts);
        _log.Debug($"💬 Message was sent Sender: {auth.AsAuthInfo().UserId} Previous PTS: {previousPts} PTS: {pts}");
        var searchModelOutgoing = new MessageSearchModel(
            GetPeerId(from) + "_" + outgoingMessage.AsMessage().Id,
            GetPeerId(from), (int)from.Type, GetPeerId(from),
            (int)to.Type, GetPeerId(to), outgoingMessage.AsMessage().Id,
            null, Encoding.UTF8.GetString(outgoingMessage.AsMessage().MessageProperty),
            outgoingMessage.AsMessage().Date);
        await _search.IndexMessage(searchModelOutgoing);
        return pts;
    }
/*
    public async Task<ServiceResult<UpdateShortSentMessageDTO>> SendMedia(long authKeyId, bool silent, 
        bool background, bool clearDraft, bool noForwards, InputPeerDTO peer,
        int? replyToMsgId, InputMediaDTO media, string message, long randomId, ReplyMarkupDTO? replyMarkup,
        IReadOnlyCollection<MessageEntityDTO>? entities, int? scheduleDate, InputPeerDTO? sendAs)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var senderCtx = _updatesContextFactory.GetUpdatesContext(authKeyId, auth.UserId);
        int senderMessageId = (int)await senderCtx.NextMessageId();
        var from = new PeerDTO(PeerType.User, auth.UserId);
        var to = PeerFromInputPeer(peer);
        MessageDTO outgoingMessage = 
            GenerateOutgoingMessage(silent, message, replyToMsgId, replyMarkup, entities, senderMessageId, from, to);
        if (media.InputMediaType == InputMediaType.Empty)
        {
            outgoingMessage.Media = new MessageMediaDTO(MessageMediaType.Empty,
                null, null, null, null,
                null, null, null, null, null, null,
                null, null, null, null, null, null,
                false, false, null, null, null, null,
                null, null, null, null, null, null,
                null, null, null);
        }
        else if (media.InputMediaType == InputMediaType.UploadedPhoto)
        {
            var saveResult = await _upload.SaveFile(media.File!);
            if (!saveResult.Success)
            {
                return new ServiceResult<UpdateShortSentMessageDTO>(null, false, saveResult.ErrorMessage);
            }
            var photoResult = await _photos.ProcessPhoto(saveResult.Result!, DateTime.Now);
            if (!photoResult.Success)
            {
                return new ServiceResult<UpdateShortSentMessageDTO>(null, false, photoResult.ErrorMessage);
            }

            SetMediaPhoto(outgoingMessage, photoResult.Result!);
        }
        else if (media.InputMediaType == InputMediaType.Photo)
        {
            var photo = await _photos.GetPhoto(authKeyId, media.Photo!);
            SetMediaPhoto(outgoingMessage, photo);
        }
        else if (media.InputMediaType == InputMediaType.PhotoExternal)
        {
            // download and save photo?
        }
        else if (media.InputMediaType == InputMediaType.GeoPoint)
        {
            var location = new GeoPointDTO(false, media.GeoPoint.Latitude,
                media.GeoPoint.Longitude, Random.Shared.NextInt64(), 
                media.GeoPoint.AccuracyRadius);
            
            outgoingMessage.Media = new MessageMediaDTO(MessageMediaType.Geo,
                null, null, location, null,
                null, null, null, null, null, null,
                null, null, null, null, null, null,
                false, false, null, null, null, null,
                null, null, null, null, null, null,
                null, null, null);
        }
        else if (media.InputMediaType == InputMediaType.GeoLive)
        {
            var location = new GeoPointDTO(false, media.GeoPoint.Latitude,
                media.GeoPoint.Longitude, Random.Shared.NextInt64(), 
                media.GeoPoint.AccuracyRadius);
            
            outgoingMessage.Media = new MessageMediaDTO(MessageMediaType.GeoLive,
                null, null, location, null,
                null, null, null, null, null, null,
                null, null, null, null, null, null,
                false, false, null, null, null, null,
                null, null, media.Heading, media.Period, media.ProximityNotificationRadius, null,
                null, null, null);
        }
        else if (media.InputMediaType == InputMediaType.Contact &&
                 _unitOfWork.UserRepository.GetUserId(media.PhoneNumber!) is { } contactUserId)
        {
            outgoingMessage.Media = new MessageMediaDTO(MessageMediaType.Contact,
                null, null, null, 
                media.PhoneNumber, media.FirstName, media.LastName, media.VCard, contactUserId, 
                null, null, null, null, null, null, null, null,
                false, false, null, null, null, null,
                null, null, null, null, null, null,
                null, null, null);
        }
        else if(media.InputMediaType == InputMediaType.UploadedDocument)
        {
            var saveResult = await _upload.SaveFile(media.File!);
            if (media.Thumb != null)
            {
                var thumbSaveResult = await _upload.SaveFile(media.Thumb!);
            }
        }
        else if(media.InputMediaType == InputMediaType.Document)
        {
            var saveResult = await _upload.SaveFile(media.File!);
        }
        else if(media.InputMediaType == InputMediaType.DocumentExternal)
        {
            
        }
        
        var pts = await SaveMessage(senderCtx, auth, outgoingMessage, from, to);
        
        if (to.PeerId != from.PeerId)
        {
            await SaveIncomingMessage(to, outgoingMessage, from);
        }
        
        await _unitOfWork.SaveAsync();

        return new ServiceResult<UpdateShortSentMessageDTO>(new UpdateShortSentMessageDTO(true, senderMessageId,
                pts, 1, (int)DateTimeOffset.Now.ToUnixTimeSeconds(), null, null, null), 
            true, ErrorMessages.None);
        throw new NotImplementedException();
    }

    private static void SetMediaPhoto(MessageDTO outgoingMessage, PhotoDTO photo)
    {
        outgoingMessage.Media = new MessageMediaDTO(MessageMediaType.Photo,
            photo, null, null, null,
            null, null, null, null, null, null,
            null, null, null, null, null, null,
            false, false, null, null, null, null,
            null, null, null, null, null, null,
            null, null, null);
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
            int ptsCount = 0;
            if (auth.UserId != peerDto.PeerId)
            {
                ptsCount++;
                var updateInbox = new UpdateReadHistoryInboxDTO(peerDto, maxId, unread, userPts, 1);
                _ = _updates.EnqueueUpdate(auth.UserId, updateInbox);
                var updateOutbox = new UpdateReadHistoryOutboxDTO(new PeerDTO(PeerType.User, auth.UserId), maxId,
                    await peerCtx.Pts(), 1);
                _ = _updates.EnqueueUpdate(peerDto.PeerId, updateOutbox);
            }
            
            return new ServiceResult<AffectedMessagesDTO>(
                new AffectedMessagesDTO(userPts
                    , ptsCount), true, ErrorMessages.None);
        }

        throw new NotSupportedException();
        throw new NotImplementedException();
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
            var pts = await DeleteMessagesInternal(maxId, minDate, maxDate, userCtx, messages, auth);

            if (!justClear)
            {
                await DeletePeerMessagesInternal(maxId, minDate, maxDate, peerDto, auth, userCtx);
            }

            return new ServiceResult<AffectedHistoryDTO>(new AffectedHistoryDTO(pts, 
                    1, 0), true,
                ErrorMessages.None);
        }

        throw new NotSupportedException();
        throw new NotImplementedException();
    }
    

    
    private async Task DeletePeerMessagesInternal(int maxId, int? minDate, int? maxDate, PeerDTO peerDto, AuthInfoDTO auth,
        IUpdatesContext userCtx)
    {
        var peerCtx = _updatesContextFactory.GetUpdatesContext(null, peerDto.PeerId);
        var peerMessages =
            _unitOfWork.MessageRepository.GetMessages(peerDto.PeerId, new PeerDTO(PeerType.User, auth.UserId));
        List<int> peerDeletedIds = new();
        if (peerMessages != null)
        {
            foreach (var m in peerMessages)
            {
                if (m.Id < maxId && (minDate == null || m.Date > minDate) &&
                    (maxDate == null || m.Date < maxDate))
                {
                    peerDeletedIds.Add(m.Id);
                    _unitOfWork.MessageRepository.DeleteMessage(peerDto.PeerId, m.Id);
                }
            }

            int peerPts = await userCtx.IncrementPts();
            var peerDeleteMessages = new UpdateDeleteMessagesDTO(peerDeletedIds, peerPts, 1);
            _updates.EnqueueUpdate(peerDto.PeerId, peerDeleteMessages);
        }
        throw new NotImplementedException();
    }

    private async Task<int> DeleteMessagesInternal(int maxId, int? minDate, int? maxDate, IUpdatesContext userCtx,
        IReadOnlyCollection<MessageDTO> messages, AuthInfoDTO auth)
    {
        List<int> deletedIds = new();
        int pts = await userCtx.Pts();
        if (messages != null)
        {
            foreach (var m in messages)
            {
                if (m.Id < maxId && (minDate == null || m.Date > minDate) &&
                    (maxDate == null || m.Date < maxDate))
                {
                    deletedIds.Add(m.Id);
                    _unitOfWork.MessageRepository.DeleteMessage(auth.UserId, m.Id);
                }
            }

            await _unitOfWork.SaveAsync();
            pts = await userCtx.IncrementPts();
            var deleteMessages = new UpdateDeleteMessagesDTO(deletedIds, pts, 1);
            _updates.EnqueueUpdate(auth.UserId, deleteMessages);
        }

        return pts;
        throw new NotImplementedException();
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
        
        //TODO: add support for revoke
        
        return new ServiceResult<AffectedMessagesDTO>(new AffectedMessagesDTO(pts, 1), true,
            ErrorMessages.None);
        throw new NotImplementedException();
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
        await ProcessMessages(messages, auth, userList, peerList, topMessages);
        await GenerateDialogs(authKeyId, peerList, userCtx, auth, topMessages, userDialogs);
        
        var dialogs = new DialogsDTO(DialogsType.Dialogs, userDialogs, 
            messages, Array.Empty<ChatDTO>(),
            userList.Values, null);
        return new ServiceResult<DialogsDTO>(dialogs, true, ErrorMessages.None);
        throw new NotImplementedException();
    }

    private async Task GenerateDialogs(long authKeyId, Dictionary<long, PeerDTO> peerList, IUpdatesContext userCtx, AuthInfoDTO auth,
        Dictionary<long, int> topMessages, List<DialogDTO> userDialogs)
    {
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
                    Peer = new InputPeerDTO { InputPeerType = InputPeerType.User, UserId = p.PeerId }
                };
                var settings = (_unitOfWork.NotifySettingsRepository.GetNotifySettings(authKeyId, peer)).FirstOrDefault();
                var dialog = new DialogDTO
                {
                    DialogType = DialogType.Dialog,
                    Peer = p,
                    TopMessage = topMessages[p.PeerId],
                    UnreadCount = unreadFromPeer,
                    ReadInboxMaxId = incomingReadMax,
                    ReadOutboxMaxId = outgoingReadMax,
                    NotifySettings = settings ?? new PeerNotifySettingsDTO()
                };
                userDialogs.Add(dialog);
            }
        }
        throw new NotImplementedException();
    }

    private async Task ProcessMessages(IReadOnlyCollection<MessageDTO> messages, AuthInfoDTO auth, Dictionary<long, UserDTO> userList,
        Dictionary<long, PeerDTO> peerList, Dictionary<long, int> topMessages)
    {
        foreach (var m in messages)
        {
            if (m.Out)
            {
                if (m.PeerId.PeerType == PeerType.User &&
                    m.PeerId.PeerId != auth.UserId)
                {
                    long userId = m.PeerId.PeerId;
                    await PopulateLists(userList, userId, m, peerList, topMessages);
                }
            }
            else
            {
                if (m.FromId.PeerType == PeerType.User &&
                    m.FromId.PeerId != auth.UserId)
                {
                    long userId = m.FromId.PeerId;
                    await PopulateLists(userList, userId, m, peerList, topMessages);
                }
            }
        }
    }

    public async Task<ServiceResult<PeerDialogsDTO>> GetPeerDialogs(long authKeyId, IEnumerable<InputDialogPeerDTO> peers)
    {
        var auth = _unitOfWork.AuthorizationRepository.GetAuthorization(authKeyId);
        if (auth == null) return new ServiceResult<PeerDialogsDTO>(null, 
            false, ErrorMessages.InvalidAuthKey);
        var userCtx = _updatesContextFactory.GetUpdatesContext(authKeyId, auth.UserId);
        List<DialogDTO> userDialogs = new();
        Dictionary<long, UserDTO> userList = new();
        Dictionary<long, PeerDTO> peerList = new();
        Dictionary<long, int> topMessages = new();
        List<MessageDTO> messageList = new();
        foreach (var p in peers)
        {
            if(p.Peer == null) continue;
            PeerDTO peer = null;
            if (p.Peer.InputPeerType == InputPeerType.User)
            {
                peer = new PeerDTO(PeerType.User, p.Peer.UserId);
            }
            else if (p.Peer.InputPeerType == InputPeerType.Chat)
            {
                peer = new PeerDTO(PeerType.Chat, p.Peer.ChatId);
            }
            else if (p.Peer.InputPeerType == InputPeerType.Channel)
            {
                peer = new PeerDTO(PeerType.Channel, p.Peer.ChannelId);
            }
            else if (p.Peer.InputPeerType == InputPeerType.UserFromMessage)
            {
                peer = new PeerDTO(PeerType.User, p.Peer.UserId);
            }
            else if (p.Peer.InputPeerType == InputPeerType.ChannelFromMessage)
            {
                peer = new PeerDTO(PeerType.Channel, p.Peer.ChannelId);
            }
            var messages = await _unitOfWork.MessageRepository.GetMessagesAsync(auth.UserId, peer);
            messageList.AddRange(messages);
            await ProcessMessages(messages, auth, userList, peerList, topMessages);
        }
        await GenerateDialogs(authKeyId, peerList, userCtx, auth, topMessages, userDialogs);
        var dialogs = new PeerDialogsDTO(userDialogs, 
            messageList, Array.Empty<ChatDTO>(),
            userList.Values, await _updates.GetState(authKeyId));
        return new ServiceResult<PeerDialogsDTO>(dialogs, true, ErrorMessages.None);
        throw new NotImplementedException();
    }

    private async Task PopulateLists(Dictionary<long, UserDTO> userList, long userId, MessageDTO m, Dictionary<long, PeerDTO> peerList,
        Dictionary<long, int> topMessages)
    {
        if (!userList.ContainsKey(userId))
        {
            userList.Add(userId, _unitOfWork.UserRepository.GetUser(m.PeerId.PeerId));
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
        throw new NotImplementedException();
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
            if (m.Id > offsetId && (offsetDate <= 0 || m.Date < offsetDate) &&
                --addOffset < 0 && messagesList.Count < limit &&
                (maxId <= 0 || m.Id <= maxId) && (minId <= 0 || m.Id >= minId))
            {
                messagesList.Add(m);
                if (!m.Out && m.FromId.PeerType == PeerType.User &&
                    !userList.ContainsKey(m.FromId.PeerId.ToString()))
                {
                    userList.Add(m.FromId.PeerId.ToString(), _unitOfWork.UserRepository.GetUser(m.FromId.PeerId));
                }
            }
        }

        var messagesResult = new MessagesDTO(MessagesType.Messages,
            messagesList, Array.Empty<ChatDTO>(), userList.Values);
        return new ServiceResult<MessagesDTO>(messagesResult, true, ErrorMessages.None);
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<MessagesDTO>> Search(long authKeyId, InputPeerDTO peer, string q, 
        InputPeerDTO? fromId, int? topMessageId, MessagesFilterType filter, int minDate, int maxDate, 
        int offsetId, int addOffset, int limit, long maxId, long minId, long hash)
    {
        
        //TODO: debug and fix this
        var searchResults =  await _search.SearchMessages(q);
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
                var user = _unitOfWork.UserRepository.GetUser(message.PeerId.PeerId);
                users.Add(user);
            }
        }

        return new ServiceResult<MessagesDTO>(new MessagesDTO(MessagesType.Messages, messages, 
            Array.Empty<ChatDTO>(), users), true, ErrorMessages.None);
        throw new NotImplementedException();
    }
*/
    public async ValueTask<TLBool> SetTyping(long authKeyId, TLBytes q)
    {
        using var peer = ((SetTyping)q).Get_Peer();
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (peer.Type == TLInputPeer.InputPeerType.InputPeerUser)
        {
            using TLUpdate update = UpdateUserTyping.Builder()
                .UserId(auth.Value.AsAuthInfo().UserId)
                .Action(((SetTyping)q).Action).
                Build();
            _updates.EnqueueUpdate(peer.AsInputPeerUser().UserId, update);
            return new BoolTrue();
        }
        throw new NotImplementedException();
    }

    private TLPeer PeerFromInputPeer(TLInputPeer p)=> p.Type switch
    {
        TLInputPeer.InputPeerType.InputPeerChat => new PeerChat(p.AsInputPeerChat().ChatId),
        TLInputPeer.InputPeerType.InputPeerUser => new PeerUser(p.AsInputPeerUser().UserId),
        TLInputPeer.InputPeerType.InputPeerChannel => new PeerChannel(p.AsInputPeerChannel().ChannelId),
        TLInputPeer.InputPeerType.InputPeerUserFromMessage => new PeerUser(p.AsInputPeerUserFromMessage().UserId),
        TLInputPeer.InputPeerType.InputPeerChannelFromMessage => new PeerChannel(p.AsInputPeerChannelFromMessage().ChannelId),
        _ => new PeerUser(0)
    };
    
    private static long GetPeerId(TLPeer p) => p.Type switch
    {
        TLPeer.PeerType.PeerChat => p.AsPeerChat().ChatId,
        TLPeer.PeerType.PeerUser => p.AsPeerUser().UserId,
        TLPeer.PeerType.PeerChannel => p.AsPeerChannel().ChannelId,
        _ => 0
    };
}