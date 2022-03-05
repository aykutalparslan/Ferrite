using System;
using System.Buffers;
using DotNext.Buffers;
using Ferrite.Crypto;

namespace Ferrite.Core;

public class PaddedIntermediateFrameEncoder : IFrameEncoder
{
    private Random _random;
    private Aes256Ctr? _encryptor;
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
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
        writer.Write(input.Length);
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
}

