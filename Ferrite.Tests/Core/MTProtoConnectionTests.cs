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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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
using MessagePack;
using Xunit;

namespace Ferrite.Tests.Core;

class FakeTime : IMTProtoTime
{
    public long FiveMinutesAgo => long.MinValue;

    public long ThirtySecondsLater => long.MaxValue;
    private Queue<long> unixTimes = new Queue<long>();
    public FakeTime()
    {
        unixTimes.Enqueue(1649323587);
        unixTimes.Enqueue(1649323588);
        unixTimes.Enqueue(1649323588);
        unixTimes.Enqueue(1649323588);
    }
    public long GetUnixTimeInSeconds()
    {
        return unixTimes.Dequeue();
    }
}
class FakeRandom : IRandomGenerator
{
    private int[] generatedPrimes;
    public FakeRandom()
    {
        int rangeEnd = RandomNumberGenerator.GetInt32(int.MaxValue / 4 * 3, int.MaxValue);
        generatedPrimes = RandomGenerator.SieveOfEratosthenesSegmented(rangeEnd - 5000000, rangeEnd);
    }
    public void Fill(Span<byte> data)
    {
        throw new NotImplementedException();
    }

    public int GetNext(int fromInclusive, int toExclusive)
    {
        return 381;
    }

    public byte[] GetRandomBytes(int count)
    {
        if(count == 16)
        {
            return new byte[]
            {
                178, 121,62,117,215,188,141,152,36,193,57,227,183,151,131,37
            };
        }
        return File.ReadAllBytes("testdata/randomBytes_0");
    }

    public BigInteger GetRandomInteger(BigInteger min, BigInteger max)
    {
        RandomNumberGenerator gen = RandomNumberGenerator.Create();
        return RandomInRange(gen, min, max);
    }

    // Implementation was taken from
    // https://stackoverflow.com/a/48855115/2015348
    private static BigInteger RandomInRange(RandomNumberGenerator rng, BigInteger min, BigInteger max)
    {
        if (min > max)
        {
            var buff = min;
            min = max;
            max = buff;
        }

        // offset to set min = 0
        BigInteger offset = -min;
        min = 0;
        max += offset;

        var value = randomInRangeFromZeroToPositive(rng, max) - offset;
        return value;
    }

    private static BigInteger randomInRangeFromZeroToPositive(RandomNumberGenerator rng, BigInteger max)
    {
        BigInteger value;
        var bytes = max.ToByteArray();

        // count how many bits of the most significant byte are 0
        // NOTE: sign bit is always 0 because `max` must always be positive
        byte zeroBitsMask = 0b00000000;

        var mostSignificantByte = bytes[bytes.Length - 1];

        // we try to set to 0 as many bits as there are in the most significant byte, starting from the left (most significant bits first)
        // NOTE: `i` starts from 7 because the sign bit is always 0
        for (var i = 7; i >= 0; i--)
        {
            // we keep iterating until we find the most significant non-0 bit
            if ((mostSignificantByte & (0b1 << i)) != 0)
            {
                var zeroBits = 7 - i;
                zeroBitsMask = (byte)(0b11111111 >> zeroBits);
                break;
            }
        }

        do
        {
            rng.GetBytes(bytes);

            // set most significant bits to 0 (because `value > max` if any of these bits is 1)
            bytes[bytes.Length - 1] &= zeroBitsMask;

            value = new BigInteger(bytes);

            // `value > max` 50% of the times, in which case the fastest way to keep the distribution uniform is to try again
        } while (value > max);

        return value;
    }

    public int GetRandomNumber(int toExclusive)
    {
        return RandomNumberGenerator.GetInt32(toExclusive);
    }

    public int GetRandomNumber(int fromInclusive, int toExclusive)
    {
        return RandomNumberGenerator.GetInt32(fromInclusive, toExclusive);
    }

