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

using Ferrite.Data.Messages;
using MessagePack;

namespace Ferrite.Data;

[MessagePackObject(true)]
public record MessageDTO
{
    public MessageType MessageType { get; set; } = MessageType.Message;
    public bool Out { get; set; }
    public bool Mentioned { get; set; }
    public bool MediaUnread { get; set; }
    public bool Silent { get; set; }
    public bool Post { get; set; }
    public bool FromScheduled { get; set; }
    public bool Legacy { get; set; }
    public bool EditHide { get; set; }
    public bool Pinned { get; set; }
    public bool NoForwards { get; set; }
    public int Id { get; set; }
    public PeerDTO? FromId { get; set; }
    public PeerDTO PeerId { get; set; }
    public MessageFwdHeaderDTO? FwdFrom { get; set; }
    public long? ViaBotId { get; set; }
    public MessageReplyHeaderDTO? ReplyTo { get; set; }
    public int Date { get; set; }
    public string MessageText { get; set; }
    public MessageMediaDTO? Media { get; set; }
    public ReplyMarkupDTO? ReplyMarkup { get; set; }
    public IReadOnlyCollection<MessageEntityDTO>? Entities { get; set; }
    public int? Views { get; set; }
    public int? Forwards { get; set; }
    public MessageRepliesDTO? Replies { get; set; }
    public int? EditDate { get; set; }
    public string? PostAuthor { get; set; }
    public long? GroupedId { get; set; }
    public MessageReactionsDTO? Reactions { get; set; }
    public IReadOnlyCollection<RestrictionReasonDTO>? RestrictionReason { get; set; }
    public int? TtlPeriod { get; set; }
}