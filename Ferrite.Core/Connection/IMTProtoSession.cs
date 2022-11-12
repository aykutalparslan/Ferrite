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

using System.Net;
using Ferrite.Data;

namespace Ferrite.Core.Connection;

public interface IMTProtoSession
{
    MTProtoConnection? Connection { get; set; }
    IPEndPoint? EndPoint { get; set; }
    long AuthKeyId { get; }
    long PermAuthKeyId { get; }
    byte[]? AuthKey { get; }
    long SessionId { get; }
    long UniqueSessionId { get; }
    ServerSaltDTO ServerSalt { get; }
    Dictionary<string, object> SessionData { get; }
    bool TryFetchAuthKey(long authKeyId);
    int GenerateQuickAck(Span<byte> messageSpan);
    int GenerateSeqNo(bool isContentRelated);

    /// <summary>
    /// Gets the next Message Identifier (msg_id) for this session.
    /// </summary>
    /// <param name="response">If the message is a response to a client message.</param>
    /// <returns></returns>
    long NextMessageId(bool response);

    long CreateNewSession(long sessionId, long firstMessageId);

    /// <summary>
    /// Checks if the given message Id is valid and adds it to the last N messages list
    /// </summary>
    /// <param name="messageId"></param>
    /// <returns></returns>
    bool IsValidMessageId(long messageId);

    Services.MTProtoMessage GenerateSessionCreated(long firstMessageId, long serverSalt);

    public long SaveCurrentSession(long authKeyId);
}