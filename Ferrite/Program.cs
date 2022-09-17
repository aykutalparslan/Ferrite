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
using System.Numerics;
using Ferrite.Core;
using Ferrite.Transport;
using System.Text.Json;
using Autofac;
using Ferrite.TL.mtproto;
using Ferrite.Utils;
using Ferrite.TL;
using System.Reflection;
using Ferrite.Core.Methods;
using Ferrite.Data;
using Ferrite.Data.Repositories;
using StackExchange.Redis;
using MessagePack;
using Ferrite.Services;
using Ferrite.TL.ObjectMapper;

namespace Ferrite;

public class Program
{
    public static async Task Main(String[] args)
    {
        IContainer container = BuildContainer();

        var scope = container.BeginLifetimeScope();

        var keyProvider = scope.Resolve<IKeyProvider>();
        var fingerprints = keyProvider.GetRSAFingerprints();
        foreach (var fingerprint in fingerprints)
        {
            var key = keyProvider.GetKey(fingerprint);
            Console.WriteLine(key?.ExportPublicKey());
            Console.WriteLine($"Modulus: {new BigInteger(key?.PublicKeyParameters.Modulus, true, true)}");
            Console.WriteLine($"Exponent: {new BigInteger(key?.PublicKeyParameters.Exponent,true,true)}");
            Console.WriteLine($"Fingerprint-HEX: 0x{fingerprint:X}");
            Console.WriteLine($"Fingerprint-DECIMAL: {fingerprint}");
        }
        
        IFerriteServer ferriteServer = scope.Resolve<IFerriteServer>();
        await ferriteServer.StartAsync(new IPEndPoint(IPAddress.Any, 5222), default);
    }

    private static IContainer BuildContainer()
    {
        var tl = Assembly.Load("Ferrite.TL");
        var core = Assembly.Load("Ferrite.Core");
        var builder = new ContainerBuilder();
        builder.RegisterType<MTProtoTime>().As<IMTProtoTime>().SingleInstance();
        builder.RegisterType<RandomGenerator>().As<IRandomGenerator>();
        builder.RegisterType<KeyProvider>().As<IKeyProvider>();
        builder.RegisterType<MTProtoService>().As<IMTProtoService>()
            .SingleInstance();
        builder.RegisterType<LangPackService>().As<ILangPackService>()
            .SingleInstance();
        builder.RegisterType<UpdatesService>().As<IUpdatesService>()
            .SingleInstance();
        builder.RegisterType<ContactsService>().As<IContactsService>()
            .SingleInstance();
        builder.RegisterType<UserService>().As<IUsersService>()
            .SingleInstance();
        builder.RegisterType<SkiaPhotoProcessor>().As<IPhotoProcessor>()
            .SingleInstance();
        builder.RegisterType<PhotosService>().As<IPhotosService>()
            .SingleInstance();
        builder.RegisterType<UploadService>().As<IUploadService>()
            .SingleInstance();
        builder.RegisterType<MessagesService>().As<IMessagesService>()
            .SingleInstance();
        builder.RegisterType<MessageRepository>().As<IMessageRepository>()
            .SingleInstance();
        builder.RegisterType<DefaultUnitOfWork>().As<IUnitOfWork>()
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
            .Where(t => t.Namespace != null && t.Namespace.StartsWith("Ferrite.TL.currentLayer"))
            .AsSelf();
        builder.RegisterAssemblyTypes(core)
            .Where(t => t.Namespace == "Ferrite.Core.Methods")
            .AsSelf();
        builder.RegisterAssemblyOpenGenericTypes(core)
            .Where(t => t.Namespace == "Ferrite.Core.Methods")
            .AsSelf();
        builder.RegisterType<DefaultApiLayer>().As<IApiLayer>().SingleInstance();
        builder.RegisterType<DefaultMapper>().As<IMapperContext>().SingleInstance();
        builder.Register(_ => new Ferrite.TL.Int128());
        builder.Register(_ => new Int256());
        builder.RegisterType<MTProtoConnection>();
        builder.RegisterType<AuthKeyProcessor>();
        builder.RegisterType<MsgContainerProcessor>();
        builder.RegisterType<ServiceMessagesProcessor>();
        builder.RegisterType<AuthorizationProcessor>();
        builder.RegisterType<MTProtoRequestProcessor>();
        builder.RegisterType<IncomingMessageHandler>().As<IProcessorManager>().SingleInstance();
        builder.RegisterType<TLObjectFactory>().As<ITLObjectFactory>();
        builder.RegisterType<MTProtoTransportDetector>().As<ITransportDetector>();
        builder.RegisterType<SocketConnectionListener>().As<IConnectionListener>();
        builder.Register(_ => new S3ObjectStore("http://localhost:9000", 
                "minioadmin", "minioadmin"))
            .As<IObjectStore>().SingleInstance();
        builder.Register(_ => new CassandraDataStore("ferrite","localhost"))
            .As<IPersistentStore>().SingleInstance();
        builder.Register(_ => new DefaultUnitOfWork(new SerilogLogger(),
                "localhost:6379", "ferrite","localhost"))
            .As<IUnitOfWork>().SingleInstance();
        builder.Register(_ => new ElasticSearchEngine("https://localhost:9200",
                "ferrite", "ferrite-server",
                 "49:99:E9:CA:B6:8C:6B:78:8F:C5:A2:7A:F2:78:CA:FA:08:1E:66:9F:2F:E8:2C:C5:87:35:49:B4:4C:07:62:DD"))
            .As<ISearchEngine>().SingleInstance();
        builder.Register(_ => new LangPackDataStore("ferrite","localhost"))
            .As<ILangPackDataStore>().SingleInstance();
        builder.Register(_=> new RedisCache("localhost:6379", new MTProtoTime()))
            .As<IDistributedCache>().SingleInstance();
        builder.Register(_=> new RedisPipe("localhost:6379")).As<IDistributedPipe>();
        //builder.Register(_=> new KafkaPipe("kafka:9092")).As<IDistributedPipe>();
        builder.RegisterType<SerilogLogger>().As<ILogger>().SingleInstance();
        builder.RegisterType<SessionService>().As<ISessionService>().SingleInstance();
        builder.RegisterType<FerriteServer>().As<IFerriteServer>().SingleInstance();
        builder.RegisterType<AuthService>().As<IAuthService>().SingleInstance();
        builder.RegisterType<AccountService>().As<IAccountService>().SingleInstance();
        var container = builder.Build();

        return container;
    }
}
