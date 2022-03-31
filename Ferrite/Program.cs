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

namespace Ferrite;

public class Program
{
    private static IContainer Container { get; set; }
    private static SessionManager sessionManager;
    private static IConnectionListener socketListener;
    public static int Main(String[] args)
    {
        BuildIoCContainer();

        sessionManager = Container.Resolve<SessionManager>();
        socketListener = Container.BeginLifetimeScope()
            .Resolve<IConnectionListener>();
        socketListener.Bind(new IPEndPoint(IPAddress.Any, 5222));
        StartAccept(socketListener);
        Console.WriteLine("Server is listening...");
        while (true)
        {
            Console.ReadLine();
        }
    }

    private static void BuildIoCContainer()
    {
        var tl = Assembly.Load("Ferrite.TL");
        var builder = new ContainerBuilder();
        builder.RegisterType<RandomGenerator>().As<IRandomGenerator>();
        builder.RegisterType<KeyProvider>().As<IKeyProvider>();
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
        builder.RegisterType<TLObjectFactory>().As<ITLObjectFactory>();
        builder.RegisterType<MTProtoTransportDetector>().As<ITransportDetector>();
        builder.RegisterType<SocketConnectionListener>().As<IConnectionListener>();
        builder.Register(_ => new CassandraDataStore("ferrite","cassandra"))
            .As<IPersistentStore>().SingleInstance();
        builder.Register(_=> new RedisDataStore("redis:6379"))
            .As<IDistributedStore>().SingleInstance();
        builder.Register(_=> new RedisPipe("redis:6379")).As<IDistributedPipe>();
        builder.RegisterType<SerilogLogger>().As<ILogger>().SingleInstance();
        builder.RegisterType<SessionManager>().SingleInstance();

        var container = builder.Build();

        Container = container;
    }

    internal static async void StartAccept(IConnectionListener socketListener)
    {
        while (true)
        {
            ITransportConnection? connection = await socketListener.AcceptAsync();
            
            if(connection != null)
            {
                connection.Start();
                var scope = Container.BeginLifetimeScope();
                MTProtoConnection mtProtoConnection = scope.Resolve<MTProtoConnection>(new NamedParameter("connection", connection));
                mtProtoConnection.MessageReceived += MtProtoConnection_MessageReceived;
                mtProtoConnection.Start();
            }
        }
    }

    private static async Task MtProtoConnection_MessageReceived(object? sender, MTProtoAsyncEventArgs e)
    {
        Console.WriteLine(e.Message.ToString());
        if (e.Message is ITLMethod method)
        {
            var result = await method.ExecuteAsync(e.ExecutionContext);
            Console.WriteLine("-->" + result.ToString());
            if (sender != null)
            {
                var connection = (MTProtoConnection)sender;
                await connection.SendAsync(result);
            }
        }
    }
}
