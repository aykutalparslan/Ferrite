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
        builder.RegisterType<ReqDhParams>();
        builder.RegisterType<PQInnerDataDc>();
        builder.RegisterType<PQInnerDataTempDc>();
        builder.RegisterType<SerilogLogger>().As<ILogger>().SingleInstance();
        builder.RegisterAssemblyTypes(tl)
            .Where(t => t.Namespace == "Ferrite.TL.mtproto")
            .AsSelf();
        var container = builder.Build();

        Container = container;

        SocketListener socketListener = new SocketListener(new IPEndPoint(IPAddress.Loopback, 12345));
        socketListener.Bind();
        StartAccept(socketListener);
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
                MTProtoConnection mtProtoConnection = new MTProtoConnection(connection,
                    Container.BeginLifetimeScope().Resolve<ITLObjectFactory>());
                mtProtoConnection.Start();
            }
        }
    }
}
