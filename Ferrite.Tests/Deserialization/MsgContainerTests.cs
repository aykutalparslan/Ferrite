//
//  Project Ferrite is an Implementation of the Telegram Server API
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
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using DotNext.IO;
using Ferrite.Core;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.mtproto;
using Ferrite.Transport;
using Ferrite.Utils;
using MessagePack;
using Moq;
using Xunit;

namespace Ferrite.Tests.Deserialization;

public class MsgContainerTests
{
    [Fact]
    public void Deserializes_MsgContainer()
    {
        Span<uint> data = new uint[] {
            0x73f1f8dc, 0x00000002, 0xb2e1df30, 0x62570c6f, 0x00000005, 0x000000b8, 0xda9b0d0d, 0x0000008b,
            0x785188b8, 0x00000002, 0x0001716f, 0x73654407, 0x706f746b, 0x63616d0c, 0x3120534f, 0x2e332e32,
            0x00000031, 0x302e3110, 0x4454202c, 0x2062694c, 0x2e382e31, 0x00000032, 0x006e6502, 0x00000000,
            0x00000000, 0x99c1d49d, 0x1cb5c415, 0x00000001, 0xc0de1bd9, 0x5f7a7409, 0x7366666f, 0x00007465,
            0x2be0dfa4, 0x00000000, 0x40c51800, 0xa677244f, 0x30392b0d, 0x32373335, 0x33303531, 0x00003233,
            0x0001716f, 0x34336120, 0x65643630, 0x37316438, 0x34626231, 0x62623232, 0x66646436, 0x64626233,
            0x65303038, 0x00000032, 0x8a6469c2, 0x00000000, 0xb3538f44, 0x62570c6f, 0x00000006, 0x00000014,
            0x62d6b459, 0x1cb5c415, 0x00000001, 0x00000005, 0x62570c65
        };

        Span<byte> dataBytes = MemoryMarshal.Cast<uint, byte>(data);

        var container = BuildIoCContainer();

        var factory = container.Resolve<TLObjectFactory>();

        SequenceReader reader = IAsyncBinaryReader.Create(dataBytes.ToArray());

        var obj = (MsgContainer)factory.Read(reader.ReadInt32(true), ref reader);

        Assert.Equal(2, obj.Messages.Count);
        Assert.IsType<Ferrite.TL.currentLayer.InvokeWithLayer>(obj.Messages[0].Body);
        Assert.IsType<Ferrite.TL.mtproto.MsgsAck>(obj.Messages[1].Body);
    }

    private IContainer BuildIoCContainer()
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
        sessionManager.Setup(x => x.AddSessionAsync(It.IsAny<SessionState>(),
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
        builder.RegisterType<MTProtoTransportDetector>().As<ITransportDetector>();
        builder.RegisterType<SocketConnectionListener>().As<IConnectionListener>();
        builder.RegisterMock(proto);
        builder.RegisterMock(logger);
        builder.RegisterMock(sessionManager);
        builder.RegisterType<AuthKeyProcessor>();
        builder.RegisterType<MsgContainerProcessor>();
        builder.RegisterType<ServiceMessagesProcessor>();
        builder.RegisterType<AuthorizationProcessor>();
        builder.RegisterType<MTProtoRequestProcessor>();
        builder.RegisterType<IncomingMessageHandler>().As<IProcessorManager>().SingleInstance();
        builder.RegisterMock(pipe);
        builder.RegisterMock(new Mock<IAuthService>());

        var container = builder.Build();

        return container;
    }
}
