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

public class AbridgedFrameEncoder : IFrameEncoder
{
    private Aes256Ctr? _encryptor;
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    public AbridgedFrameEncoder()
    {
    }
    public AbridgedFrameEncoder(Aes256Ctr encryptor)
    {
        _encryptor = encryptor;
    }

    public ReadOnlySequence<byte> Encode(in ReadOnlySequence<byte> input)
    {
        int len = (int)input.Length / 4;
        if (len < 127)
        {
            writer.Write((byte)len);
        }
        else
        {
            writer.Write((byte)0x7f);
            writer.Write((byte)(len & 0xff));
            writer.Write((byte)((len >> 8) & 0xFF));
            writer.Write((byte)((len >> 16) & 0xFF));
        }
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

    public ReadOnlySequence<byte> EncodeHead(int length)
    {
        int len = length / 4;
        if (len < 127)
        {
            writer.Write((byte)len);
        }
        else
        {
            writer.Write((byte)0x7f);
            writer.Write((byte)(len & 0xff));
            writer.Write((byte)((len >> 8) & 0xFF));
            writer.Write((byte)((len >> 16) & 0xFF));
        }
        var result = writer.ToReadOnlySequence();
        writer.Clear();
        return result;
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
        return new ReadOnlySequence<byte>();
    }
}

