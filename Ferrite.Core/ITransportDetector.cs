using System;
using System.Buffers;

namespace Ferrite.Core
{
    public interface ITransportDetector
    {
        MTProtoTransport DetectTransport(ref SequenceReader<byte> reader,
            out IFrameDecoder decoder, out IFrameEncoder encoder);
    }
}

