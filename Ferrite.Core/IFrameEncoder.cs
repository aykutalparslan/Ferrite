using System;
using System.Buffers;

namespace Ferrite.Core;

public interface IFrameEncoder
{
    ReadOnlySequence<byte> Encode(in ReadOnlySequence<byte> input);
}


