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
using Ferrite.Core.Execution.Functions.BaseLayer;
using Ferrite.Core.Execution.Functions.BaseLayer.Account;
using Ferrite.Core.Execution.Functions.BaseLayer.Auth;
using Ferrite.Core.Execution.Functions.BaseLayer.Contacts;
using Ferrite.Core.Execution.Functions.BaseLayer.Help;
using Ferrite.Core.Execution.Functions.BaseLayer.Users;
using Ferrite.Core.Framing;
using Ferrite.Core.RequestChain;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Data.Repositories;
using Ferrite.Services;
using Ferrite.Services.Gateway;
using Ferrite.TL;
using Ferrite.TL.ObjectMapper;
using Ferrite.TL.slim;
using Ferrite.TL.slim.layer150.account;
using Ferrite.Transport;
using Ferrite.Utils;

namespace Ferrite;

public class ServerBuilder
{
    private const int DefaultLayer = 150;
    public static IFerriteServer BuildServer(string ipAddress, int port, string path = "data")
    {
        IContainer container = BuildContainer(ipAddress, port, path);

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
    private static IContainer BuildContainer(string ipAddress, int port, string path)
    {
        var builder = new ContainerBuilder();
        RegisterPrimitives(builder);
        RegisterServices(builder);
        RegisterSchema(builder);
        RegisterCoreComponents(builder);
        RegisterLocalDataStores(builder, path);
        builder.Register(c=> new DataCenter(1, ipAddress, port, false))
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

    private static void RegisterLocalDataStores(ContainerBuilder builder, string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        builder.Register(_ => new LocalPipe())
            .As<IMessagePipe>().SingleInstance();
        builder.Register(_ => new LocalObjectStore(Path.Combine(path, "uploaded-files")))
            .As<IObjectStore>().SingleInstance();
        builder.Register(_ => new LuceneSearchEngine(Path.Combine(path, "lucene-index-data")))
            .As<ISearchEngine>().SingleInstance();
        builder.Register(_ => new FasterCounterFactory(Path.Combine(path, "faster-counter-data")))
            .As<ICounterFactory>().SingleInstance();
        builder.Register(_ => new FasterUpdatesContextFactory(Path.Combine(path,"faster-updates-data")))
            .As<IUpdatesContextFactory>().SingleInstance();
        builder.Register(_ => new LocalUnitOfWork(Path.Combine(path, "rocksdb-data")))
            .As<IUnitOfWork>().SingleInstance();
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
        RegisterMTProtoMethods(builder);
        RegisterBaseMethods(builder);
        RegisterHelpMethods(builder);
        RegisterAuthMethods(builder);
        RegisterAccountMethods(builder);
        RegisterUsersMethods(builder);
        RegisterContactsMethods(builder);
    }

    private static void RegisterContactsMethods(ContainerBuilder builder)
    {
        builder.RegisterType<GetStatusesFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_GetStatuses))
            .SingleInstance();
        builder.RegisterType<GetContactsFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_GetContacts))
            .SingleInstance();
        builder.RegisterType<ImportContactsFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_ImportContacts))
            .SingleInstance();
        builder.RegisterType<DeleteContactsFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_DeleteContacts))
            .SingleInstance();
        builder.RegisterType<DeleteByPhonesFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_DeleteByPhones))
            .SingleInstance();
        builder.RegisterType<BlockFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_Block))
            .SingleInstance();
        builder.RegisterType<UnblockFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_Unblock))
            .SingleInstance();
    }

    private static void RegisterUsersMethods(ContainerBuilder builder)
    {
        builder.RegisterType<GetUsersFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_GetUsers))
            .SingleInstance();
        builder.RegisterType<GetFullUserFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_GetFullUser))
            .SingleInstance();
    }

    private static void RegisterAccountMethods(ContainerBuilder builder)
    {
        builder.RegisterType<RegisterDeviceFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_RegisterDevice))
            .SingleInstance();
        builder.RegisterType<RegisterDeviceL57Func>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_RegisterDeviceL57))
            .SingleInstance();
        builder.RegisterType<UnregisterDeviceFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_UnregisterDevice))
            .SingleInstance();
        builder.RegisterType<UpdateNotifySettingsFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_AccountUpdateNotifySettings))
            .SingleInstance();
        builder.RegisterType<GetNotifySettingsFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_GetNotifySettings))
            .SingleInstance();
        builder.RegisterType<ResetNotifySettingsFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_ResetNotifySettings))
            .SingleInstance();
        builder.RegisterType<UpdateProfileFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_UpdateProfile))
            .SingleInstance();
        builder.RegisterType<UpdateStatusFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_UpdateStatus))
            .SingleInstance();
        builder.RegisterType<ReportPeerFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_ReportPeer))
            .SingleInstance();
        builder.RegisterType<CheckUsernameFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_AccountCheckUsername))
            .SingleInstance();
        builder.RegisterType<UpdateUsernameFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_AccountUpdateUsername))
            .SingleInstance();
        builder.RegisterType<SetPrivacyFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_SetPrivacy))
            .SingleInstance();
        builder.RegisterType<GetPrivacyFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_GetPrivacy))
            .SingleInstance();
        builder.RegisterType<DeleteAccountFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_DeleteAccount))
            .SingleInstance();
        builder.RegisterType<SetAccountTtlFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_SetAccountTTL))
            .SingleInstance();
        builder.RegisterType<GetAccountTtlFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_GetAccountTTL))
            .SingleInstance();
        builder.RegisterType<SendChangePhoneCodeFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_SendChangePhoneCode))
            .SingleInstance();
        builder.RegisterType<ChangePhoneFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_ChangePhone))
            .SingleInstance();
        builder.RegisterType<UpdateDeviceLockedFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_UpdateDeviceLocked))
            .SingleInstance();
        builder.RegisterType<GetAuthorizationsFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_GetAuthorizations))
            .SingleInstance();
        builder.RegisterType<ResetAuthorizationFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_ResetAuthorization))
            .SingleInstance();
        builder.RegisterType<SetContactSignUpNotificationFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_SetContactSignUpNotification))
            .SingleInstance();
        builder.RegisterType<GetContactSignUpNotificationFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_GetContactSignUpNotification))
            .SingleInstance();
        builder.RegisterType<ChangeAuthorizationSettingsFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_ChangeAuthorizationSettings))
            .SingleInstance();
    }

    private static void RegisterAuthMethods(ContainerBuilder builder)
    {
        builder.RegisterType<SendCodeFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_SendCode))
            .SingleInstance();
        builder.RegisterType<ResendCodeFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_ResendCode))
            .SingleInstance();
        builder.RegisterType<CancelCodeFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_CancelCode))
            .SingleInstance();
        builder.RegisterType<SignUpFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_SignUp))
            .SingleInstance();
        builder.RegisterType<SignInFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_SignIn))
            .SingleInstance();
        builder.RegisterType<BindTempAuthKeyFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_BindTempAuthKey))
            .SingleInstance();
        builder.RegisterType<LogOutFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_LogOut))
            .SingleInstance();
        builder.RegisterType<DropTempAuthKeysFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_DropTempAuthKeys))
            .SingleInstance();
        builder.RegisterType<ResetAuthorizationsFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_ResetAuthorizations))
            .SingleInstance();
        builder.RegisterType<ExportLoginTokenFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_ExportLoginToken))
            .SingleInstance();
        builder.RegisterType<ExportAuthorizationFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_ExportAuthorization))
            .SingleInstance();
        builder.RegisterType<ImportAuthorizationFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_ImportAuthorization))
            .SingleInstance();
    }

    private static void RegisterHelpMethods(ContainerBuilder builder)
    {
        builder.RegisterType<GetConfigFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_GetConfig))
            .SingleInstance();
    }

    private static void RegisterBaseMethods(ContainerBuilder builder)
    {
        builder.RegisterType<InitConnectionFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_InitConnection))
            .SingleInstance()
            .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
        builder.RegisterType<InvokeWithLayerFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.layer150_InvokeWithLayer))
            .SingleInstance()
            .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
    }

    private static void RegisterMTProtoMethods(ContainerBuilder builder)
    {
        builder.RegisterType<ReqPQFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.mtproto_ReqPqMulti))
            .SingleInstance();
        builder.RegisterType<ReqDhParamsFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.mtproto_ReqDhParams))
            .SingleInstance();
        builder.RegisterType<SetClientDhParamsFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.mtproto_SetClientDhParams))
            .SingleInstance();
        builder.RegisterType<MsgsAckFunc>()
            .Keyed<ITLFunction>(
                new FunctionKey(DefaultLayer, Constructors.mtproto_MsgsAck))
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
        builder.RegisterType<VerificationGateway>()
            .As<IVerificationGateway>().SingleInstance();
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