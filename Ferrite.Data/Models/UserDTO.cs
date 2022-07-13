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
using MessagePack;

namespace Ferrite.Data;

[MessagePackObject(true)] 
public record UserDTO
{
    public bool Empty { get; set; }
    public bool Self { get; set; }
    public bool Contact { get; set; }
    public bool MutualContact { get; set; }
    public bool Bot { get; set; }
    public bool BotChatHistory { get; set; }
    public bool BotNoChats { get; set; }
    public bool Verified { get; set; }
    public bool Restricted { get; set; }
    public bool Min { get; set; }
    public bool BotInlineGeo { get; set; }
    public bool Support { get; set; }
    public bool Scam { get; set; }
    public bool ApplyMinPhoto { get; set; }
    public bool Fake { get; set; }
    public long Id { get; set; }
    public long HashAccessHash { get; set; }
    public long AccessHash { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string About { get; set; } = default!;
    public UserProfilePhotoDTO Photo { get; set; } = default!;
    public UserStatusDTO Status { get; set; }
    public int Expires { get; set; }
    public int WasOnline { get; set; }
    public int BotInfoVersion { get; set; }
    public RestrictionReasonDTO RestrictionReason { get; set; } = default!;
    public string BotInlinePlaceHolder { get; set; } = default!;
    public string LangCode { get; set; } = default!;
}

