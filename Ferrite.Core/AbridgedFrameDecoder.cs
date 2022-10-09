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

public class AbridgedFrameDecoder : IFrameDecoder
{
    private readonly byte[] _lengthBytes = new byte[4];
    private const int StreamChunkSize = 1024;
    private int _length;
    private int _remaining;
    private bool _isStream;
    private Aes256Ctr? _decryptor;
    private readonly IMTProtoService _mtproto;
    byte[] _headerBytes = new byte[72];

    public AbridgedFrameDecoder(IMTProtoService mtproto)
    {
        _mtproto = mtproto;
    }

    public AbridgedFrameDecoder(Aes256Ctr decryptor, IMTProtoService mtproto)
    {
        _decryptor = decryptor;
        _mtproto = mtproto;
    }

    public bool Decode(ReadOnlySequence<byte> bytes, out ReadOnlySequence<byte> frame, 
        out bool isStream, out bool requiresQuickAck, out SequencePosition position)
    {
        var reader = new SequenceReader<byte>(bytes);
        isStream = _isStream;
        requiresQuickAck = false;
        if (_length == 0)
        {
            if (reader.Remaining == 0)
            {
                frame = new ReadOnlySequence<byte>();
                position = reader.Position;
                return false;
            }

            if (_lengthBytes[0] == 0)
            {
                reader.TryCopyTo(_lengthBytes.AsSpan().Slice(0, 1));
                reader.Advance(1);
                if (_decryptor != null)
                {
                    _decryptor.Transform(_lengthBytes.AsSpan().Slice(0, 1));
                }
            }
            if ((_lengthBytes[0] & 1 << 7) == 1 << 7)
            {
                requiresQuickAck = true;
                _lengthBytes[0] &= 0x7f;
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
                    position = reader.Position;
                    return false;
                }
                reader.TryCopyTo(_lengthBytes.AsSpan().Slice(1, 3));
                reader.Advance(3);
                if (_decryptor != null)
                {
                    _decryptor.Transform(_lengthBytes.AsSpan().Slice(1, 3));
                }
                if ((_lengthBytes[3] & 1 << 7) == 1 << 7)
                {
                    requiresQuickAck = true;
                    _lengthBytes[3] &= 0x7f;
                }
                _length = (_lengthBytes[1]) |
                          (_lengthBytes[2] << 8) |
                          (_lengthBytes[3] << 16);
                _length *= 4;
            }
            _remaining = _length;
        }
        
        if (reader.Remaining >= 72 && !_isStream)
        {
            _isStream = IsStream(reader.UnreadSequence.Slice(0, 72));
            isStream = _isStream;
        }
        
        int toBeWritten = Math.Min(_remaining, StreamChunkSize);
        if (_isStream && reader.Remaining >= toBeWritten)
        {
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
        if (_remaining != 0)
        {
            position = reader.Position;
            return true;
        }
        position = reader.Position;
        return false;
    }

    private bool IsStream(ReadOnlySequence<byte> header)
    {
        SequenceReader reader;
        if (_decryptor != null)
        {
            _decryptor.TransformPeek(header, _headerBytes);
            reader = IAsyncBinaryReader.Create(_headerBytes);
        }
        else
        {
            header.CopyTo(_headerBytes);
            reader = IAsyncBinaryReader.Create(_headerBytes);
        }
        long authKeyId = reader.ReadInt64(true);
        var authKey = (_mtproto.GetAuthKey(authKeyId) ?? 
                       _mtproto.GetTempAuthKey(authKeyId));
        if (authKey is { Length: > 0 })
        {
            Span<byte> messageKey = stackalloc byte[16];
            reader.Read(messageKey);
            AesIge aesIge = new AesIge(authKey, messageKey);
            Span<byte> messageHeader = stackalloc byte[48];
            reader.Read(messageHeader);
            aesIge.Decrypt(messageHeader);
            SpanReader<byte> sr = new SpanReader<byte>(messageHeader);
            sr.Advance(32);
            int constructor = sr.ReadInt32(true);
            if (constructor == TLConstructor.Upload_SaveFilePart ||
                constructor == TLConstructor.Upload_SaveBigFilePart)
            {
                return true;
            }
        }
        return false;
    }
}

