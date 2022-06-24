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

using Ferrite.Data;
using Xunit;

namespace Ferrite.Tests.Data;

public class ConcatenatedStreamTests
{
    [Theory]
    [InlineData(16, 0, 16 * 1024 + 100)]
    [InlineData(16, 2 * 1024, 16 * 1024 + 100)]
    [InlineData(16, 512, 16 * 1024 + 100)]
    [InlineData(16, 512, 13000)]
    public void ConcatenatedStream_Should_Read(int count, int offset, int limit)
    {
        byte[] data = new byte[count * 1024 + 100];
        new Random().NextBytes(data);
        var streams = new Queue<Stream>();
        for (int i = 0; i < count; i++)
        {
            var stream = new MemoryStream(data, i * 1024, 1024);
            streams.Enqueue(stream);
        }
        var stream2 = new MemoryStream(data, count * 1024, 100);
        streams.Enqueue(stream2);
        var concatenatedStream = new ConcatenatedStream(streams, offset, limit);
        byte[] actual = new byte[concatenatedStream.Length];
        int remaining = (int)concatenatedStream.Length;
        while (remaining > 0)
        {
            int read = concatenatedStream.Read(actual, actual.Length - remaining, remaining);
            remaining -= read;
        }
        var expected = data.AsSpan().Slice(offset,
            Math.Min(limit, data.Length - offset)).ToArray();
        Assert.Equal(expected, actual);
    }
}