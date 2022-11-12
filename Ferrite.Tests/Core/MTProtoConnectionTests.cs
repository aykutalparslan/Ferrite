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
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using Cassandra;
using DotNext.Buffers;
using DotNext.Collections.Generic;
using Ferrite.Core;
using Ferrite.Core.Connection;
using Ferrite.Core.Connection.TransportFeatures;
using Ferrite.Core.Exceptions;
using Ferrite.Core.Framing;
using Ferrite.Core.RequestChain;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Data.Account;
using Ferrite.Data.Auth;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.currentLayer;
using Ferrite.TL.mtproto;
using Ferrite.TL.ObjectMapper;
using Ferrite.TL.slim;
using Ferrite.Transport;
using Ferrite.Utils;
using MessagePack;
using Moq;
using Moq.AutoMock;
using Xunit;
using MTProtoMessage = Ferrite.Services.MTProtoMessage;

namespace Ferrite.Tests.Core;

class FakeTransportConnection : ITransportConnection
{
    public IDuplexPipe Transport { get; set; }
    public IDuplexPipe Application { get; set; }
    public Pipe Input { get; set; }
    public Pipe Output { get; set; }

    public EndPoint? RemoteEndPoint => new IPEndPoint(IPAddress.Any,13579);

    private string[] _file;

    public FakeTransportConnection(string file = "testdata/obfuscatedIntermediateSession.bin")
    {
        _file = new string[1];
        _file[0] = file;
        Input = new Pipe();
        Output = new Pipe();
        var duplex1 = new Mock<IDuplexPipe>();
        duplex1.SetupGet(x => x.Input).Returns(Input.Reader);
        duplex1.SetupGet(x => x.Output).Returns(Output.Writer);
        var duplex2 = new Mock<IDuplexPipe>();
        duplex2.SetupGet(x => x.Input).Returns(Output.Reader);
        duplex2.SetupGet(x => x.Output).Returns(Input.Writer);
        Transport = duplex1.Object;
        Application = duplex2.Object;
    }
    public FakeTransportConnection(params string[] file)
    {
        _file = file;
        Input = new Pipe();
        Output = new Pipe();
        var duplex1 = new Mock<IDuplexPipe>();
        duplex1.SetupGet(x => x.Input).Returns(Input.Reader);
        duplex1.SetupGet(x => x.Output).Returns(Output.Writer);
        var duplex2 = new Mock<IDuplexPipe>();
        duplex2.SetupGet(x => x.Input).Returns(Output.Reader);
        duplex2.SetupGet(x => x.Output).Returns(Input.Writer);
        Transport = duplex1.Object;
        Application = duplex2.Object;
    }

    public async void Start()
    {
        foreach (var f in _file)
        {
            byte[] data = File.ReadAllBytes(f);
            await Input.Writer.WriteAsync(data);
        }
    }

    public async Task Receive(string file)
    {
        byte[] data = File.ReadAllBytes(file);
        await Input.Writer.WriteAsync(data);
    }

    public void Abort(Exception abortReason)
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}

public class MTProtoConnectionTests
{
    /* this is because of the refactor of the MTProtoConnection class
     [Fact]
    public void ReceivesUnencryptedMessages()
    {
        var container = BuildIoCContainer();
        ITransportConnection connection = new FakeTransportConnection();
        connection.Start();
        MTProtoConnection mtProtoConnection = container.Resolve<MTProtoConnection>(new NamedParameter("connection", connection));
        List<ITLObject> received = new List<ITLObject>();
        var sess = new Dictionary<string, object>();
        mtProtoConnection.MessageReceived += async (s,e) => {
            received.Add(e.Message);
        };
        mtProtoConnection.Start();
        Assert.IsType<ReqPqMulti>(received[0]);
        Assert.IsType<ReqDhParams>(received[1]);
        Assert.IsType<SetClientDhParams>(received[2]);
    }*/

