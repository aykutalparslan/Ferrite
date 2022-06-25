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
using System.Text;
using Ferrite.Crypto;
using Xunit;

namespace Ferrite.Tests.Crypto;

public class Crc32Tests
{
    [Fact]
    public void IncrementalCrc32_Should_Return_Correct_Crc32()
    {
        var random = new Random();
        var bytes = new byte[1024];
        random.NextBytes(bytes);
        var expected = bytes.AsSpan().GetCrc32();
        var crc = new IncrementalCrc32();
        //append the bytes to the crc in chunks of 8 bytes
        for (var i = 0; i < bytes.Length; i += 8)
        {
            crc.AppendData(new ReadOnlySequence<byte>(bytes,i, Math.Min(8, bytes.Length - i)));
        }
        Assert.Equal(expected, crc.Crc32);
    }
    [Fact]
    public void Crc32_Should_Return_Correct_Crc32()
    {
        var expected = 0xc0e1fa83;
        var bytes = Encoding.ASCII.GetBytes("asdlfhasldfg");
        var crc = bytes.AsSpan().GetCrc32();
        Assert.Equal(expected, crc);
        crc = new ReadOnlySequence<byte>(bytes).GetCrc32();
        Assert.Equal(expected, crc);
    }
}