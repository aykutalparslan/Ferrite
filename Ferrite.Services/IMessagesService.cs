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

namespace Ferrite.Services;

public interface IMessagesService
{
    Task<ServiceResult<MessagesDTO>> GetMessagesAsync(long authKeyId, IReadOnlyCollection<InputMessageDTO> id);
    Task<ServiceResult<Data.Messages.PeerSettingsDTO>> GetPeerSettings(long authKeyId, InputPeerDTO peer);
    Task<ServiceResult<UpdateShortSentMessageDTO>> SendMessage(long authKeyId, bool noWebpage, bool silent,
        bool background, bool clearDraft, bool noForwards, InputPeerDTO peer, string message, long randomId,
        int? replyToMsgId, ReplyMarkupDTO? replyMarkup, IReadOnlyCollection<MessageEntityDTO> ? entities,
        int? scheduleDate, InputPeerDTO? sendAs);
    Task<ServiceResult<UpdateShortSentMessageDTO>> SendMedia(long authKeyId, bool silent, bool background, 
        bool clearDraft, bool noForwards, InputPeerDTO peer, int? replyToMsgId, InputMediaDTO media, 
        string message, long randomId, ReplyMarkupDTO? replyMarkup, IReadOnlyCollection<MessageEntityDTO>? entities,
        int? scheduleDate, InputPeerDTO? sendAs);
    Task<ServiceResult<AffectedMessagesDTO>> ReadHistory(long authKeyId, InputPeerDTO peer, int maxId);
    Task<ServiceResult<AffectedHistoryDTO>> DeleteHistory(long authKeyId, InputPeerDTO peer, int maxId,
        int? minDate = null, int? maxDate = null, bool justClear = false, bool revoke = false);
    Task<ServiceResult<AffectedMessagesDTO>> DeleteMessages(long authKeyId, ICollection<int> id, bool revoke = false);
    Task<ServiceResult<DialogsDTO>> GetDialogs(long authKeyId, int offsetDate, int offsetId, InputPeerDTO offsetPeer,
        int limit, long hash, bool? excludePinned = null, int? folderId = null);
    Task<ServiceResult<PeerDialogsDTO>> GetPeerDialogs(long authKeyId, IEnumerable<InputDialogPeerDTO> peers);
    Task<ServiceResult<MessagesDTO>> GetHistory(long authKeyId, InputPeerDTO peer, int offsetId, int offsetDate,
        int addOffset, int limit, long maxId, long minId, long hash);
    Task<ServiceResult<MessagesDTO>> Search(long authKeyId, InputPeerDTO peer, string q, InputPeerDTO? fromId,
        int? topMessageId, MessagesFilterType filter, int minDate, int maxDate, int offsetId, int addOffset, 
        int limit, long maxId, long minId, long hash);

    Task<ServiceResult<bool>> SetTyping(long authKeyId, InputPeerDTO peer,
        SendMessageActionDTO action, int? topMessageId = null);
}