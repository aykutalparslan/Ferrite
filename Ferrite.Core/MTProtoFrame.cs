/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using System;
using System.Buffers;

namespace Ferrite.Core
{
    public class MTProtoFrame
    {
        public long AuthKeyId { get; set; }
        public ReadOnlySequence<byte> MsgKey { get; set; }
        public ReadOnlySequence<byte> EncryptedData { get; set; }
    }
}

