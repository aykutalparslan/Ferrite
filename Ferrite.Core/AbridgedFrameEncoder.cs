using System;
using System.Buffers;
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
            writer.Write((byte)len & 0xff);
            writer.Write((byte)((len >> 8) & 0xFF));
            writer.Write((byte)((len >> 16) & 0xFF));
        }
        writer.Write(input.Length);
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

