/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
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

        Container = container;

        SocketListener socketListener = new SocketListener(new IPEndPoint(IPAddress.Loopback, 5222));
        socketListener.Bind();
        StartAccept(socketListener);
        Console.WriteLine("Server is listening...");
        while (true)
        {
            Console.ReadLine();
        }
    }


    internal static async void StartAccept(SocketListener socketListener)
    {
        while (true)
        {
            SocketConnection? connection = await socketListener.AcceptAsync();
            
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
        connection.Send(result);
    }
}
