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

[MessagePackObject(true)] public record Message(bool Out, bool Mentioned, bool MediaUnread,
    bool Silent, bool Post, bool FromScheduled, bool Legacy, bool EditHide, bool Pinned, bool NoForwards, int Id, 
    Peer FromId, Peer PeerId, MessageFwdHeader FwdFrom, long ViaBotId, MessageReplyHeader ReplyTo,
    int Date, string MessageText, MessageMedia? Media, ReplyMarkup? ReplyMarkup,
    IReadOnlyCollection<MessageEntity>? Entities, int Views, int Forwards, MessageReplies? Replies,
    int? EditDate, string? PostAuthor, long? GroupedId, MessageReactions? Reactions,
    IReadOnlyCollection<RestrictionReason>? RestrictionReason, int? TtlPeriod);