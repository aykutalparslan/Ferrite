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
    public class FrameDecoderTests
    {
        [Fact]
        public void ShouldDecodeObfuscatedIntermediate()
        {
            var container = BuildContainer();
            var detector = container.Resolve<ITransportDetector>();

            byte[] data = File.ReadAllBytes("testdata/obfuscatedIntermediateSession.bin");
            var seq = new ReadOnlySequence<byte>(data);
            var reader = new SequenceReader<byte>(seq);
            _ = detector.DetectTransport(ref reader, out var decoder, out var encoder);
            List<byte[]> frames = new();
            bool hasMore = false;
            do
            {
                hasMore = decoder.Decode(ref reader, out var frame);
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

        private static IContainer BuildContainer()
        {
            var tl = Assembly.Load("Ferrite.TL");
            var builder = new ContainerBuilder();
            builder.RegisterType<RandomGenerator>().As<IRandomGenerator>();
            builder.RegisterType<KeyProvider>().As<IKeyProvider>();
            builder.RegisterAssemblyTypes(tl)
                .Where(t => t.Namespace == "Ferrite.TL.mtproto")
                .AsSelf();
            builder.Register(_=> new Int128());
            builder.Register(_=> new Int256());
            builder.RegisterType<TLObjectFactory>().As<ITLObjectFactory>();
            builder.RegisterType<MTProtoTransportDetector>().As<ITransportDetector>();
            builder.RegisterType<RocksDBKVStore>().As<IKVStore>();
            builder.RegisterType<KVDataStore>().As<IPersistentStore>();
            builder.RegisterType<SerilogLogger>().As<ILogger>().SingleInstance();
            var container = builder.Build();

            return container;
        }
    }
}

