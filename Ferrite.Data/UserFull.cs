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

namespace Ferrite.Data;

public record UserFull
{
    public bool Blocked { get; init; }
    public bool PhoneCallsAvailable { get; init; }
    public bool PhoneCallsPrivate { get; init; }
    public bool CanPinMessage { get; init; }
    public bool HasScheduled { get; init; }
    public bool VideoCallsAvailable { get; init; }
    public long Id { get; init; }
    public string? About { get; init; }
    public PeerSettings Settings { get; init; }
    public Photo? ProfilePhoto { get; init; }
    public PeerNotifySettings NotifySettings { get; init; }
    public BotInfo? BotInfo { get; init; }
    public int? PinnedMessageId { get; init; }
    public int CommonChatsCount { get; init; }
    public int? FolderId { get; init; }
    public int? TtlPeriod { get; init; }
    public string? ThemeEmoticon { get; init; }
    public string? PrivateForwardName { get; init; }
}