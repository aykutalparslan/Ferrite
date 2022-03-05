using System;
using System.Buffers;
using Ferrite.Crypto;

namespace Ferrite.Core;

public class PaddedIntermediateFrameDecoder : IFrameDecoder
{
    private readonly byte[] _lengthBytes = new byte[4];
    private int _length;
    private Aes256Ctr? _decryptor;

    public PaddedIntermediateFrameDecoder()
    {

    }

    public PaddedIntermediateFrameDecoder(Aes256Ctr decryptor)
    {
        _decryptor = decryptor;
    }

    public bool Decode(ref SequenceReader<byte> reader, out ReadOnlySequence<byte> frame)
    {
        if (_length == 0)
        {
            if (reader.Remaining < 4)
            {
                frame = new ReadOnlySequence<byte>();
                return false;
            }
            else
            {
                reader.TryCopyTo(_lengthBytes);
                if (_decryptor != null)
                {
                    _decryptor.Transform(_lengthBytes);
                    _length = (_lengthBytes[0]) |
                        (_lengthBytes[1] << 8) |
                        (_lengthBytes[2] << 16) |
                        (_lengthBytes[3] << 24);
                }
                reader.Advance(4);
            }
        }

        if (reader.Remaining < _length)
        {
            frame = new ReadOnlySequence<byte>();
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
        
        if (reader.Remaining != 0)
        {
            return true;
        }
        return false;
    }
}

