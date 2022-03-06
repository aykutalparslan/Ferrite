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

public class IntermediateFrameEncoder : IFrameEncoder
{
    private Aes256Ctr? _encryptor;
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    public IntermediateFrameEncoder()
    {
    }
    public IntermediateFrameEncoder(Aes256Ctr encryptor)
    {
        _encryptor = encryptor;
    }

    public ReadOnlySequence<byte> Encode(in ReadOnlySequence<byte> input)
    {
        writer.WriteInt32((int)input.Length, true);
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
}
