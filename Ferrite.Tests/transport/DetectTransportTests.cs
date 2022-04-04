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
        builder.Register(_=> new Int128());
        builder.Register(_=> new Int256());
        builder.RegisterType<TLObjectFactory>().As<ITLObjectFactory>();
        builder.RegisterType<MTProtoTransportDetector>().As<ITransportDetector>();
        builder.RegisterType<CassandraDataStore>().As<IPersistentStore>();
        builder.RegisterType<SerilogLogger>().As<ILogger>().SingleInstance();
        var container = builder.Build();

        return container;
    }
}

