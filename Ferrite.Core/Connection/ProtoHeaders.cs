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

namespace Ferrite.Core.Connection;

public readonly struct ProtoHeaders
{
    public ProtoHeaders(long authKeyId, long salt, long sessionId, long messageId, int sequenceNo)
    {
        AuthKeyId = authKeyId;
        Salt = salt;
        SessionId = sessionId;
        MessageId = messageId;
        SequenceNo = sequenceNo;
    }

    public long AuthKeyId { get; init; }
    public long Salt { get; init; }
    public long SessionId { get; init; }
    public long MessageId { get; init; }
    public int SequenceNo { get; init; }
}