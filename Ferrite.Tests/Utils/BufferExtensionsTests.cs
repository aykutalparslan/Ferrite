/*
 *   Project Ferrite is an Implementation Telegram Server API
 *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Affero General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Affero General Public License for more details.
 *
 *   You should have received a copy of the GNU Affero General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Buffers;
using System.Text;
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Utils;
using Xunit;

namespace Ferrite.Tests.Utils
{
    public class BufferExtensionsTests
    {
        //----
        //If L <= 253, the serialization contains one byte with the value of L,
        //then L bytes of the string followed by 0 to 3 characters containing 0,
        //such that the overall length of the value be divisible by 4, whereupon
        //all of this is interpreted as a sequence of int (L/4)+1 32-bit numbers.
        //----
        //If L >= 254, the serialization contains byte 254, followed by 3 bytes
        //with the string length L, followed by L bytes of the string, further
        //followed by 0 to 3 null padding bytes.
        //----


        /*[Theory]
        [InlineData(new byte[] {1}, 4)]
        [InlineData(new byte[] {1, 2}, 4)]
        [InlineData(new byte[] {1, 2, 3}, 4)]
        [InlineData(new byte[] {1, 2, 3, 4}, 8)]
        [InlineData(new byte[] {1, 2, 3, 4, 5}, 8)]
        [InlineData(new byte[] {1, 2, 3, 4, 5, 6, 7, 8}, 12)]
        public void WriteTLBytes_ShouldPadWithZeros(byte[] data, int bytesWritten)
        {
            byte[] buff = new byte[128];
            byte[] expected = new byte[bytesWritten];
            expected[0] = (byte)data.Length;
            for (int i = 0; i < data.Length; i++)
            {
                expected[i+1] = data[i];
            }

            Span<byte> s = buff.AsSpan();
            SpanWriter<byte> sr = new SpanWriter<byte>(s);
            sr.WriteTLBytes(data);
            Assert.Equal(expected, sr.WrittenSpan.ToArray());

            SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>();
            writer.WriteTLBytes(data);
            Assert.Equal(expected, writer.ToReadOnlySequence().ToArray());

            writer = new SparseBufferWriter<byte>();
            writer.WriteTLBytes(new ReadOnlySequence<byte>(data));
            Assert.Equal(expected, writer.ToReadOnlySequence().ToArray());

            writer = new SparseBufferWriter<byte>();
            writer.WriteTLBytes(new ReadOnlySpan<byte>(data));
            Assert.Equal(expected, writer.ToReadOnlySequence().ToArray());

            writer = new SparseBufferWriter<byte>();
            writer.WriteTLBytes(new ReadOnlySequence<byte>(data), false);
            Assert.Equal(expected, writer.ToReadOnlySequence().ToArray());

            writer = new SparseBufferWriter<byte>();
            writer.WriteTLBytes(new ReadOnlySequence<byte>(data), true);
            Assert.Equal(expected, writer.ToReadOnlySequence().ToArray());

        }
        [Theory]
        [InlineData(254, 260)]
        [InlineData(512, 516)]
        [InlineData(717, 724)]
        [InlineData(935, 940)]
        public void WriteTLBytes_ShouldPadWithZeros2(int numberOfBytes, int bytesWritten)
        {
            byte[] buff = new byte[1024];
            byte[] data = RandomNumberGenerator.GetBytes(numberOfBytes);
            Span<byte> s = buff.AsSpan();
            SpanWriter<byte> sr = new SpanWriter<byte>(s);
            sr.WriteTLBytes(data);
            byte[] expected = new byte[bytesWritten];
            expected[0] = (byte)254;
            expected[1] = (byte)(numberOfBytes & 0xff);
            expected[2] = ((byte)((numberOfBytes >> 8) & 0xff));
            expected[3] = ((byte)((numberOfBytes >> 16) & 0xff));
            for (int i = 0; i < data.Length; i++)
            {
                expected[i+4] = data[i];
            }
            Assert.Equal(expected, sr.WrittenSpan.ToArray());
        }*/

        /*[Theory]
        [InlineData(254)]
        [InlineData(512)]
        [InlineData(717)]
        [InlineData(935)]
        public void ReadTLBytes_ShouldReadBytes(int numberOfBytes)
        {
            byte[] buff = new byte[1024];
            byte[] data = RandomNumberGenerator.GetBytes(numberOfBytes);
            Span<byte> s = buff.AsSpan();
            SpanWriter<byte> sr = new SpanWriter<byte>(s);
            sr.WriteTLBytes(data);
            var reader = IAsyncBinaryReader.Create(sr.WrittenSpan.ToArray());
            var result = reader.ReadTLBytes().ToArray();
            Assert.Equal(data, result);
        }

        [Theory]
        [InlineData(254, 4, 8)]
        [InlineData(512, 1, 3)]
        [InlineData(8, 4, 4)]
        [InlineData(1, 3, 1)]
        public void ReadTLBytes_ShouldReadMultipleArrays(int numberOfBytes, int numberOfBytes2, int numberOfBytes3)
        {
            byte[] buff = new byte[1024];
            byte[] data = RandomNumberGenerator.GetBytes(numberOfBytes);
            byte[] data2 = RandomNumberGenerator.GetBytes(numberOfBytes2);
            byte[] data3 = RandomNumberGenerator.GetBytes(numberOfBytes3);
            Span<byte> s = buff.AsSpan();
            SpanWriter<byte> sr = new SpanWriter<byte>(s);
            sr.WriteTLBytes(data);
            sr.WriteTLBytes(data2);
            sr.WriteTLBytes(data3);
            var reader = IAsyncBinaryReader.Create(sr.WrittenSpan.ToArray());
            var result = reader.ReadTLBytes().ToArray();
            var result2 = reader.ReadTLBytes().ToArray();
            var result3 = reader.ReadTLBytes().ToArray();
            Assert.Equal(data, result);
            Assert.Equal(data2, result2);
            Assert.Equal(data3, result3);
        }*/

        [Theory]
        [InlineData("asdfasdf", "dsf", "ggggggg")]
        [InlineData("s", "asödmfnaösjdföasbdföabsdföb", "jjj")]
        [InlineData("asldjfalsjdghflasjghdfaksgdfasd" +
            "fasdfkjashdkfjhasdkfhjaksdjhfkasdfjha" +
            "sdaslkdjhfklashdfkjhaskdhfjkasdjhfkashjdf" +
            "aaslkdjhflaksdjflhasdkjfhkasdjhfkajshdkfhaj" +
            "sdalskdjflaksjdflkjasdlfjalskdjflasdjkflkakj" +
            "sdalskdjflaksjdflkjasdlfjalskdjflasdjkflkakj" +
            "sdalskdjflaksjdflkjasdlfjalskdjflasdjkflkakj" +
            "sdalskdjflaksjdflkjasdlfjalskdjflasdjkflkakj" +
            "sdalskdjflaksjdflkjasdlfjalskdjflasdjkflkakj" +
            "fasdfasdfasdfasdfasdfasdfasdfasdfasdf", "dsf", "ggggggg")]
        [InlineData("asdddfasdf", "ddddddddsf", "lkasjsdfdflkjasdlfjalsdjkf" +
            "asdfasdfasdfasdfasdfasdfasdfasdfasdfasdfas" +
            "asdasdfasdfasdfasdfasdfjashdgfjagsdjhfgjasdghff" +
            "asdasdfasdfasdfasdfasdfjashdgfjagsdjhfgjasdghff" +
            "asdasdfasdfasdfasdfasdfjashdgfjagsdjhfgjasdghff" +
            "asdasdfasdfasdfasdfasdfjashdgfjagsdjhfgjasdghff" +
            "asdasdfasdfasdfasdfasdfjashdgfjagsdjhfgjasdghff" +
            "asdasdfasdfasdfasdfasdfjashdgfjagsdjhfgjasdghff" +
            "asdasdfasdfasdfasdfasdfjashdgfjagsdjhfgjasdghff" +
            "asdfasdfasdfasdfa")]
        public void ReadTLBytes_ShouldReadMultipleStrings(string s1, string s2, string s3)
        {
            SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>();

            writer.WriteTLString(s1);
            writer.WriteTLString(s2);
            writer.WriteTLString(s3);
            SequenceReader reader = IAsyncBinaryReader.Create(writer.ToReadOnlySequence().ToArray());
            var result = reader.ReadTLString();
            var result2 = reader.ReadTLString();
            var result3 = reader.ReadTLString();
            Assert.Equal(s1, result);
            Assert.Equal(s2, result2);
            Assert.Equal(s3, result3);
        }

        [Theory]
        [InlineData("a")]
        [InlineData("aasdfjas")]
        [InlineData("şalskdfjşalskjdfşalskdhfşha")]
        [InlineData("aasşkdjfhaşskdjhflkasjhdlfkajhsdlkfhjalksdjhflakjh" +
            "alkjsdhflkajshdflkjhasdlkfhjalksdjhflkashjdflkajhsdlfkahjsdlfkhj" +
            "asdljkfhalsdkjfhlaksdjhflkajshdflkahjdsflkahjsdlkfhjlaksdhjflkahsd" +
            "sdlfkhjasdkfhgasldhgflasjdgfljaghsdljfghalsdjgfasldjfhlaksdjhflakjs" +
            "hdlfkjhasdlkfhjalsdkjhflaksjdhflaksdhjflaksjdhfhlasaksjdhfkasdhjfka" +
            "asmdlfalsdbjflasbdflabsdlfbasldjbfalskdjbflaksdbjflshdkfahjsdfjdhgf")]
        public void ShouldWriteAndReadTLString(string value)
        {
            var expected = Encoding.UTF8.GetBytes(value);
            SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>();
            writer.WriteTLString(value);

            var data = writer.ToReadOnlySequence();
            SequenceReader reader = IAsyncBinaryReader.Create(data);
            var actual = reader.ReadTLBytes().ToArray();
            Assert.Equal(expected, actual);
        }
    }
}

