// 
// Project Ferrite is an Implementation of the Telegram Server API
// Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 

using System.Buffers;
using System.IO.Pipelines;
using DotNext.Buffers;
using DotNext.IO;
using DotNext.IO.Pipelines;

namespace Ferrite.Core.Framing;

public class EncoderPipe : IDisposable
{
    private readonly Pipe _encoderPipe;
    private readonly Pipe _pipe;
    private readonly IFrameEncoder _encoder;
    private Task? _encodeTask;
    public EncoderPipe(IFrameEncoder encoder)
    {
        _encoder = encoder;
        _encoderPipe = new Pipe();
        _pipe = new Pipe();
        _encodeTask = DoEncode();
        Input = _pipe.Reader;
    }
    public async ValueTask<long> WriteLength(int length)
    {
        return await _encoderPipe.Writer.WriteAsync(_encoder.GenerateHead(length));
    }
    public async ValueTask<FlushResult> WriteAsync(SequenceReader reader)
    {
        int count = (int)reader.RemainingSequence.Length;
        var encBuff = _encoderPipe.Writer.GetMemory(count);
        reader.Read(encBuff.Span.Slice(0, count));
        _encoderPipe.Writer.Advance(count);
        return await _encoderPipe.Writer.FlushAsync();
    }
    public async ValueTask<FlushResult> WriteAsync(byte[] data)
    {
        return await _encoderPipe.Writer.WriteAsync(data);
    }
    public async ValueTask<FlushResult> WriteAsync(Memory<byte> data)
    {
        return await _encoderPipe.Writer.WriteAsync(data);
    }
    public async ValueTask<long> WriteAsync(ReadOnlySequence<byte> data)
    {
        return await _encoderPipe.Writer.WriteAsync(data);
    }
    private async Task DoEncode()
    {
        while (true)
        {
            var readResult = await _encoderPipe.Reader.ReadAsync();
            var buff = readResult.Buffer;
            var encoded = _encoder.EncodeBlock(buff);
            _pipe.Writer.Write(encoded);
            await _pipe.Writer.FlushAsync();
            _encoderPipe.Reader.AdvanceTo(readResult.Buffer.End, 
                readResult.Buffer.End);

            if (readResult.IsCompleted)
            {
                var tail = _encoder.EncodeTail();
                if (tail.Length > 0)
                {
                    await _pipe.Writer.WriteAsync(tail);
                }
                await _pipe.Writer.CompleteAsync();
                break;
            }
        }
    }

    public async ValueTask CompleteAsync()
    {
        await _encoderPipe.Writer.CompleteAsync();
    }

    public PipeReader Input { get; }

    public void Dispose()
    {
        
    }
}