    [Fact]
    public void ReceivesMessagesFromWebSocket()
    {
        var builder = GetContainerBuilder();
        List<ITLObject> received = new List<ITLObject>();
        var processor = new Mock<ITLHandler>();
        processor.Setup(p =>
            p.Process(It.IsAny<object?>(),
                It.IsAny<ITLObject>(), 
                It.IsAny<TLExecutionContext>())).Callback((object? sender, 
            ITLObject input, TLExecutionContext ctx) =>
        {
            received.Add(input);
        });
        builder.RegisterMock(processor);
        var container = builder.Build();
        ITransportConnection connection = new FakeTransportConnection("testdata/websocketSession.bin");
        connection.Start();
        MTProtoConnection mtProtoConnection = container.Resolve<MTProtoConnection>(new NamedParameter("connection", connection));

        mtProtoConnection.Start();

        Assert.IsType<InvokeWithLayer>(received[0]);
        Assert.IsType<MsgsAck>(received[1]);
        Assert.IsType<PingDelayDisconnect>(received[2]);
    }

    [Fact]
    public async Task SendsWebSocketHeader()
    {
        var builder = GetContainerBuilder();
        List<ITLObject> received = new List<ITLObject>();
        var processor = new Mock<ITLHandler>();
        processor.Setup(p =>
            p.Process(It.IsAny<object?>(),
                It.IsAny<ITLObject>(), 
                It.IsAny<TLExecutionContext>())).Callback((object? sender, 
            ITLObject input, TLExecutionContext ctx) =>
        {
            received.Add(input);
        });
        builder.RegisterMock(processor);
        var container = builder.Build();
        FakeTransportConnection connection = new FakeTransportConnection("testdata/websocketSession_plain");
        connection.Start();
        MTProtoConnection mtProtoConnection = container.Resolve<MTProtoConnection>(new NamedParameter("connection", connection));
        mtProtoConnection.Start();
        var webSocketResult = await connection.Application.Input.ReadAsync();
        string wsExpected = "HTTP/1.1 101 Switching Protocols\r\nConnection: upgrade\r\nUpgrade: websocket\r\nSec-WebSocket-Accept: AJivmihbVG1JSXhoiKaZkpv82+s=\r\nSec-WebSocket-Protocol: binary\r\n\r\n";

        var slice = webSocketResult.Buffer.Slice(0, wsExpected.Length);
        string webSocketResponse = Encoding.UTF8.GetString(slice);
        connection.Application.Input.AdvanceTo(slice.End);
        Assert.Equal(wsExpected, webSocketResponse);
    }


    [Fact]
    public async Task SendsUnencryptedMessages()
    {
        var builder = GetContainerBuilder();
        List<ITLObject> received = new List<ITLObject>();
        var processor = new Mock<ITLHandler>();
        processor.Setup(p =>
            p.Process(It.IsAny<object?>(),
                It.IsAny<ITLObject>(), 
                It.IsAny<TLExecutionContext>())).Callback((object? sender, 
            ITLObject input, TLExecutionContext ctx) =>
        {
            received.Add(input);
        });
        builder.RegisterMock(processor);
        var container = builder.Build();
        FakeTransportConnection connection = new FakeTransportConnection("testdata/websocketSession_plain");
        connection.Start();
        MTProtoConnection mtProtoConnection = container.Resolve<MTProtoConnection>(new NamedParameter("connection", connection));
        
        mtProtoConnection.Start();
        var webSocketResult = await connection.Application.Input.ReadAsync();
        connection.Application.Input.AdvanceTo(webSocketResult.Buffer.End);
        
        byte[] data = File.ReadAllBytes("testdata/message_0");
        byte[] expected = File.ReadAllBytes("testdata/sent_0");
        var message = MessagePackSerializer.Deserialize<MTProtoMessage>(data);
        await mtProtoConnection.SendAsync(message);
        var result = await connection.Application.Input.ReadAsync();
        var actual = result.Buffer.ToSpan().Slice(2).ToArray();
        Assert.Equal(expected, actual);
        connection.Application.Input.AdvanceTo(result.Buffer.End);

        data = File.ReadAllBytes("testdata/message_1");
        expected = File.ReadAllBytes("testdata/sent_1");
        message = MessagePackSerializer.Deserialize<MTProtoMessage>(data);
        await mtProtoConnection.SendAsync(message);
        result = await connection.Application.Input.ReadAsync();
        Assert.Equal(expected, result.Buffer.ToSpan().Slice(4).ToArray());
        connection.Application.Input.AdvanceTo(result.Buffer.End);

        data = File.ReadAllBytes("testdata/message_2");
        expected = File.ReadAllBytes("testdata/sent_2");
        message = MessagePackSerializer.Deserialize<MTProtoMessage>(data);
        await mtProtoConnection.SendAsync(message);
        result = await connection.Application.Input.ReadAsync();
        Assert.Equal(expected, result.Buffer.ToSpan().Slice(2).ToArray());
        connection.Application.Input.AdvanceTo(result.Buffer.End);
    }

