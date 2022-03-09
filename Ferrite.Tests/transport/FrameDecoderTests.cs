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
            builder.RegisterType<Int128>();
            builder.RegisterType<Int256>();
            builder.RegisterType<TLObjectFactory>().As<ITLObjectFactory>();
            builder.RegisterType<MTProtoTransportDetector>().As<ITransportDetector>();
            builder.RegisterType<RocksDBKVStore>().As<IKVStore>();
            builder.RegisterType<PersistentDataStore>().As<IPersistentStore>();
            builder.RegisterType<SerilogLogger>().As<ILogger>().SingleInstance();
            var container = builder.Build();

            return container;
        }
    }
}

