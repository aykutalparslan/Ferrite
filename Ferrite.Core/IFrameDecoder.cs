using System;
using System.Buffers;

namespace Ferrite.Core;

public interface IFrameDecoder
{
    bool Decode(ref SequenceReader<byte> reader, out ReadOnlySequence<byte> frame);
}