    public int GetRandomPrime()
    {
        int rnd = RandomNumberGenerator.GetInt32(generatedPrimes.Length);
        return generatedPrimes[rnd];
    }
}
class FakeRedis : IDistributedStore
{
    public FakeRedis()
    {
        byte[] key = File.ReadAllBytes("testdata/authKey_1508830554984586608");
        authKeys.Add(1508830554984586608, key);
        key = File.ReadAllBytes("testdata/authKey_-12783902225236342");
        authKeys.Add(-12783902225236342, key);
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

    public Task<byte[]> GetPhoneCodeAsync(string phoneNumber, string phoneCodeHash)
    {
        throw new NotImplementedException();
    }

    public Task<bool> PutPhoneCodeAsync(string phoneNumber, string phoneCodeHash, string phoneCode, TimeSpan expiresIn)
    {
        throw new NotImplementedException();
    }

    public Task<byte[]> GetAuthKeySessionAsync(byte[] nonce)
    {
        throw new NotImplementedException();
    }

    public Task<bool> PutAuthKeySessionAsync(byte[] nonce, byte[] sessionData)
    {
        throw new NotImplementedException();
    }
}
class FakeCassandra : IPersistentStore
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
class FakeDuplexPipe : IDuplexPipe
{
    public FakeDuplexPipe(PipeReader reader, PipeWriter writer)
    {
        Input = reader;
        Output = writer;
    }

    public PipeReader Input { get; }

    public PipeWriter Output { get; }
}
class FakeTransportConnection : ITransportConnection
{
    public IDuplexPipe Transport { get; set; }
    public IDuplexPipe Application { get; set; }
    public Pipe Input { get; set; }
    public Pipe Output { get; set; }
    private string[] _file;

