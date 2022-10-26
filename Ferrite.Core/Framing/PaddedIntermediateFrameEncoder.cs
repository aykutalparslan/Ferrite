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
using System.IO.Pipelines;
using DotNext.Buffers;
using Ferrite.Crypto;

namespace Ferrite.Core;

public class PaddedIntermediateFrameEncoder : IFrameEncoder
{
    private Random _random;
    private Aes256Ctr? _encryptor;
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private int _currentFrameLength;
    public PaddedIntermediateFrameEncoder()
    {
        _random = new Random();
    }
    public PaddedIntermediateFrameEncoder(Aes256Ctr encryptor)
    {
        _random = new Random();
        _encryptor = encryptor;
    }

    public ReadOnlySequence<byte> Encode(in ReadOnlySequence<byte> input)
    {
        writer.WriteInt32((int)input.Length, true);
        writer.Write(input, false);
        if(writer.WrittenCount % 16 != 0)
        {
            byte[] padding = new byte[16 - (writer.WrittenCount % 16)];
            _random.NextBytes(padding);
            writer.Write(padding);
        }
        var frame = writer.ToReadOnlySequence();
        writer.Clear();
        if (_encryptor != null)
        {
            byte[] frameEncrypted = new byte[frame.Length];
            _encryptor.Transform(frame, frameEncrypted);
            frame = new ReadOnlySequence<byte>(frameEncrypted);
        }
        return frame;
    }

    public ReadOnlySequence<byte> EncodeHead(int length)
    {
        _currentFrameLength = length;
        writer.WriteInt32(length, true);
        var frame = writer.ToReadOnlySequence();
        writer.Clear();
        return frame;
    }

    public ReadOnlySequence<byte> EncodeBlock(in ReadOnlySequence<byte> input)
    {
        writer.Write(input, false);
        var frame = writer.ToReadOnlySequence();
        writer.Clear();
        if (_encryptor != null)
        {
            byte[] frameEncrypted = new byte[frame.Length];
            _encryptor.Transform(frame, frameEncrypted);
            frame = new ReadOnlySequence<byte>(frameEncrypted);
        }
        return frame;
    }

    public ReadOnlySequence<byte> EncodeTail()
    {
        int len = _currentFrameLength + 4;
        if(len % 16 != 0)
        {
            byte[] padding = new byte[16 - (len % 16)];
            _random.NextBytes(padding);
            writer.Write(padding);
        }
        var frame = writer.ToReadOnlySequence();
        _currentFrameLength = 0;
        writer.Clear();
        if (_encryptor != null)
        {
            byte[] frameEncrypted = new byte[frame.Length];
            _encryptor.Transform(frame, frameEncrypted);
            frame = new ReadOnlySequence<byte>(frameEncrypted);
        }
        return frame;
    }
}