    [Theory]
    [InlineData(16)]
    [InlineData(256)]
    [InlineData(512)]
    [InlineData(4096)]
    public async Task SendsEncryptedMessage(int len)
    {
        using var mocker = AutoMock.GetLoose((builder) =>
        {
            builder.RegisterType<ProtoHandler>().As<IProtoHandler>();
            builder.RegisterType<RandomGenerator>().As<IRandomGenerator>();
        });
        var transport = mocker.Mock<ITransportConnection>();
        var pipe = new Pipe();
        transport.Setup(t => t.Write(It.IsAny<ReadOnlySequence<byte>>()))
            .Callback((ReadOnlySequence<byte> buffer) =>
            {
                pipe.Writer.Write(buffer.ToArray());
            });
        transport.Setup(t => t.FlushAsync())
            .Returns(() => pipe.Writer.FlushAsync());
        var session = mocker.Mock<IMTProtoSession>();
        var authKey = new byte[192];
        Random.Shared.NextBytes(authKey);
        session.SetupGet(s => s.AuthKeyId).Returns(() => Random.Shared.NextInt64());
        session.SetupGet(s => s.AuthKey).Returns(() => authKey);
        session.SetupGet(s => s.ServerSalt).Returns(() => new ServerSaltDTO());
        var connection = mocker.Create<MTProtoConnection>();
        var expected = new byte[len];
        Random.Shared.NextBytes(expected);
        MTProtoMessage message = new()
        {
            Data = expected,
            MessageType = MTProtoMessageType.Encrypted,
        };
        connection.Start();
        await connection.SendAsync(message);
        var result = await pipe.Reader.ReadAsync();
        var actual = DecryptMessage(result.Buffer.Slice(8), authKey).AsSpan(32,len).ToArray();
        Assert.Equal(expected, actual);
    }
    [Theory]
    [InlineData(8192)]
    [InlineData(16384)]
    [InlineData(32168)]
    [InlineData(65536)]
    public async Task SendsStream(int len)
    {
        using var mocker = AutoMock.GetLoose((builder) =>
        {
            builder.RegisterType<ProtoHandler>().As<IProtoHandler>();
            builder.RegisterType<RandomGenerator>().As<IRandomGenerator>();
        });
        var transport = mocker.Mock<ITransportConnection>();
        var pipe = new Pipe();
        transport.Setup(t => t.Write(It.IsAny<ReadOnlySequence<byte>>()))
            .Callback((ReadOnlySequence<byte> buffer) =>
            {
                pipe.Writer.Write(buffer.ToArray());
            });
        transport.Setup(t => t.FlushAsync())
            .Returns(() => pipe.Writer.FlushAsync());
        var session = mocker.Mock<IMTProtoSession>();
        var authKey = new byte[192];
        Random.Shared.NextBytes(authKey);
        session.SetupGet(s => s.AuthKeyId).Returns(() => Random.Shared.NextInt64());
        session.SetupGet(s => s.AuthKey).Returns(() => authKey);
        session.SetupGet(s => s.ServerSalt).Returns(() => new ServerSaltDTO());
        var connection = mocker.Create<MTProtoConnection>();
        var expected = new byte[len];
        Random.Shared.NextBytes(expected);
        var file = new Mock<IFileOwner>();
        file.Setup(f => f.GetFileStream())
            .ReturnsAsync(() => new MemoryStream(expected));
        connection.Start();
        await connection.SendAsync(file.Object);
        int fileHeaderLen = 24 + (len < 254 ? 1 : 4);
        var result = await pipe.Reader.ReadAtLeastAsync(56 + len + fileHeaderLen);
        var decrypted = DecryptMessage(result.Buffer.Slice(8), authKey);
        var actual = decrypted.AsSpan(32 + fileHeaderLen,len).ToArray();
        Assert.Equal(expected, actual);
    }
    private byte[] DecryptMessage(ReadOnlySequence<byte> bytes, byte[] authKey)
    {
        if (bytes.Length < 16)
        {
            throw new ArgumentOutOfRangeException();
        }
        Span<byte> messageKey = stackalloc byte[16];
        bytes.Slice(0, 16).CopyTo(messageKey);
        AesIge aesIge = new(authKey, messageKey, false);
        var messageData = new byte[(int)bytes.Length - 16];
        bytes.Slice(16).CopyTo(messageData);
        aesIge.Decrypt(messageData);
        return messageData;
    }
    private ContainerBuilder GetContainerBuilder()
    {
        ConcurrentQueue<byte[]> _channel = new();
        var pipe = new Mock<IMessagePipe>();
        pipe.Setup(x => x.WriteMessageAsync(It.IsAny<string>(), It.IsAny<byte[]>())).ReturnsAsync((string a, byte[] b) =>
        {
            _channel.Enqueue(b);
            return true;
        });
        pipe.Setup(x => x.ReadMessageAsync(default)).Returns(() =>
        {
            _channel.TryDequeue(out var result);
            return ValueTask.FromResult(result!);
        });
        var logger = new Mock<ILogger>();
        Dictionary<long, byte[]> authKeys = new Dictionary<long, byte[]>();
        Dictionary<long, byte[]> sessions = new Dictionary<long, byte[]>();
        byte[] key = File.ReadAllBytes("testdata/authKey_1508830554984586608");
        authKeys.Add(1508830554984586608, key);
        key = File.ReadAllBytes("testdata/authKey_-12783902225236342");
        authKeys.Add(-12783902225236342, key);
        var proto = new Mock<IMTProtoService>();
        proto.Setup(x => x.PutAuthKeyAsync(It.IsAny<long>(), It.IsAny<byte[]>())).ReturnsAsync((long a, byte[] b) =>
        {
            authKeys.Add(a, b);
            return true;
        });
        proto.Setup(x => x.GetAuthKey(It.IsAny<long>())).Returns((long a) =>
        {
            if (!authKeys.ContainsKey(a))
            {
                return new byte[0];
            }
            return authKeys[a];
        });
        proto.Setup(x => x.GetAuthKeyAsync(It.IsAny<long>())).ReturnsAsync((long a) =>
        {
            if (!authKeys.ContainsKey(a))
            {
                return new byte[0];
            }
            return authKeys[a];
        });
        Dictionary<long, byte[]> authKeys2 = new Dictionary<long, byte[]>();
        Queue<long> unixTimes = new Queue<long>();
        var time = new Mock<IMTProtoTime>();
        unixTimes.Enqueue(1649323587);
        unixTimes.Enqueue(1649323588);
        unixTimes.Enqueue(1649323588);
        unixTimes.Enqueue(1649323588);
        time.SetupGet(x => x.FiveMinutesAgo).Returns(long.MinValue);
        time.SetupGet(x => x.ThirtySecondsLater).Returns(long.MaxValue);
        time.Setup(x => x.GetUnixTimeInSeconds()).Returns(() => unixTimes.Dequeue());
        int rangeEnd = RandomNumberGenerator.GetInt32(int.MaxValue / 4 * 3, int.MaxValue);
        var generatedPrimes = RandomGenerator.SieveOfEratosthenesSegmented(rangeEnd - 5000000, rangeEnd);
        var random = new Mock<IRandomGenerator>();
        RandomGenerator rnd = new RandomGenerator();
        random.Setup(x => x.GetNext(It.IsAny<int>(),It.IsAny<int>()))
            .Returns(()=> 381);
        random.Setup(x => x.GetRandomBytes(It.IsAny<int>()))
            .Returns((int count) =>
            {
                if(count == 16)
                {
                    return new byte[]
                    {
                        178, 121,62,117,215,188,141,152,36,193,57,227,183,151,131,37
                    };
                }
                return File.ReadAllBytes("testdata/randomBytes_0");
            });
        random.Setup(x => x.GetRandomInteger(It.IsAny<BigInteger>(),It.IsAny<BigInteger>()))
            .Returns((int a, int b)=> rnd.GetRandomInteger(a,b));
        random.Setup(x => x.GetRandomNumber(It.IsAny<int>()))
            .Returns((int a)=> rnd.GetRandomNumber(a));
        random.Setup(x => x.GetRandomNumber(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int a, int b)=> rnd.GetRandomNumber(a, b));
        random.Setup(x => x.GetRandomPrime())
            .Returns(()=> rnd.GetRandomPrime());
        Dictionary<Ferrite.TL.Int128, byte[]> _authKeySessionStates = new(); 
        Dictionary<Ferrite.TL.Int128, ActiveSession> _authKeySessions = new();
        var sessionManager = new Mock<ISessionService>();
        sessionManager.SetupGet(x => x.NodeId).Returns(Guid.NewGuid());
        sessionManager.Setup(x => x.AddAuthSessionAsync(It.IsAny<byte[]>(),
            It.IsAny<AuthSessionState>(), It.IsAny<ActiveSession>())).ReturnsAsync(
            (byte[] nonce, AuthSessionState state, ActiveSession session) =>
        {
            var stateBytes = MessagePackSerializer.Serialize(state);
            _authKeySessions.Add((Ferrite.TL.Int128)nonce, session);
            _authKeySessionStates.Add((Ferrite.TL.Int128)nonce, stateBytes);
            return true;
        });
        sessionManager.Setup(x => x.AddSessionAsync(It.IsAny<long>(), It.IsAny<long>(),
            It.IsAny<ActiveSession>())).ReturnsAsync(() => true);
        sessionManager.Setup(x => x.GetAuthSessionStateAsync(It.IsAny<byte[]>())).ReturnsAsync((byte[] nonce) =>
        {
            var rawSession = _authKeySessionStates[(Ferrite.TL.Int128)nonce];
            if (rawSession != null)
            {
                var state = MessagePackSerializer.Deserialize<AuthSessionState>(rawSession);

                return state;
            }
            return null;
        });
        sessionManager.Setup(x => x.GetSessionStateAsync(It.IsAny<long>())).ReturnsAsync((long sessionId) =>
        {
            var data = File.ReadAllBytes("testdata/sessionState");
            return MessagePackSerializer.Deserialize<RemoteSession>(data);
        });
        sessionManager.Setup(x => x.UpdateAuthSessionAsync(It.IsAny<byte[]>(), It.IsAny<AuthSessionState>()))
            .ReturnsAsync(
                (byte[] nonce, AuthSessionState state) =>
                {
                    _authKeySessionStates.Remove((Ferrite.TL.Int128)nonce);
                    _authKeySessionStates.Add((Ferrite.TL.Int128)nonce, MessagePackSerializer.Serialize(state));
                    return true;
                });
        var tl = Assembly.Load("Ferrite.TL");
        var builder = new ContainerBuilder();
        builder.RegisterMock(time);
        builder.RegisterMock(random);
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
            .Where(t => t.Namespace != null && t.Namespace.StartsWith("Ferrite.TL.currentLayer"))
            .AsSelf();
        builder.Register(_ => new Ferrite.TL.Int128());
        builder.Register(_ => new Int256());
        builder.RegisterType<MTProtoConnection>();
        builder.RegisterType<TLObjectFactory>().As<ITLObjectFactory>();
        builder.RegisterType<DefaultMapper>().As<IMapperContext>();
        builder.RegisterType<MTProtoTransportDetector>().As<ITransportDetector>();
        builder.RegisterType<SocketConnectionListener>().As<IConnectionListener>();
        builder.RegisterMock(proto);
        builder.RegisterMock(logger);
        builder.RegisterMock(sessionManager);
        builder.RegisterType<ProtoHandler>().As<IProtoHandler>();
        builder.RegisterType<QuickAckFeature>().As<IQuickAckFeature>().SingleInstance();
        builder.RegisterType<TransportErrorFeature>().As<ITransportErrorFeature>().SingleInstance();
        builder.RegisterType<WebSocketFeature>().As<IWebSocketFeature>();
        builder.RegisterType<ProtoTransport>();
        builder.RegisterType<SerializationFeature>();
        builder.RegisterType<MTProtoSession>().As<IMTProtoSession>();
        builder.RegisterMock(pipe);
        builder.RegisterMock(new Mock<IAuthService>());
        return builder;
    }

}

