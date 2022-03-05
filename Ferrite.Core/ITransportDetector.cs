﻿/*
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
    public interface ITransportDetector
    {
        MTProtoTransport DetectTransport(ref SequenceReader<byte> reader,
            out IFrameDecoder decoder, out IFrameEncoder encoder);
    }
}

