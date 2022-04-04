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
namespace Ferrite.Tests.transport
{
    public class FrameEncoderTests
    {
        [Fact]
        public void ShouldEncodeIntermediate()
        {
            byte[] data = File.ReadAllBytes("testdata/message0");
            IntermediateFrameEncoder encoder = new IntermediateFrameEncoder();
            var encoded = encoder.Encode(new ReadOnlySequence<byte>(data)).ToArray();
            byte[] expected = File.ReadAllBytes("testdata/messageIntermediate0");
            Assert.Equal(expected, encoded);
            data = File.ReadAllBytes("testdata/message1");
            encoded = encoder.Encode(new ReadOnlySequence<byte>(data)).ToArray();
            expected = File.ReadAllBytes("testdata/messageIntermediate1");
            Assert.Equal(expected, encoded);
            data = File.ReadAllBytes("testdata/message2");
            encoded = encoder.Encode(new ReadOnlySequence<byte>(data)).ToArray();
            expected = File.ReadAllBytes("testdata/messageIntermediate2");
            Assert.Equal(expected, encoded);
        }
        [Fact]
        public void ShouldEncodeObfuscatedIntermediate()
        {
            byte[] data = File.ReadAllBytes("testdata/message0");
            byte[] key = File.ReadAllBytes("testdata/messageAesKey0");
            byte[] iv = File.ReadAllBytes("testdata/messageAesIv0");
            Aes256Ctr aes256Ctr = new Aes256Ctr(key, iv);
            IntermediateFrameEncoder encoder = new IntermediateFrameEncoder(aes256Ctr);
            var encoded = encoder.Encode(new ReadOnlySequence<byte>(data)).ToArray();
            byte[] expected = File.ReadAllBytes("testdata/messageEncrypted0");
            Assert.Equal(expected, encoded);
        }
    }
}

