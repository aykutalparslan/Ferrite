/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using System;
using System.Buffers;
using Ferrite.Crypto;

namespace Ferrite.Core;

public class FullFrameDecoder : IFrameDecoder
{
    private readonly byte[] _lengthBytes = new byte[4];
    private int _length;
    private int _sequence;
    private Aes256Ctr? _decryptor;

    public FullFrameDecoder()
    {

    }

    public FullFrameDecoder(Aes256Ctr decryptor)
    {
        _decryptor = decryptor;
    }

    public bool Decode(ref SequenceReader<byte> reader, out ReadOnlySequence<byte> frame)
    {
        if (_length == 0)
        {
            if (reader.Remaining < 4)
            {
                frame = new ReadOnlySequence<byte>();
                return false;
            }
            else
            {
                reader.TryCopyTo(_lengthBytes);
                if (_decryptor != null)
                {
                    _decryptor.Transform(_lengthBytes);
                    _length = (_lengthBytes[0]) |
                        (_lengthBytes[1] << 8) |
                        (_lengthBytes[2] << 16) |
                        (_lengthBytes[3] << 24);
                }
                reader.Advance(4);
            }
        }
        if (reader.Remaining < _length)
        {
            frame = new ReadOnlySequence<byte>();
            return false;
        }
        var data = new byte[_length];
        Array.Copy(_lengthBytes, data, 4);
        reader.UnreadSequence.Slice(0, _length - 4).CopyTo(data.AsSpan().Slice(4));
        reader.Advance(_length-4);
        _length = 0;
        Array.Clear(_lengthBytes);
        if (_decryptor != null)
        {
            _decryptor.Transform(data.AsSpan().Slice(4));
        }
        uint crc32 = data.AsSpan().GetCrc32();
        _sequence = (data[4]) |
                        (data[5] << 8) |
                        (data[6] << 16) |
                        (data[7] << 24);

        int checkSum = (data[data.Length-4]) |
                        (data[data.Length - 3] << 8) |
                        (data[data.Length - 2] << 16) |
                        (data[data.Length - 1] << 24);

        if (crc32 == (uint)checkSum)
        {
            frame = new ReadOnlySequence<byte>(data.AsMemory().Slice(8,data.Length-12));
        }
        else
        {
            frame = new ReadOnlySequence<byte>();
        }
        if (reader.Remaining != 0)
        {
            return false;
        }
        return false;
    }
}

