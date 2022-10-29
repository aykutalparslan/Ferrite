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
using System.Security.Cryptography;
using Autofac;
using Ferrite.Core;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.TL;
using Ferrite.Utils;
using DotNext.IO;
using Xunit;
using Autofac.Extras.Moq;
using Ferrite.Core.Framing;
using Ferrite.Services;
using Moq;

namespace Ferrite.Tests.Core
{
    public class FrameDecoderTests
    {
        [Fact]
        public void ShouldDecodeObfuscatedAbridged()
        {
            using var mock = AutoMock.GetLoose();
            var proto = mock.Mock<IMTProtoService>();
            proto.Setup(x => x.GetAuthKey(It.IsAny<long>()))
                .Returns(RandomNumberGenerator.GetBytes(192));
            proto.Setup(x => x.PutAuthKeyAsync(It.IsAny<long>(), 
                It.IsAny<byte[]>())).ReturnsAsync(true);
            var detector = mock.Create<MTProtoTransportDetector>();
            byte[] data = File.ReadAllBytes("testdata/obfuscatedAbridgedSession.bin");
            var seq = new ReadOnlySequence<byte>(data);
            SequencePosition pos = seq.Start;
            _ = detector.DetectTransport(seq, out var decoder, out var encoder, out pos);
            List<byte[]> frames = new();
            bool hasMore = false;
            do
            {
                hasMore = decoder.Decode(seq.Slice(pos), out var frame, 
                    out var isStream, out var requiresQuickAck, out pos);
                var framedata = frame.ToArray();
                frames.Add(framedata);
            } while (hasMore);
            Assert.Equal(10, frames.Count);
            Assert.Equal(40, frames[0].Length);
            SequenceReader rd = IAsyncBinaryReader.Create(frames[0]);
            long authKey = rd.ReadInt64(true);
            long msgId = rd.ReadInt64(true);
            int msgLength = rd.ReadInt32(true);
            int constructor = rd.ReadInt32(true);
            Assert.Equal(0, authKey);
            Assert.Equal(7083195348767984184, msgId);
            Assert.Equal(20, msgLength);
            Assert.Equal(-1099002127, constructor);
            Assert.Equal(340, frames[1].Length);
            rd = IAsyncBinaryReader.Create(frames[1]);
            authKey = rd.ReadInt64(true);
            msgId = rd.ReadInt64(true);
            msgLength = rd.ReadInt32(true);
            constructor = rd.ReadInt32(true);
            Assert.Equal(0, authKey);
            Assert.Equal(7083195350463984280, msgId);
            Assert.Equal(320, msgLength);
            Assert.Equal(-686627650, constructor);
            Assert.Equal(396, frames[2].Length); rd = IAsyncBinaryReader.Create(frames[1]);
            rd = IAsyncBinaryReader.Create(frames[2]);
            authKey = rd.ReadInt64(true);
            msgId = rd.ReadInt64(true);
            msgLength = rd.ReadInt32(true);
            constructor = rd.ReadInt32(true);
            Assert.Equal(0, authKey);
            Assert.Equal(7083195351299983772, msgId);
            Assert.Equal(376, msgLength);
            Assert.Equal(-184262881, constructor);
        }


        [Fact]
        public void ShouldDecodeObfuscatedIntermediate()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var proto = mock.Mock<IMTProtoService>();
                proto.Setup(x => x.GetAuthKey(It.IsAny<long>()))
                    .Returns(RandomNumberGenerator.GetBytes(192));
                proto.Setup(x => x.PutAuthKeyAsync(It.IsAny<long>(), 
                    It.IsAny<byte[]>())).ReturnsAsync(true);
                var detector = mock.Create<MTProtoTransportDetector>();

                byte[] data = File.ReadAllBytes("testdata/obfuscatedIntermediateSession.bin");
                var seq = new ReadOnlySequence<byte>(data);
                SequencePosition pos = seq.Start;
                _ = detector.DetectTransport(seq, out var decoder, out var encoder, out pos);
                List<byte[]> frames = new();
                bool hasMore = false;
                do
                {
                    hasMore = decoder.Decode(seq.Slice(pos), out var frame, 
                        out var isStream, out var requiresQuickAck, out pos);
                    var framedata = frame.ToArray();
                    frames.Add(framedata);
                } while (hasMore);
                Assert.Equal(3, frames.Count);
                Assert.Equal(244, frames[0].Length);
                SequenceReader rd = IAsyncBinaryReader.Create(frames[0]);
                long authKey = rd.ReadInt64(true);
                long msgId = rd.ReadInt64(true);
                int msgLength = rd.ReadInt32(true);
                int constructor = rd.ReadInt32(true);
                Assert.Equal(0, authKey);
                Assert.Equal(0, msgId);
                Assert.Equal(224, msgLength);
                Assert.Equal(-1099002127, constructor);
                Assert.Equal(580, frames[1].Length);
                rd = IAsyncBinaryReader.Create(frames[1]);
                authKey = rd.ReadInt64(true);
                msgId = rd.ReadInt64(true);
                msgLength = rd.ReadInt32(true);
                constructor = rd.ReadInt32(true);
                Assert.Equal(0, authKey);
                Assert.Equal(0, msgId);
                Assert.Equal(560, msgLength);
                Assert.Equal(-686627650, constructor);
                Assert.Equal(436, frames[2].Length); rd = IAsyncBinaryReader.Create(frames[1]);
                rd = IAsyncBinaryReader.Create(frames[2]);
                authKey = rd.ReadInt64(true);
                msgId = rd.ReadInt64(true);
                msgLength = rd.ReadInt32(true);
                constructor = rd.ReadInt32(true);
                Assert.Equal(0, authKey);
                Assert.Equal(0, msgId);
                Assert.Equal(416, msgLength);
                Assert.Equal(-184262881, constructor);
            }
        }
    }
}

