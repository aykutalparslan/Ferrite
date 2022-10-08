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
using System.IO.Compression;
using System.IO.Pipelines;
using DotNext.IO;
using Ferrite.TL;
using Ferrite.TL.mtproto;
using Ferrite.TL.slim;

namespace Ferrite.Core;

public class GZipProcessor : IProcessor
{
    private readonly ITLObjectFactory _factory;

    public GZipProcessor(ITLObjectFactory factory)
    {
        _factory = factory;
    }
    public async Task Process(object? sender, ITLObject input, Queue<ITLObject> output, TLExecutionContext ctx)
    {
        if (sender is MTProtoConnection connection)
        {
            if (input.Constructor == TLConstructor.GzipPacked)
            {
                var gzipped = new MemoryStream(((GzipPacked)input).PackedData);
                var stream = new GZipStream(gzipped, CompressionMode.Decompress);
                MemoryStream ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var decompressed = ms.ToArray();
                var rd = new SequenceReader(new ReadOnlySequence<byte>(decompressed));
                int constructor = rd.ReadInt32(true);
                var obj = _factory.Read(constructor, ref rd);
                output.Enqueue(obj);
            }
            else
            {
                output.Enqueue(input);
            }
        }
        else
        {
            output.Enqueue(input);
        }
    }

    public Task Process(object? sender, EncodedObject input, Queue<EncodedObject> output, TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }
}