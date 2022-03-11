/*
 *   Project Ferrite is an Implementation Telegram Server API
 *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Affero General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Affero General Public License for more details.
 *
 *   You should have received a copy of the GNU Affero General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using Ferrite.TL;

namespace Ferrite.Core
{
    public class MTProtoAsyncEventArgs : EventArgs
    {
        public ITLObject Message { get; set; }
        public TLExecutionContext ExecutionContext { get; set; }
        public long AuthKeyId { get; set; }
        public long MessageId { get; set; }
        public long Salt { get; set; }
        public long SessionId { get; set; }
        public int SeqNo { get; set; }
        public MTProtoAsyncEventArgs(ITLObject message, TLExecutionContext context, long authKeyId = 0,
            long messageId = 0, long salt = 0, long sessionId = 0, int seqNo = 0)
        {
            Message = message;
            ExecutionContext = context;
            AuthKeyId = authKeyId;
            MessageId = MessageId;
            Salt = salt;
            SessionId = sessionId;
            seqNo = seqNo;
        }
    }
}

