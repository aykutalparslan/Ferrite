// 
// Project Ferrite is an Implementation of the Telegram Server API
// Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 

using System.Numerics;
using System.Reflection;
using Autofac;
using Ferrite.Core;
using Ferrite.Core.Connection;
using Ferrite.Core.Connection.TransportFeatures;
using Ferrite.Core.Execution;
using Ferrite.Core.Execution.Functions;
using Ferrite.Core.Execution.Functions.Layer146;
using Ferrite.Core.Execution.Functions.Layer146.Help;
using Ferrite.Core.Framing;
using Ferrite.Core.RequestChain;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Data.Repositories;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.ObjectMapper;
using Ferrite.TL.slim;
using Ferrite.TL.slim.layer146;
using Ferrite.Transport;
using Ferrite.Utils;
using InitConnection = Ferrite.TL.InitConnection;

namespace Ferrite;

public class ServerBuilder
{
    public static IFerriteServer BuildServer(string ipAddress, int port)
    {
        IContainer container = BuildContainer(ipAddress, port);

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
        
        return scope.Resolve<IFerriteServer>();
    }
    private static IContainer BuildContainer(string ipAddress, int port)
    {
        var builder = new ContainerBuilder();
        RegisterPrimitives(builder);
        RegisterServices(builder);
        RegisterSchema(builder);
        RegisterCoreComponents(builder);
        RegisterLocalDataStores(builder);
        builder.Register(c=> new DataCenter(ipAddress, port, false))
            .As<IDataCenter>().SingleInstance();
        builder.RegisterType<SerilogLogger>().As<ILogger>().SingleInstance();
        var container = builder.Build();
        return container;
    }

    private static void RegisterPrimitives(ContainerBuilder builder)
    {
        builder.RegisterType<MTProtoTime>().As<IMTProtoTime>().SingleInstance();
        builder.RegisterType<RandomGenerator>().As<IRandomGenerator>().SingleInstance();
        builder.RegisterType<KeyProvider>().As<IKeyProvider>().SingleInstance();
    }

    private static void RegisterLocalDataStores(ContainerBuilder builder)
    {
        builder.Register(_ => new LocalPipe())
            .As<IMessagePipe>().SingleInstance();
        builder.Register(_ => new LocalObjectStore("uploaded-files"))
            .As<IObjectStore>().SingleInstance();
        builder.Register(_ => new LuceneSearchEngine("lucene-index-data"))
            .As<ISearchEngine>().SingleInstance();
        builder.Register(_ => new FasterCounterFactory("faster-counter-data"))
            .As<ICounterFactory>().SingleInstance();
        builder.Register(_ => new FasterUpdatesContextFactory("faster-updates-data"))
            .As<IUpdatesContextFactory>().SingleInstance();
        builder.RegisterType<LocalUnitOfWork>().As<IUnitOfWork>()
            .SingleInstance();
    }

    private static void RegisterCoreComponents(ContainerBuilder builder)
    {
        builder.RegisterType<MTProtoConnection>();
        builder.RegisterType<AuthKeyProcessor>();
        builder.RegisterType<MsgContainerProcessor>();
        builder.RegisterType<ServiceMessagesProcessor>();
        builder.RegisterType<GZipProcessor>();
        builder.RegisterType<AuthorizationProcessor>();
        builder.RegisterType<MTProtoRequestProcessor>();
        builder.RegisterType<DefaultChain>().As<ITLHandler>().SingleInstance();
        RegisterApiLayers(builder);
        builder.RegisterType<ExecutionEngine>().As<IExecutionEngine>().SingleInstance();
        builder.RegisterType<ProtoHandler>().As<IProtoHandler>();
        builder.RegisterType<QuickAckFeature>().As<IQuickAckFeature>().SingleInstance();
        builder.RegisterType<TransportErrorFeature>().As<ITransportErrorFeature>().SingleInstance();
        builder.RegisterType<WebSocketFeature>().As<IWebSocketFeature>();
        builder.RegisterType<ProtoTransport>();
        builder.RegisterType<SerializationFeature>();
        builder.RegisterType<MTProtoSession>().As<IMTProtoSession>();
        builder.RegisterType<MTProtoTransportDetector>().As<ITransportDetector>();
        builder.RegisterType<SocketConnectionListener>().As<IConnectionListener>();
        builder.RegisterType<FerriteServer>().As<IFerriteServer>().SingleInstance();
    }

    private static void RegisterApiLayers(ContainerBuilder builder)
    {
        builder.RegisterType<ReqPQFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(146, Constructors.mtproto_ReqPqMulti))
            .SingleInstance();
        builder.RegisterType<ReqDhParamsFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(146, Constructors.mtproto_ReqDhParams))
            .SingleInstance();
        builder.RegisterType<SetClientDhParamsFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(146, Constructors.mtproto_SetClientDhParams))
            .SingleInstance();
        builder.RegisterType<InitConnectionFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(146, Constructors.layer146_InitConnection))
            .SingleInstance()
            .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
        builder.RegisterType<InvokeWithLayerFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(146, Constructors.layer146_InvokeWithLayer))
            .SingleInstance()
            .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
        builder.RegisterType<GetConfigFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(146, Constructors.layer146_GetConfig))
            .SingleInstance();
    }

    private static void RegisterSchema(ContainerBuilder builder)
    {
        var tl = Assembly.Load("Ferrite.TL");
        var core = Assembly.Load("Ferrite.Core");
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
        builder.Register(_ => new Ferrite.TL.Int128());
        builder.Register(_ => new Int256());
        builder.RegisterType<DefaultMapper>().As<IMapperContext>().SingleInstance();
        builder.RegisterType<TLObjectFactory>().As<ITLObjectFactory>();
    }

    private static void RegisterServices(ContainerBuilder builder)
    {
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
        builder.RegisterType<PhotosService>().As<IPhotosService>()
            .SingleInstance();
        builder.RegisterType<UploadService>().As<IUploadService>()
            .SingleInstance();
        builder.RegisterType<MessagesService>().As<IMessagesService>()
            .SingleInstance();
        builder.RegisterType<SessionService>().As<ISessionService>().SingleInstance();
        builder.RegisterType<AuthService>().As<IAuthService>().SingleInstance();
        builder.RegisterType<AccountService>().As<IAccountService>().SingleInstance();
        builder.RegisterType<SkiaPhotoProcessor>().As<IPhotoProcessor>()
            .SingleInstance();
    }
}