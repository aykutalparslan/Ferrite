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

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autofac;
using Ferrite.Core;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.TL;
using Ferrite.Utils;
using DotNext.IO;
using Xunit;
namespace Ferrite.Tests.Core
{
    public class FrameEncoderTests
    {
        [Fact]
        public void ShouldEncodeAbridged()
        {
            byte[] data = File.ReadAllBytes("testdata/abridged/raw0");
            AbridgedFrameEncoder encoder = new AbridgedFrameEncoder();
            var encoded = encoder.Encode(new ReadOnlySequence<byte>(data)).ToArray();
            byte[] expected = File.ReadAllBytes("testdata/abridged/encoded0");
            Assert.Equal(expected, encoded);
            data = File.ReadAllBytes("testdata/abridged/raw1");
            encoded = encoder.Encode(new ReadOnlySequence<byte>(data)).ToArray();
            expected = File.ReadAllBytes("testdata/abridged/encoded1");
            Assert.Equal(expected, encoded);
            data = File.ReadAllBytes("testdata/abridged/raw2");
            encoded = encoder.Encode(new ReadOnlySequence<byte>(data)).ToArray();
            expected = File.ReadAllBytes("testdata/abridged/encoded2");
            Assert.Equal(expected, encoded);
            data = File.ReadAllBytes("testdata/abridged/raw3");
            encoded = encoder.Encode(new ReadOnlySequence<byte>(data)).ToArray();
            expected = File.ReadAllBytes("testdata/abridged/encoded3");
            Assert.Equal(expected, encoded);
        }
        [Fact]
        public void ShouldEncodeObfuscatedAbridged()
        {
            byte[] data = File.ReadAllBytes("testdata/abridged/raw0");
            byte[] key = File.ReadAllBytes("testdata/abridged/aesKey");
            byte[] iv = File.ReadAllBytes("testdata/abridged/aesIV");
            AbridgedFrameEncoder encoder = new AbridgedFrameEncoder(new Aes256Ctr(key,iv));
            var encoded = encoder.Encode(new ReadOnlySequence<byte>(data)).ToArray();
            byte[] expected = File.ReadAllBytes("testdata/abridged/encrypted0");
            Assert.Equal(expected, encoded);
            data = File.ReadAllBytes("testdata/abridged/raw1");
            encoded = encoder.Encode(new ReadOnlySequence<byte>(data)).ToArray();
            expected = File.ReadAllBytes("testdata/abridged/encrypted1");
            Assert.Equal(expected, encoded);
            data = File.ReadAllBytes("testdata/abridged/raw2");
            encoded = encoder.Encode(new ReadOnlySequence<byte>(data)).ToArray();
            expected = File.ReadAllBytes("testdata/abridged/encrypted2");
            Assert.Equal(expected, encoded);
            data = File.ReadAllBytes("testdata/abridged/raw3");
            encoded = encoder.Encode(new ReadOnlySequence<byte>(data)).ToArray();
            expected = File.ReadAllBytes("testdata/abridged/encrypted3");
            Assert.Equal(expected, encoded);
        }
        [Fact]
        public void ShouldEncodeIntermediate()
        {
            byte[] data = File.ReadAllBytes("testdata/intermediate/raw0");
            IntermediateFrameEncoder encoder = new IntermediateFrameEncoder();
            var encoded = encoder.Encode(new ReadOnlySequence<byte>(data)).ToArray();
            byte[] expected = File.ReadAllBytes("testdata/intermediate/encoded0");
            Assert.Equal(expected, encoded);
            data = File.ReadAllBytes("testdata/intermediate/raw1");
            encoded = encoder.Encode(new ReadOnlySequence<byte>(data)).ToArray();
            expected = File.ReadAllBytes("testdata/intermediate/encoded1");
            Assert.Equal(expected, encoded);
            data = File.ReadAllBytes("testdata/intermediate/raw2");
            encoded = encoder.Encode(new ReadOnlySequence<byte>(data)).ToArray();
            expected = File.ReadAllBytes("testdata/intermediate/encoded2");
            Assert.Equal(expected, encoded);
        }
        [Fact]
        public void ShouldEncodeObfuscatedIntermediate()
        {
            byte[] data = File.ReadAllBytes("testdata/intermediate/raw0");
            byte[] key = File.ReadAllBytes("testdata/intermediate/aesKey");
            byte[] iv = File.ReadAllBytes("testdata/intermediate/aesIV");
            IntermediateFrameEncoder encoder = new IntermediateFrameEncoder(new Aes256Ctr(key, iv));
            var encoded = encoder.Encode(new ReadOnlySequence<byte>(data)).ToArray();
            byte[] expected = File.ReadAllBytes("testdata/intermediate/encrypted0");
            Assert.Equal(expected, encoded);
            data = File.ReadAllBytes("testdata/intermediate/raw1");
            encoded = encoder.Encode(new ReadOnlySequence<byte>(data)).ToArray();
            expected = File.ReadAllBytes("testdata/intermediate/encrypted1");
            Assert.Equal(expected, encoded);
            data = File.ReadAllBytes("testdata/intermediate/raw2");
            encoded = encoder.Encode(new ReadOnlySequence<byte>(data)).ToArray();
            expected = File.ReadAllBytes("testdata/intermediate/encrypted2");
            Assert.Equal(expected, encoded);
        }
    }
}

