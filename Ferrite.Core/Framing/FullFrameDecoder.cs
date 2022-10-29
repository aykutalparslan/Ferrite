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

namespace Ferrite.Core;

public class FullFrameDecoder : FrameDecoderBase
{


    /*public bool Decode(ReadOnlySequence<byte> bytes, out ReadOnlySequence<byte> frame, 
        out bool isStream, out bool requiresQuickAck, out SequencePosition position)
    {
        var reader = new SequenceReader<byte>(bytes);
        isStream = _isStream;
        requiresQuickAck = false;
        if (_length == 0)
        {
            if (reader.Remaining < 4)
            {
                frame = new ReadOnlySequence<byte>();
                position = reader.Position;
                return false;
            }
            else
            {
                reader.TryCopyTo(_lengthBytes);
                if (_decryptor != null)
                {
                    _decryptor.Transform(_lengthBytes);
                }
                if ((_lengthBytes[3] & 1 << 7) == 1 << 7)
                {
                    requiresQuickAck = true;
                    _lengthBytes[3] &= 0x7f;
                }
                _length = (_lengthBytes[0]) |
                          (_lengthBytes[1] << 8) |
                          (_lengthBytes[2] << 16) |
                          (_lengthBytes[3] << 24);
                reader.Advance(4);
            }
        }
        if (reader.Remaining >= 72 && !_isStream)
        {
            _isStream = IsStream(reader.UnreadSequence.Slice(0, 72));
            isStream = _isStream;
        }

        if (_isStream && reader.Remaining > 0)
        {
            int toBeWritten = Math.Min(_remaining, StreamChunkSize);
            ReadOnlySequence<byte> chunk = reader.UnreadSequence.Slice(0, toBeWritten);
            reader.Advance(toBeWritten);
            _remaining -= toBeWritten;
            if (_decryptor != null)
            {
                var chunkDecrypted = new byte[toBeWritten];
                _decryptor.Transform(chunk, chunkDecrypted);
                frame = new ReadOnlySequence<byte>(chunkDecrypted);
            }
            else
            {
                frame = chunk;
            }
            if (_remaining == 0)
            {
                _length = 0;
                _isStream = false;
                Array.Clear(_lengthBytes);
                position = reader.Position;
                return false;
            }
            position = reader.Position;
            return true;
        }
        if (reader.Remaining < _length)
        {
            frame = new ReadOnlySequence<byte>();
            position = reader.Position;
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
            position = reader.Position;
            return false;
        }
        position = reader.Position;
        return false;
    }
    */
    public FullFrameDecoder(IMTProtoService mtproto) : base(mtproto)
    {
        SkipLength = 4;
        Header = 4;
        Tail = 4;
    }

    public FullFrameDecoder(Aes256Ctr decryptor, IMTProtoService mtproto) : base(decryptor, mtproto)
    {
        SkipLength = 4;
        Header = 4;
        Tail = 4;
    }

    protected override bool DecodeLength(ref SequenceReader<byte> reader, out bool emptyFrame)
    {
        if (reader.Remaining < 4)
        {
            emptyFrame = true;
            return false;
        }

        reader.TryCopyTo(LengthBytes);
        Decryptor?.Transform(LengthBytes);
        bool requiresQuickAck = CheckRequiresQuickAck(LengthBytes, 3);

        Length = (LengthBytes[0]) |
                 (LengthBytes[1] << 8) |
                 (LengthBytes[2] << 16) |
                 (LengthBytes[3] << 24);
        reader.Advance(4);
        emptyFrame = false;
        return requiresQuickAck;
    }
}

