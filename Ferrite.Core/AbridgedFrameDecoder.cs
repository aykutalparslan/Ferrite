﻿/*
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

