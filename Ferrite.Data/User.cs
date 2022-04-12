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
namespace Ferrite.Data;

public record User
{
    public bool Self { get; init; }
    public bool Contact { get; init; }
    public bool MutualContact { get; init; }
    public bool Bot { get; init; }
    public bool BotChatHistory { get; init; }
    public bool BotNoChats { get; init; }
    public bool Verified { get; init; }
    public bool Restricted { get; init; }
    public bool Min { get; init; }
    public bool BotInlineGeo { get; init; }
    public bool Support { get; init; }
    public bool Scam { get; init; }
    public bool ApplyMinPhoto { get; init; }
    public bool Fake { get; init; }
    public long Id { get; init; }
    public long HashAccessHash { get; init; }
    public long AccessHash { get; init; }
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string Username { get; init; } = default!;
    public string Phone { get; init; } = default!;
    public UserProfilePhoto Photo { get; init; } = default!;
    public UserStatus Status { get; init; }
    public int Expires { get; init; }
    public int WasOnline { get; init; }
    public int BotInfoVersion { get; init; }
    public RestrictionReason RestrictionReason { get; init; } = default!;
    public string BotInlinePlaceHolder { get; init; } = default!;
    public string LangCode { get; init; } = default!;
}

