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
using Autofac.Extras.Moq;
using Ferrite.Core;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.Utils;
using Moq;
using Xunit;

namespace Ferrite.Tests.Core;

public class DetectTransportTests
{
    [Fact]
    public void ShouldDetectObfuscatedIntermediate()
    {
        using(var mock = AutoMock.GetLoose())
        {
            var cache = mock.Mock<IDistributedCache>();
            cache.Setup(x => x.GetAuthKey(It.IsAny<long>()))
                .Returns(RandomNumberGenerator.GetBytes(192));
            cache.Setup(x => x.PutAuthKeyAsync(It.IsAny<long>(), 
                It.IsAny<byte[]>())).ReturnsAsync(true);
            var detector = mock.Create<MTProtoTransportDetector>();
            byte[] data = File.ReadAllBytes("testdata/obfuscatedIntermediate.bin");
            var seq = new ReadOnlySequence<byte>(data);
            var reader = new SequenceReader<byte>(seq);
            var actual = detector.DetectTransport(ref reader, out var decoder, out var encoder);
            Assert.Equal(MTProtoTransport.Intermediate, actual);
            Assert.Equal(64, reader.Consumed);
            Assert.NotNull(decoder);
            Assert.NotNull(encoder);
        }
    }
}

