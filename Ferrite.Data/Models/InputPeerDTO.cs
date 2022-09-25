﻿//
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
public record InputPeerDTO
{
    public InputPeerType InputPeerType { get; set; }
    public long ChatId { get; set; }
    public long UserId { get; set; }
    public long ChannelId { get; set; }
    public long AccessHash { get; set; }
    public int MsgId { get; set; }
    public InputPeerDTO? Peer { get; set; }

    public long GetPeerId()
    {
        long peerId = 0;
        if (Peer.InputPeerType is InputPeerType.User or InputPeerType.UserFromMessage)
        {
            peerId = Peer.UserId;
        }
        else if (Peer.InputPeerType == InputPeerType.Chat)
        {
            peerId = Peer.ChatId;
        }
        else if (Peer.InputPeerType is InputPeerType.Channel or InputPeerType.ChannelFromMessage)
        {
            peerId = Peer.ChannelId;
        }

        return peerId;
    }
}

