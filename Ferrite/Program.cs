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

namespace Ferrite;

public class Program
{
    private static IContainer Container { get; set; }
    public static int Main(String[] args)
    {
        BuildIoCContainer();

        IConnectionListener socketListener = Container.BeginLifetimeScope()
            .Resolve<IConnectionListener>();
        socketListener.Bind(new IPEndPoint(IPAddress.Loopback, 5222));
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
        builder.RegisterType<Int128>();
        builder.RegisterType<Int256>();
        builder.RegisterType<TLObjectFactory>().As<ITLObjectFactory>();
        builder.RegisterType<MTProtoTransportDetector>().As<ITransportDetector>();
        builder.RegisterType<SocketConnectionListener>().As<IConnectionListener>();
        builder.RegisterType<RocksDBKVStore>().As<IKVStore>();
        builder.RegisterType<PersistentDataStore>().As<IPersistentStore>();
        builder.RegisterType<SerilogLogger>().As<ILogger>().SingleInstance();

        var container = builder.Build();

        Container = container;
    }

    internal static async void StartAccept(IConnectionListener socketListener)
    {
        while (true)
        {
            ISocketConnection? connection = await socketListener.AcceptAsync();
            
            if(connection != null)
            {
                connection.Start();
                var scope = Container.BeginLifetimeScope();
                MTProtoConnection mtProtoConnection = new MTProtoConnection(connection,
                    scope.Resolve<ITLObjectFactory>(), scope.Resolve<ITransportDetector>());
                mtProtoConnection.MessageReceived += MtProtoConnection_MessageReceived;
                mtProtoConnection.Start();
            }
        }
    }

    private static void MtProtoConnection_MessageReceived(object? sender, MTProtoAsyncEventArgs e)
    {
        var connection = (MTProtoConnection)sender;
        Console.WriteLine(e.Message.ToString());
        var result = e.Message.Execute(e.ExecutionContext);
        Console.WriteLine("-->"+result.ToString());
        connection.SendAsync(result);
    }
}
