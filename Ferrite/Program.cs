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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using Ferrite.Crypto;
using DotNext.Buffers;
using System.IO.Pipelines;
using System.Buffers;
using Ferrite.Core;
using Ferrite.Transport;
using System.Text.Json;
using Autofac;
using Ferrite.TL.mtproto;
using Ferrite.Utils;
using Ferrite.TL;
using System.Reflection;
using Ferrite.Data;
using StackExchange.Redis;
using MessagePack;
using Ferrite.Services;

namespace Ferrite;

public class Program
{
    public static async Task Main(String[] args)
    {
        IContainer container = BuildContainer();

        var scope = container.BeginLifetimeScope();
        IFerriteServer ferriteServer = scope.Resolve<IFerriteServer>();
        await ferriteServer.StartAsync(new IPEndPoint(IPAddress.Any, 5222), default);
    }

    private static IContainer BuildContainer()
    {
        var tl = Assembly.Load("Ferrite.TL");
        var builder = new ContainerBuilder();
        builder.RegisterType<MTProtoTime>().As<IMTProtoTime>().SingleInstance();
        builder.RegisterType<RandomGenerator>().As<IRandomGenerator>();
        builder.RegisterType<KeyProvider>().As<IKeyProvider>();
        builder.RegisterType<LangPackService>().As<ILangPackService>()
            .SingleInstance();
        builder.RegisterAssemblyTypes(tl)
            .Where(t => t.Namespace == "Ferrite.TL.mtproto")
            .AsSelf();
        builder.RegisterAssemblyTypes(tl)
            .Where(t => t.Namespace == "Ferrite.TL")
            .AsSelf();
        builder.RegisterAssemblyOpenGenericTypes(tl)
            .Where(t => t.Namespace == "Ferrite.TL")
            .AsSelf();
        builder.RegisterAssemblyTypes(tl)
            .Where(t => t.Namespace != null && t.Namespace.StartsWith("Ferrite.TL.layer139"))
            .AsSelf();
        builder.Register(_ => new Int128());
        builder.Register(_ => new Int256());
        builder.RegisterType<MTProtoConnection>();
        builder.RegisterType<AuthKeyProcessor>().As<IProcessor>().AsSelf();
        builder.RegisterType<MTProtoRequestProcessor>().As<IProcessor>().AsSelf();
        builder.RegisterType<IncomingMessageHandler>().As<IProcessorManager>().SingleInstance();
        builder.RegisterType<TLObjectFactory>().As<ITLObjectFactory>();
        builder.RegisterType<MTProtoTransportDetector>().As<ITransportDetector>();
        builder.RegisterType<SocketConnectionListener>().As<IConnectionListener>();
        builder.Register(_ => new CassandraDataStore("ferrite","cassandra"))
            .As<IPersistentStore>().SingleInstance();
        builder.Register(_=> new RedisDataStore("redis:6379"))
            .As<IDistributedStore>().SingleInstance();
        builder.Register(_=> new RedisPipe("redis:6379")).As<IDistributedPipe>();
        builder.RegisterType<SerilogLogger>().As<ILogger>().SingleInstance();
        builder.RegisterType<SessionManager>().As<ISessionManager>().SingleInstance();
        builder.RegisterType<FerriteServer>().As<IFerriteServer>();

        var container = builder.Build();

        return container;
    }
}
