//
//  Project Ferrite is an Implementation Telegram Server API
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
using Ferrite.Data;
using MessagePack;

namespace Ferrite.Services;

[MessagePackObject]
public class MTProtoMessage
{
    [Key(0)]
    public long SessionId { get; set; }
    [Key(1)]
    public bool IsResponse { get; set; }
    [Key(2)]
    public bool IsContentRelated { get; set; }
    [Key(3)]
    public byte[]? Data { get; set; }
    [Key(4)]
    public MTProtoMessageType MessageType { get; set; }
    [Key(5)]
    public byte[]? Nonce { get; set; }
    [Key(6)]
    public long MessageId { get; set; }
    public IDistributedFileOwner? StreamingFile { get; set; } = null;
}

