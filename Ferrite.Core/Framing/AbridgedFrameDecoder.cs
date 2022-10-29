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
using System.Buffers;
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL.currentLayer;
using Ferrite.Utils;

namespace Ferrite.Core;

public class AbridgedFrameDecoder : FrameDecoderBase
{
    public AbridgedFrameDecoder(IMTProtoService mtproto) : base(mtproto)
    {
    }

    public AbridgedFrameDecoder(Aes256Ctr decryptor, IMTProtoService mtproto) : base(decryptor, mtproto)
    {
    }
    
    protected override bool DecodeLength(ref SequenceReader<byte> reader, out bool emptyFrame)
    {
        if (reader.Remaining == 0)
        {
            emptyFrame = true;
            return false;
        }
        GetFirstLengthByte(ref reader);
        if (LengthBytes[0] == 127 && reader.Remaining < 3)
        {
            emptyFrame = true;
            return false;
        }
        bool requiresQuickAck = CheckRequiresQuickAck(LengthBytes, 0);
        if (LengthBytes[0] < 127)
        {
            Length = LengthBytes[0] * 4;
        }
        else if (LengthBytes[0] == 127)
        {
            reader.TryCopyTo(LengthBytes.AsSpan().Slice(1, 3));
            reader.Advance(3);
            Decryptor?.Transform(LengthBytes.AsSpan().Slice(1, 3));
            requiresQuickAck = CheckRequiresQuickAck(LengthBytes, 3);
            Length = (LengthBytes[1]) |
                      (LengthBytes[2] << 8) |
                      (LengthBytes[3] << 16);
            Length *= 4;
        }

        emptyFrame = false;
        return requiresQuickAck;
    }

    private void GetFirstLengthByte(ref SequenceReader<byte> reader)
    {
        if (LengthBytes[0] == 0)
        {
            reader.TryCopyTo(LengthBytes.AsSpan()[..1]);
            reader.Advance(1);
            Decryptor?.Transform(LengthBytes.AsSpan()[..1]);
        }
    }
}

