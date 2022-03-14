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
using Ferrite.Crypto;

namespace Ferrite.Core;

public class MTProtoTransportDetector : ITransportDetector
{
    const byte Abridged = 0xef;
    const int AbridgedInt = unchecked((int)0xefefefef);
    const int Intermediate = unchecked((int)0xeeeeeeee);
    const int PaddedIntermediate = unchecked((int)0xdddddddd);
    const int Full = unchecked((int)0xdddddddd);
    const int WebSocketGet = 542393671;

    public MTProtoTransport DetectTransport(ref SequenceReader<byte> reader,
        out IFrameDecoder decoder, out IFrameEncoder encoder)
    {
        MTProtoTransport transport = MTProtoTransport.Unknown;
        decoder = null;
        encoder = null;
        if (reader.Remaining > 0)
        {
            reader.TryPeek(out var firstbyte);
            if (firstbyte == Abridged)
            {
                transport = MTProtoTransport.Abridged;
                reader.Advance(1);
                decoder = new AbridgedFrameDecoder();
                encoder = new AbridgedFrameEncoder();
                return transport;
            }
        }

        if (reader.Remaining > 3)
        {
            reader.TryReadLittleEndian(out int firstint);
            if (firstint == Intermediate)
            {
                transport = MTProtoTransport.Intermediate;
                decoder = new IntermediateFrameDecoder();
                encoder = new IntermediateFrameEncoder();
                return transport;
            }
            else if (firstint == PaddedIntermediate)
            {
                transport = MTProtoTransport.PaddedIntermediate;
                decoder = new PaddedIntermediateFrameDecoder();
                encoder = new PaddedIntermediateFrameEncoder();
                return transport;
            }
        }

        if (reader.Remaining > 3)
        {
            reader.TryReadLittleEndian(out int secondint);
            if (secondint == Full)
            {
                transport = MTProtoTransport.Full;
                reader.Rewind(8);
                decoder = new FullFrameDecoder();
                encoder = new FullFrameEncoder();
                return transport;
            }
        }
        else
        {
            reader.Rewind(4);
            return transport;
        }

        if (reader.Remaining > 55)
        {
            var payload = reader.Sequence.Slice(0, 64);
            var decryptionKey = payload.Slice(8, 32).ToArray();
            var decryptionIV = payload.Slice(40, 16).ToArray();
            var encryptionKey = payload.ToArray().Reverse().ToArray().AsSpan().Slice(8, 32).ToArray();
            var encryptionIV = payload.ToArray().Reverse().ToArray().AsSpan().Slice(40, 16).ToArray();
            
            Aes256Ctr decryptor = new Aes256Ctr(decryptionKey, decryptionIV);
            Aes256Ctr encryptor = new Aes256Ctr(encryptionKey, encryptionIV);

            Memory<byte> data = UnmanagedMemoryPool<byte>.Shared.Rent(64).Memory;
            
            decryptor.Transform(payload, data.Span);
            SpanReader<byte> rd = new SpanReader<byte>(data.Span.Slice(56));
            int identifier = rd.ReadInt32(true);
            if (identifier == AbridgedInt)
            {
                transport = MTProtoTransport.Abridged;
                decoder = new AbridgedFrameDecoder(decryptor);
                encoder = new AbridgedFrameEncoder(encryptor);
            }
            else if (identifier == Intermediate)
            {
                transport = MTProtoTransport.Intermediate;
                decoder = new IntermediateFrameDecoder(decryptor);
                encoder = new IntermediateFrameEncoder(encryptor);
            }
            else if (identifier == PaddedIntermediate)
            {
                transport = MTProtoTransport.PaddedIntermediate;
                decoder = new PaddedIntermediateFrameDecoder(decryptor);
                encoder = new PaddedIntermediateFrameEncoder(encryptor);
            }
            else
            {
                transport = MTProtoTransport.Full;
                decoder = new FullFrameDecoder(decryptor);
                encoder = new FullFrameEncoder(encryptor);
            }
            reader.Advance(56);
            return transport;
        }
        else
        {
            reader.Rewind(8);
            return transport;
        }
    }
}

