/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
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
            var encryptionKey = payload.Slice(24, 32).ToArray();
            Array.Reverse(encryptionKey);
            var encryptionIV = payload.Slice(8, 16).ToArray();
            Array.Reverse(encryptionIV);

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

