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

using MessagePack;

namespace Ferrite.Data;

[MessagePackObject(true)]
public record Message
{
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
    public Peer FromId { get; set; }
    public Peer PeerId { get; set; }
    public MessageFwdHeader FwdFrom { get; set; }
    public long ViaBotId { get; set; }
    public MessageReplyHeader ReplyTo { get; set; }
    public int Date { get; set; }
    public string MessageText { get; set; }
    public MessageMedia? Media { get; set; }
    public ReplyMarkup? ReplyMarkup { get; set; }
    public IReadOnlyCollection<MessageEntity>? Entities { get; set; }
    public int Views { get; set; }
    public int Forwards { get; set; }
    public MessageReplies? Replies { get; set; }
    public int? EditDate { get; set; }
    public string? PostAuthor { get; set; }
    public long? GroupedId { get; set; }
    public MessageReactions? Reactions { get; set; }
    public IReadOnlyCollection<RestrictionReason>? RestrictionReason { get; set; }
    public int? TtlPeriod { get; set; }
}