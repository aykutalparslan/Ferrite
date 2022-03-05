using System;
using Ferrite.TL;

namespace Ferrite.Core
{
    public class MTProtoAsyncEventArgs : EventArgs
    {
        public ITLObject Message { get; set; }
        public long AuthKeyId { get; set; }
        public long MessageId { get; set; }
        public long Salt { get; set; }
        public long SessionId { get; set; }
        public int SeqNo { get; set; }
        public MTProtoAsyncEventArgs(ITLObject message, long authKeyId = 0,
            long messageId = 0, long salt = 0, long sessionId = 0, int seqNo = 0)
        {
            Message = message;
            AuthKeyId = authKeyId;
            MessageId = MessageId;
            Salt = salt;
            SessionId = sessionId;
            seqNo = seqNo;
        }
    }
}

