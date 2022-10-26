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
using DotNext.IO;
using Ferrite.TL;
using Ferrite.TL.mtproto;
using Ferrite.TL.slim;

namespace Ferrite.Core.RequestChain;

public class GZipProcessor : ILinkedHandler
{
    private readonly ITLObjectFactory _factory;

    public GZipProcessor(ITLObjectFactory factory)
    {
        _factory = factory;
    }
    
    public ILinkedHandler SetNext(ILinkedHandler value)
    {
        Next = value;
        return Next;
    }

    public ILinkedHandler Next { get; set; }

    public async ValueTask Process(object? sender, ITLObject input, TLExecutionContext ctx)
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
                await Next.Process(sender, obj, ctx);
            }
            else
            {
                await Next.Process(sender, input, ctx);
            }
        }
        else
        {
            await Next.Process(sender, input, ctx);
        }
    }

    public ValueTask Process(object? sender, TLBytes input, TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }
}