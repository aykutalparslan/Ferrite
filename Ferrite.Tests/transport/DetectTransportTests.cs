/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
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
using Xunit;

namespace Ferrite.Tests.transport;

public class DetectTransportTests
{
    [Fact]
    public void ShouldDetectObfuscatedIntermediate()
    {
        var container = BuildContainer();
        var detector = container.Resolve<ITransportDetector>();
        byte[] data = File.ReadAllBytes("testdata/obfuscatedIntermediate.bin");
        var seq = new ReadOnlySequence<byte>(data);
        var reader = new SequenceReader<byte>(seq);
        var actual = detector.DetectTransport(ref reader, out var decoder, out var encoder);
        Assert.Equal(MTProtoTransport.Intermediate, actual);
        Assert.Equal(64, reader.Consumed);
        Assert.NotNull(decoder);
        Assert.NotNull(encoder);
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

