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
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using DotNext.Buffers;
using Ferrite.Core;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Data.Repositories;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.ObjectMapper;
using Ferrite.Transport;
using Ferrite.Utils;
using MessagePack;
using Moq;
using Xunit;
using TLConstructor = Ferrite.TL.currentLayer.TLConstructor;

namespace Ferrite.Tests.Core;
class StubTransportConnection : ITransportConnection
{
    public IDuplexPipe Transport { get; set; }
    public IDuplexPipe Application { get; set; }
    public Pipe Input { get; set; }
    public Pipe Output { get; set; }

    public EndPoint? RemoteEndPoint => new IPEndPoint(IPAddress.Any,13579);

    private byte[] _data;

    public StubTransportConnection(byte[] data)
    {
        _data = data;
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
        await Input.Writer.WriteAsync(_data);
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
public class StreamingRequestTests
{
    [Theory]
    [InlineData(64)]
    [InlineData(128)]
    [InlineData(256)]
    [InlineData(4096)]
    [InlineData(8192)]
    [InlineData(9000)]
    public async void ReceivesDataFor_SaveFilePart(int partSize)
    {
        byte[] fileData = RandomNumberGenerator.GetBytes(partSize);
        SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>();
        writer.WriteInt64(0, true);
        writer.WriteInt64(0, true);
        writer.WriteInt64(0, true);
        writer.WriteInt32(0, true);
        writer.WriteInt32(fileData.Length+16, true);
        writer.WriteInt32(TLConstructor.Upload_SaveFilePart, true);
        writer.WriteInt64(123, true);
        writer.WriteInt32(5, true);
        int length = fileData.Length;
        int rem;
        if (length < 254)
        {
            writer.Write((byte)length);
            rem = (int)((4 - ((length + 1) % 4)) % 4);
        }
        else
        {
            writer.Write((byte)254);
            writer.Write((byte)(length & 0xff));
            writer.Write((byte)((length >> 8) & 0xff));
            writer.Write((byte)((length >> 16) & 0xff));
            rem = (int)((4 - ((length + 4) % 4)) % 4);
        }
        writer.Write(fileData);
        writer.Write(RandomNumberGenerator.GetBytes(140));
        byte[] plaintext = writer.ToReadOnlySequence().ToArray();
        byte[] ciphertext = new byte[plaintext.Length];
        writer.Clear();
        byte[] authKey = RandomNumberGenerator.GetBytes(192);
        byte[] messageKey = AesIge.GenerateMessageKey(authKey, plaintext, true).ToArray();
        byte[] aesKey = new byte[32];
        byte[] aesIV = new byte[32];
        AesIge.GenerateAesKeyAndIV(authKey, messageKey, true, aesKey, aesIV);
        Aes aes = Aes.Create();
        aes.Key = aesKey;
        aes.EncryptIge(plaintext, aesIV, ciphertext);
        byte firstByte = 0xef;
        writer.Write(firstByte);
        int len = (plaintext.Length + 24) / 4;
        if (len < 127)
        {
            writer.Write((byte)len);
        }
        else
        {
            writer.Write((byte)0x7f);
            writer.Write((byte)(len & 0xff));
            writer.Write((byte)((len >> 8) & 0xFF));
            writer.Write((byte)((len >> 16) & 0xFF));
        }
        writer.WriteInt64(1, true);
        writer.Write(messageKey);
        writer.Write(ciphertext);
        var finalMessage = writer.ToReadOnlySequence().ToArray();
        writer.Clear();
        var builder = GetContainerBuilder();
        Dictionary<long, byte[]> authKeys = new Dictionary<long, byte[]>();
        Dictionary<long, byte[]> sessions = new Dictionary<long, byte[]>();
        authKeys.Add(1, authKey);
        var proto = new Mock<IMTProtoService>();
        proto.Setup(x => x.GetAuthKey(It.IsAny<long>())).Returns((long a) =>
        {
            if (!authKeys.ContainsKey(a))
            {
                return new byte[0];
            }
            return authKeys[a];
        });
        proto.Setup(x => x.PutAuthKeyAsync(It.IsAny<long>(), It.IsAny<byte[]>())).ReturnsAsync((long a, byte[] b) =>
        {
            if (!authKeys.ContainsKey(a))
            {
                authKeys.Add(a, b);
            }
            return true;
        });
        proto.Setup(x => x.GetAuthKeyAsync(It.IsAny<long>())).ReturnsAsync((long a) =>
        {
            if (!authKeys.ContainsKey(a))
            {
                return new byte[0];
            }
            return authKeys[a];
        });
        builder.RegisterMock(proto);
        ConcurrentDictionary<string, byte[]> storedObjects = new ConcurrentDictionary<string, byte[]>();
        var objectStore = new Mock<IUploadService>();
        objectStore.Setup(x => x.SaveFilePart(It.IsAny<long>(), 
            It.IsAny<int>(), It.IsAny<Stream>())).Returns(async (long fileId, int filePart, Stream data) =>
            {
                var bytes = default(byte[]);
                using (var memstream = new MemoryStream())
                {
                    await data.CopyToAsync(memstream);
                    bytes = memstream.ToArray();
                }
                storedObjects.TryAdd(fileId+"-"+filePart, bytes);
                return true;
            });
        builder.RegisterMock(objectStore);
        var processorManager = new Mock<IProcessorManager>();
        processorManager.Setup(x => x.Process(It.IsAny<object?>(),
            It.IsAny<ITLObject>(), It.IsAny<TLExecutionContext>())).Callback( 
            (object? sender, ITLObject input, TLExecutionContext ctx) =>
        {
            ((ITLMethod)input).ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>()));
        });
        
        builder.RegisterMock(processorManager);
        var container = builder.Build();
        ITransportConnection connection = new StubTransportConnection(finalMessage);
        connection.Start();
        MTProtoConnection mtProtoConnection = container.Resolve<MTProtoConnection>(new NamedParameter("connection", connection));
        List<ITLObject> received = new List<ITLObject>();
        var sess = new Dictionary<string, object>();
        mtProtoConnection.Start();
        await Task.Run(() =>
        {
            while (!storedObjects.ContainsKey("123-5"))
            {
                Task.Delay(20);
            }
        });
        Assert.Equal(fileData, storedObjects["123-5"]);
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
        Dictionary<long, byte[]> authKeys2 = new Dictionary<long, byte[]>();
        var proto = new Mock<IMTProtoService>();
        proto.Setup(x => x.PutAuthKeyAsync(It.IsAny<long>(), It.IsAny<byte[]>())).ReturnsAsync((long a, byte[] b) =>
        {
            authKeys2.Add(a, b);
            return true;
        });
        proto.Setup(x => x.GetAuthKeyAsync(It.IsAny<long>())).ReturnsAsync((long a) =>
        {
            if (!authKeys2.ContainsKey(a))
            {
                return new byte[0];
            }

            return authKeys2[a];
        });

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
        Dictionary<Ferrite.TL.Int128, MTProtoSession> _authKeySessions = new();
        var sessionManager = new Mock<ISessionService>();
        sessionManager.SetupGet(x => x.NodeId).Returns(Guid.NewGuid());
        sessionManager.Setup(x => x.AddAuthSessionAsync(It.IsAny<byte[]>(),
            It.IsAny<AuthSessionState>(), It.IsAny<MTProtoSession>())).ReturnsAsync(
            (byte[] nonce, AuthSessionState state, MTProtoSession session) =>
        {
            var stateBytes = MessagePackSerializer.Serialize(state);
            _authKeySessions.Add((Ferrite.TL.Int128)nonce, session);
            _authKeySessionStates.Add((Ferrite.TL.Int128)nonce, stateBytes);
            return true;
        });
        sessionManager.Setup(x => x.AddSessionAsync(It.IsAny<long>(), It.IsAny<long>(),
            It.IsAny<MTProtoSession>())).ReturnsAsync(() => true);
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
            return MessagePackSerializer.Deserialize<SessionState>(data);
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
        builder.RegisterMock(proto);
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
        builder.RegisterMock(logger);
        builder.RegisterMock(sessionManager);
        builder.RegisterMock(pipe);
        builder.RegisterMock(new Mock<IAuthService>());

        return builder;
    }
}