/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
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

