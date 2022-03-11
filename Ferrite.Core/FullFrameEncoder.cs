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

public class FullFrameEncoder : IFrameEncoder
{
    private Aes256Ctr? _encryptor;
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private int _sequence = 0;
    public FullFrameEncoder()
    {
    }
    public FullFrameEncoder(Aes256Ctr encryptor)
    {
        _encryptor = encryptor;
    }

    public ReadOnlySequence<byte> Encode(in ReadOnlySequence<byte> input)
    {
        writer.WriteInt32((int)input.Length, true);
        writer.WriteInt32(_sequence++, true);
        writer.Write(input, false);
        writer.WriteInt32((int)writer.ToReadOnlySequence().GetCrc32(), true);
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
}