    public FakeTransportConnection(string file = "testdata/obfuscatedIntermediateSession.bin")
    {
        _file = new string[1];
        _file[0] = file;
        Input = new Pipe();
        Output = new Pipe();
        Transport = new FakeDuplexPipe(Input.Reader, Output.Writer);
        Application = new FakeDuplexPipe(Output.Reader, Input.Writer);
    }
    public FakeTransportConnection(params string[] file)
    {
        _file = file;
        Input = new Pipe();
        Output = new Pipe();
        Transport = new FakeDuplexPipe(Input.Reader, Output.Writer);
        Application = new FakeDuplexPipe(Output.Reader, Input.Writer);
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
class FakeSessionManager : ISessionManager
{
    public Guid NodeId => Guid.NewGuid();

    private Dictionary<Int128, byte[]> _authKeySessionStates = new();
    private Dictionary<Int128, MTProtoSession> _authKeySessions = new();

    public async Task<bool> AddAuthSessionAsync(byte[] nonce, AuthSessionState state, MTProtoSession session)
    {
        var stateBytes = MessagePackSerializer.Serialize(state);
        _authKeySessions.Add((Int128)nonce, session);
        _authKeySessionStates.Add((Int128)nonce, stateBytes);
        return true;
    }

    public async Task<bool> AddSessionAsync(SessionState state, MTProtoSession session)
    {
        return true;
    }

    public async Task<AuthSessionState?> GetAuthSessionStateAsync(byte[] nonce)
    {
        var rawSession = _authKeySessionStates[(Int128)nonce];
        if (rawSession != null)
        {
            var state = MessagePackSerializer.Deserialize<AuthSessionState>(rawSession);

            return state;
        }
        return null;
    }

    public async Task<SessionState?> GetSessionStateAsync(long sessionId)
    {
        var data = File.ReadAllBytes("testdata/sessionState");
        return MessagePackSerializer.Deserialize<SessionState>(data);
    }

    public bool LocalAuthSessionExists(byte[] nonce)
    {
        throw new NotImplementedException();
    }

    public bool LocalSessionExists(long sessionId)
    {
        throw new NotImplementedException();
    }

    public bool RemoveSession(long sessionId)
    {
        throw new NotImplementedException();
    }

    public bool TryGetLocalAuthSession(byte[] nonce, out MTProtoSession session)
    {
        throw new NotImplementedException();
    }

    public bool TryGetLocalSession(long sessionId, out MTProtoSession session)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpdateAuthSessionAsync(byte[] nonce, AuthSessionState state)
    {
        _authKeySessionStates.Remove((Int128)nonce);
        _authKeySessionStates.Add((Int128)nonce, MessagePackSerializer.Serialize(state));
        return true;
    }
}
class FakeLogger : ILogger
{
    public void Debug(string message)
    {
        
    }

    public void Debug(Exception exception, string message)
    {
        
    }

    public void Error(string message)
    {
        
    }

    public void Error(Exception exception, string message)
    {
        
    }

    public void Fatal(string message)
    {
        
    }

    public void Fatal(Exception exception, string message)
    {
        
    }

    public void Information(string message)
    {
        
    }

    public void Information(Exception exception, string message)
    {
        
    }

    public void Verbose(string message)
    {
        
    }

    public void Verbose(Exception exception, string message)
    {
        
    }

    public void Warning(string message)
    {
        
    }

    public void Warning(Exception exception, string message)
    {
        
    }
}
class FakeDistributedPipe : IDistributedPipe
{
    ConcurrentQueue<byte[]> _channel = new();
    public async Task<byte[]> ReadAsync(CancellationToken cancellationToken = default)
    {
        _channel.TryDequeue(out var result);
        return result;
    }

    public void Subscribe(string channel)
    {
        
    }

    public async Task UnSubscribeAsync()
    {

    }

    public async Task WriteAsync(string channel, byte[] message)
    {
        _channel.Enqueue(message);
    }
}

public class MTProtoConnectionTests
{
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
    }

    [Fact]
    public void ReceivesMessagesFromWebSocket()
    {
        var container = BuildIoCContainer();
        ITransportConnection connection = new FakeTransportConnection("testdata/websocketSession.bin");
        connection.Start();
        MTProtoConnection mtProtoConnection = container.Resolve<MTProtoConnection>(new NamedParameter("connection", connection));
        List<ITLObject> received = new List<ITLObject>();
        var sess = new Dictionary<string, object>();
        mtProtoConnection.MessageReceived += async (s, e) => {
            received.Add(e.Message);
        };
        mtProtoConnection.Start();
        Assert.IsType<ReqPqMulti>(received[0]);
        Assert.IsType<ReqDhParams>(received[1]);
        Assert.IsType<SetClientDhParams>(received[2]);
        Assert.IsType<Ferrite.TL.layer139.updates.GetState>(received[4]);
        Assert.IsType<Ferrite.TL.mtproto.MsgsAck>(received[5]);
        Assert.IsType<Ferrite.TL.mtproto.MsgContainer>(received[6]);
        Assert.IsType<Ferrite.TL.mtproto.PingDelayDisconnect>(received[7]);
    }

    [Fact]
    public async Task SendsWebSocketHeader()
    {
        var container = BuildIoCContainer();
        FakeTransportConnection connection = new FakeTransportConnection("testdata/websocketSession_plain");
        connection.Start();
        MTProtoConnection mtProtoConnection = container.Resolve<MTProtoConnection>(new NamedParameter("connection", connection));
        List<ITLObject> received = new List<ITLObject>();
        var sess = new Dictionary<string, object>();
        mtProtoConnection.MessageReceived += async (s, e) => {
            received.Add(e.Message);
        };
        mtProtoConnection.Start();
        var webSocketResult = await connection.Application.Input.ReadAsync();
        string webSocketResponse = Encoding.UTF8.GetString(webSocketResult.Buffer.ToSpan());
        string wsExpected = "HTTP/1.1 101 Switching Protocols\r\nConnection: upgrade\r\nUpgrade: websocket\r\nSec-WebSocket-Accept: AJivmihbVG1JSXhoiKaZkpv82+s=\r\nSec-WebSocket-Protocol: binary\r\n\r\n";
        connection.Application.Input.AdvanceTo(webSocketResult.Buffer.End);
        Assert.Equal(wsExpected, webSocketResponse);
    }


    [Fact]
    public async Task SendsUnencryptedMessages()
    {
        var container = BuildIoCContainer();
        FakeTransportConnection connection = new FakeTransportConnection("testdata/websocketSession_plain");
        connection.Start();
        MTProtoConnection mtProtoConnection = container.Resolve<MTProtoConnection>(new NamedParameter("connection", connection));
        List<ITLObject> received = new List<ITLObject>();
        var sess = new Dictionary<string, object>();
        mtProtoConnection.MessageReceived += async (s, e) => {
            received.Add(e.Message);
        };
        mtProtoConnection.Start();
        var webSocketResult = await connection.Application.Input.ReadAsync();
        string webSocketResponse = Encoding.UTF8.GetString(webSocketResult.Buffer.ToSpan());
        connection.Application.Input.AdvanceTo(webSocketResult.Buffer.End);
        
        byte[] data = File.ReadAllBytes("testdata/message_0");
        byte[] expected = File.ReadAllBytes("testdata/sent_0");
        var message = MessagePackSerializer.Deserialize<MTProtoMessage>(data);
        await mtProtoConnection.SendAsync(message);
        var result = await connection.Application.Input.ReadAsync();
        Assert.Equal(expected, result.Buffer.ToSpan().Slice(2).ToArray());
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

    [Fact]
    public async Task SendsEncryptedMessage()
    {
        var container = BuildIoCContainer();
        FakeTransportConnection connection = new FakeTransportConnection("testdata/websocketSession_plain");
        connection.Start();
        MTProtoConnection mtProtoConnection = container.Resolve<MTProtoConnection>(new NamedParameter("connection", connection));
        List<ITLObject> received = new List<ITLObject>();
        var sess = new Dictionary<string, object>();
        mtProtoConnection.MessageReceived += async (s, e) => {
            received.Add(e.Message);
        };
        mtProtoConnection.Start();
        var webSocketResult = await connection.Application.Input.ReadAsync();
        string webSocketResponse = Encoding.UTF8.GetString(webSocketResult.Buffer.ToSpan());
        connection.Application.Input.AdvanceTo(webSocketResult.Buffer.End);
        
        byte[] data = File.ReadAllBytes("testdata/message_0");
        var message = MessagePackSerializer.Deserialize<MTProtoMessage>(data);
        await mtProtoConnection.SendAsync(message);
        var result = await connection.Application.Input.ReadAsync();
        connection.Application.Input.AdvanceTo(result.Buffer.End);

        await connection.Receive("testdata/websocketSession_encrypted");

        data = File.ReadAllBytes("testdata/message_1");
        message = MessagePackSerializer.Deserialize<MTProtoMessage>(data);
        await mtProtoConnection.SendAsync(message);
        result = await connection.Application.Input.ReadAsync();
        connection.Application.Input.AdvanceTo(result.Buffer.End);

        data = File.ReadAllBytes("testdata/message_2");
        message = MessagePackSerializer.Deserialize<MTProtoMessage>(data);
        await mtProtoConnection.SendAsync(message);
        result = await connection.Application.Input.ReadAsync();
        connection.Application.Input.AdvanceTo(result.Buffer.End);

        int wait = 20;
        while (!mtProtoConnection.IsEncrypted)
        {
            await Task.Delay(wait);
        }
        data = File.ReadAllBytes("testdata/message_3");
        var expected = File.ReadAllBytes("testdata/sent_3");
        message = MessagePackSerializer.Deserialize<MTProtoMessage>(data);
        await mtProtoConnection.SendAsync(message);
        result = await connection.Application.Input.ReadAsync();
        Assert.Equal(expected, result.Buffer.ToSpan().Slice(4).ToArray());
        connection.Application.Input.AdvanceTo(result.Buffer.End);
    }


    private IContainer BuildIoCContainer()
    {
        var tl = Assembly.Load("Ferrite.TL");
        var builder = new ContainerBuilder();
        builder.RegisterType<FakeTime>().As<IMTProtoTime>().SingleInstance();
        builder.RegisterType<FakeRandom>().As<IRandomGenerator>();
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
        builder.RegisterType<FakeCassandra>().As<IPersistentStore>().SingleInstance();
        builder.RegisterType<FakeRedis>().As<IDistributedStore>().SingleInstance();
        builder.RegisterType<FakeLogger>().As<ILogger>().SingleInstance();
        builder.RegisterType<FakeSessionManager>().As<ISessionManager>().SingleInstance();
        builder.RegisterType<AuthKeyProcessor>();
        builder.RegisterType<MsgContainerProcessor>();
        builder.RegisterType<ServiceMessagesProcessor>();
        builder.RegisterType<AuthorizationProcessor>();
        builder.RegisterType<MTProtoRequestProcessor>();
        builder.RegisterType<IncomingMessageHandler>().As<IProcessorManager>().SingleInstance();
        builder.RegisterType<FakeDistributedPipe>().As<IDistributedPipe>().SingleInstance();

        var container = builder.Build();

        return container;
    }

}

