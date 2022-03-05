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

public class AbridgedFrameDecoder : IFrameDecoder
{
    private readonly byte[] _lengthBytes = new byte[4];
    private int _length;
    private Aes256Ctr? _decryptor;

    public AbridgedFrameDecoder()
    {
        
    }

    public AbridgedFrameDecoder(Aes256Ctr decryptor)
    {
        _decryptor = decryptor;
    }

    public bool Decode(ref SequenceReader<byte> reader, out ReadOnlySequence<byte> frame)
    {
        if (_length == 0)
        {
            if (reader.Remaining == 0)
            {
                frame = new ReadOnlySequence<byte>();
                return false;
            }
            else
            {
                if (_lengthBytes[0] == 0)
                {
                    reader.TryCopyTo(_lengthBytes.AsSpan().Slice(0, 1));
                    reader.Advance(1);
                    if (_decryptor != null)
                    {
                        _decryptor.Transform(_lengthBytes.AsSpan().Slice(0, 1));
                    }
                }
                if (_lengthBytes[0] < 127)
                {
                    _length = _lengthBytes[0] * 4;
                }
                else if (_lengthBytes[0] == 127)
                {
                    if (reader.Remaining < 3)
                    {
                        frame = new ReadOnlySequence<byte>();
                        return false;
                    }
                    reader.TryCopyTo(_lengthBytes.AsSpan().Slice(1, 3));
                    reader.Advance(3);
                    if (_decryptor != null)
                    {
                        _decryptor.Transform(_lengthBytes.AsSpan().Slice(1, 3));
                        _length = (_lengthBytes[1]) |
                            (_lengthBytes[2] << 8) |
                            (_lengthBytes[3] << 16);
                    }
                }
            }
        }

        if (reader.Remaining < _length)
        {
            frame = new ReadOnlySequence<byte>();
            return false;
        }
        ReadOnlySequence<byte> data = reader.UnreadSequence.Slice(0, _length);
        reader.Advance(_length);
        _length = 0;
        Array.Clear(_lengthBytes);
        if (_decryptor != null)
        {
            var frameDecrypted = new byte[data.Length];
            _decryptor.Transform(data, frameDecrypted);
            frame = new ReadOnlySequence<byte>(frameDecrypted);
        }
        else
        {
            frame = data;
        }
        if (reader.Remaining != 0)
        {
            return true;
        }

        return false;
    }
}

