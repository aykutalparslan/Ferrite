//
//  Project Ferrite is an Implementation Telegram Server API
//  Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Affero General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Affero General Public License for more details.
//
//  You should have received a copy of the GNU Affero General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Ferrite.Core;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.mtproto;
using Ferrite.Transport;
using Ferrite.Utils;
using Xunit;

namespace Ferrite.Tests.Core;

class MockTime : IMTProtoTime
{
    public long FiveMinutesAgo => long.MinValue;

    public long ThirtySecondsLater => long.MaxValue;
}
class MockRedis : IDistributedStore
{
    public MockRedis()
    {
        byte[] key = File.ReadAllBytes("testdata/authKey_1508830554984586608");
        authKeys.Add(1508830554984586608, key);
    }
    Dictionary<long, byte[]> authKeys = new Dictionary<long, byte[]>();
    Dictionary<long, byte[]> sessions = new Dictionary<long, byte[]>();
    public async Task<byte[]> GetAuthKeyAsync(long authKeyId)
    {
        if (!authKeys.ContainsKey(authKeyId))
        {
            return null;
        }
        return authKeys[authKeyId];
    }

    public async Task<byte[]> GetSessionAsync(long sessionId)
    {
        if (!sessions.ContainsKey(sessionId))
        {
            return null;
        }
        return sessions[sessionId];
    }

    public async Task<bool> PutAuthKeyAsync(long authKeyId, byte[] authKey)
    {
        authKeys.Add(authKeyId, authKey);
        return true;
    }

    public async Task<bool> PutSessionAsync(long sessionId, byte[] sessionData)
    {
        sessions.Add(sessionId, sessionData);
        return true;
    }

    public async Task<bool> RemoveSessionAsync(long sessionId)
    {
        sessions.Remove(sessionId);
        return false;
    }
}
class MockCassandra : IPersistentStore
{
    Dictionary<long, byte[]> authKeys = new Dictionary<long, byte[]>();
    public async Task<byte[]?> GetAuthKeyAsync(long authKeyId)
    {
        if (!authKeys.ContainsKey(authKeyId))
        {
            return null;
        }
        return authKeys[authKeyId];
    }

    public async Task SaveAuthKeyAsync(long authKeyId, byte[] authKey)
    {
        authKeys.Add(authKeyId, authKey);
    }
}
class MockDuplexPipe : IDuplexPipe
{
    public MockDuplexPipe(PipeReader reader, PipeWriter writer)
    {
        Input = reader;
        Output = writer;
    }

    public PipeReader Input { get; }

    public PipeWriter Output { get; }
}
class MockTransportConnection : ITransportConnection
{
    public IDuplexPipe Transport { get; set; }
    public Pipe Input { get; set; }
    public Pipe Output { get; set; }
    private string _file;

    public MockTransportConnection(string file = "testdata/obfuscatedIntermediateSession.bin")
    {
        _file = file;
        Input = new Pipe();
        Output = new Pipe();
        Transport = new MockDuplexPipe(Input.Reader, Output.Writer);
    }

    public void Start()
    {
        byte[] data = File.ReadAllBytes(_file);
        Input.Writer.WriteAsync(data);
        Input.Writer.FlushAsync();
    }
}

public class MTProtoConnectionTests
{
    [Fact]
    public void ReceivesUnencryptedMessages()
    {
        var container = BuildIoCContainer();
        ITransportConnection connection = new MockTransportConnection();
        connection.Start();
        MTProtoConnection mtProtoConnection = container.Resolve<MTProtoConnection>(new NamedParameter("connection", connection));
        List<ITLObject> received = new List<ITLObject>();
        var sess = new Dictionary<string, object>();
        mtProtoConnection.MessageReceived += async (s,e) => {
            received.Add(e.Message);
            await ((ITLMethod)e.Message).ExecuteAsync(new TLExecutionContext(sess));
        };
        mtProtoConnection.Start();
        Assert.IsType<ReqPqMulti>(received[0]);
        Assert.IsType<ReqDhParams>(received[1]);
        Assert.IsType<SetClientDhParams>(received[2]);
    }

    [Fact]
    public void ReceivesMessagesFromWebSocket()
    {
        var container = BuildIoCContainer();
        ITransportConnection connection = new MockTransportConnection("testdata/websocketSession.bin");
        connection.Start();
        MTProtoConnection mtProtoConnection = container.Resolve<MTProtoConnection>(new NamedParameter("connection", connection));
        List<ITLObject> received = new List<ITLObject>();
        var sess = new Dictionary<string, object>();
        mtProtoConnection.MessageReceived += async (s, e) => {
            received.Add(e.Message);
            await ((ITLMethod)e.Message).ExecuteAsync(new TLExecutionContext(sess));
        };
        mtProtoConnection.Start();
        Assert.IsType<ReqPqMulti>(received[0]);
        Assert.IsType<ReqDhParams>(received[1]);
        Assert.IsType<SetClientDhParams>(received[2]);
        Assert.IsType<Ferrite.TL.layer139.InvokeWithLayer>(received[3]);
        Assert.IsType<Ferrite.TL.layer139.updates.GetState>(received[4]);
        Assert.IsType<Ferrite.TL.mtproto.MsgsAck>(received[5]);
        Assert.IsType<Ferrite.TL.mtproto.MsgContainer>(received[6]);
        Assert.IsType<Ferrite.TL.mtproto.PingDelayDisconnect>(received[7]);
    }

    private IContainer BuildIoCContainer()
    {
        var tl = Assembly.Load("Ferrite.TL");
        var builder = new ContainerBuilder();
        builder.RegisterType<MockTime>().As<IMTProtoTime>().SingleInstance();
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
        builder.RegisterType<TLObjectFactory>().As<ITLObjectFactory>();
        builder.RegisterType<MTProtoTransportDetector>().As<ITransportDetector>();
        builder.RegisterType<SocketConnectionListener>().As<IConnectionListener>();
        builder.RegisterType<MockCassandra>().As<IPersistentStore>().SingleInstance();
        builder.RegisterType<MockRedis>().As<IDistributedStore>().SingleInstance();
        builder.RegisterType<SerilogLogger>().As<ILogger>().SingleInstance();
        builder.RegisterType<SessionManager>().As<ISessionManager>().SingleInstance();

        var container = builder.Build();

        return container;
    }

}

