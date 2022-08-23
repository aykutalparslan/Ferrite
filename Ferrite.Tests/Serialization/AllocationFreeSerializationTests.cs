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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Autofac;
using Autofac.Core.Lifetime;
using Autofac.Extras.Moq;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.mtproto;
using Ferrite.TL.slim.mtproto;
using Ferrite.Utils;
using Moq;
using Xunit;
using ReqDhParams = Ferrite.TL.mtproto.ReqDhParams;
using ResPQ = Ferrite.TL.mtproto.ResPQ;

namespace Ferrite.Tests.Serialization;

public class AllocationFreeSerializationTests
{
    [Fact]
    public void ReqDhParams_Should_Serialize()
    {
        var container = BuildContainer();
        var tmp = container.Resolve<ReqDhParams>();
        tmp.Nonce = (Ferrite.TL.Int128)RandomNumberGenerator.GetBytes(16);
        tmp.ServerNonce = (Ferrite.TL.Int128)RandomNumberGenerator.GetBytes(16);
        tmp.P = RandomNumberGenerator.GetBytes(4);
        tmp.Q = RandomNumberGenerator.GetBytes(4);
        tmp.EncryptedData = RandomNumberGenerator.GetBytes(278);
        tmp.PublicKeyFingerprint = 123741692374192L;
        byte[] data = tmp.TLBytes.ToArray();
        Ferrite.TL.slim.mtproto.req_DH_params reqDhParams =
            Ferrite.TL.slim.mtproto.req_DH_params.Create((byte[])tmp.Nonce,
                (byte[])tmp.ServerNonce, tmp.P, tmp.Q, tmp.PublicKeyFingerprint, tmp.EncryptedData);
        Assert.Equal(data, reqDhParams.ToReadOnlySpan().ToArray());
    }
    [Fact]
    public void ResPq_Should_Serialize()
    {
        var container = BuildContainer();
        var tmp = container.Resolve<ResPQ>();
        tmp.Nonce = (Ferrite.TL.Int128)RandomNumberGenerator.GetBytes(16);
        tmp.ServerNonce = (Ferrite.TL.Int128)RandomNumberGenerator.GetBytes(16);
        tmp.Pq = RandomNumberGenerator.GetBytes(8);
        var fingerprints = new VectorOfLong(3);
        fingerprints.Add(123416662344445L);
        fingerprints.Add(734657345673634L);
        fingerprints.Add(923874923784422L);
        tmp.ServerPublicKeyFingerprints = fingerprints;
        byte[] data = tmp.TLBytes.ToArray();
        var fingerprints2 = Ferrite.TL.slim.VectorOfLong.Create(tmp.ServerPublicKeyFingerprints);
        Ferrite.TL.slim.mtproto.resPQ value =
            Ferrite.TL.slim.mtproto.resPQ.Create((byte[])tmp.Nonce,
                (byte[])tmp.ServerNonce, tmp.Pq, fingerprints2);
        Assert.Equal(data, value.ToReadOnlySpan().ToArray());
    }
    [Fact]
    public void Vector_Should_Serialize()
    {
        var container = BuildContainer();
        var vecTmp = new Vector<ReqDhParams>(container.Resolve<ITLObjectFactory>());
        for (int i = 0; i < 10; i++)
        {
            var tmp = container.Resolve<ReqDhParams>();
            tmp.Nonce = (Ferrite.TL.Int128)RandomNumberGenerator.GetBytes(16);
            tmp.ServerNonce = (Ferrite.TL.Int128)RandomNumberGenerator.GetBytes(16);
            tmp.P = RandomNumberGenerator.GetBytes(4);
            tmp.Q = RandomNumberGenerator.GetBytes(4);
            tmp.EncryptedData = RandomNumberGenerator.GetBytes(33);
            tmp.PublicKeyFingerprint = 123741692374192L+i;
            vecTmp.Add(tmp);
        }
        byte[] data = vecTmp.TLBytes.ToArray();
        List<Ferrite.TL.slim.mtproto.req_DH_params> items = new();
        Stack<IMemoryOwner<byte>> memoryOwners = new Stack<IMemoryOwner<byte>>();
        foreach (var tmp in vecTmp)
        {
            items.Add(Ferrite.TL.slim.mtproto.req_DH_params.Create((byte[])tmp.Nonce,
                (byte[])tmp.ServerNonce, tmp.P, tmp.Q, tmp.PublicKeyFingerprint, tmp.EncryptedData));
        }

        var vec = Ferrite.TL.slim.Vector<Ferrite.TL.slim.mtproto.req_DH_params>
            .Create(items);
        var actual = vec.ToReadOnlySpan().ToArray();
        Assert.Equal(data, actual);

    }
    [Fact]
    public void VectorOfInt_Should_Serialize()
    {
        var vecTmp = new VectorOfInt();
        for (int i = 0; i < 100; i++)
        {
            vecTmp.Add(i);
        }
        byte[] data = vecTmp.TLBytes.ToArray();
        List<int> items = new();
        foreach (var tmp in vecTmp)
        {
            items.Add(tmp);
        }
        var vec = Ferrite.TL.slim.VectorOfInt
            .Create(items);
        var actual = vec.ToReadOnlySpan().ToArray();
        Assert.Equal(data, actual);
    }
    [Fact]
    public void VectorOfInt_Should_SerializeWithSpanSource()
    {
        var vecTmp = new VectorOfInt();
        for (int i = 0; i < 100; i++)
        {
            vecTmp.Add(i);
        }
        byte[] data = vecTmp.TLBytes.ToArray();
        var items = vecTmp.ToArray();
        var vec = Ferrite.TL.slim.VectorOfInt
            .Create(items.AsSpan());
        var actual = vec.ToReadOnlySpan().ToArray();
        Assert.Equal(data, actual);
    }
    [Fact]
    public void VectorOfLong_Should_Serialize()
    {
        var vecTmp = new VectorOfLong();
        for (int i = 0; i < 100; i++)
        {
            vecTmp.Add(i);
        }
        byte[] data = vecTmp.TLBytes.ToArray();
        List<long> items = new();
        foreach (var tmp in vecTmp)
        {
            items.Add(tmp);
        }
        var vec = Ferrite.TL.slim.VectorOfLong
            .Create(items);
        var actual = vec.ToReadOnlySpan().ToArray();
        Assert.Equal(data, actual);
    }
    [Fact]
    public void VectorOfLong_Should_SerializeWithSpanSource()
    {
        var vecTmp = new VectorOfLong();
        for (int i = 0; i < 100; i++)
        {
            vecTmp.Add(i);
        }
        byte[] data = vecTmp.TLBytes.ToArray();
        var items = vecTmp.ToArray();
        var vec = Ferrite.TL.slim.VectorOfLong
            .Create(items.AsSpan());
        var actual = vec.ToReadOnlySpan().ToArray();
        Assert.Equal(data, actual);
    }
    [Fact]
    public void VectorOfDouble_Should_Serialize()
    {
        var vecTmp = new VectorOfDouble();
        for (int i = 0; i < 100; i++)
        {
            vecTmp.Add(i+0.3);
        }
        byte[] data = vecTmp.TLBytes.ToArray();
        List<double> items = new();
        foreach (var tmp in vecTmp)
        {
            items.Add(tmp);
        }
        var vec = Ferrite.TL.slim.VectorOfDouble
            .Create(items);
        var actual = vec.ToReadOnlySpan().ToArray();
        Assert.Equal(data, actual);
    }
    [Fact]
    public void VectorOfDouble_Should_SerializeWithSpanSource()
    {
        var vecTmp = new VectorOfDouble();
        for (int i = 0; i < 100; i++)
        {
            vecTmp.Add(i+0.3);
        }
        byte[] data = vecTmp.TLBytes.ToArray();
        var items = vecTmp.ToArray();
        var vec = Ferrite.TL.slim.VectorOfDouble
            .Create(items.AsSpan());
        var actual = vec.ToReadOnlySpan().ToArray();
        Assert.Equal(data, actual);
    }
    private static IContainer BuildContainer()
    {
        var logger = new Mock<ILogger>();
        Dictionary<long, byte[]> authKeys = new Dictionary<long, byte[]>();
        Dictionary<long, byte[]> sessions = new Dictionary<long, byte[]>();
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
        
        var redis = new Mock<IDistributedCache>();
        
        redis.Setup(x => x.PutSessionAsync(It.IsAny<long>(), It.IsAny<byte[]>(), It.IsAny<TimeSpan>())).ReturnsAsync((long a, byte[] b, TimeSpan c) =>
        {
            sessions.Add(a, b);
            return true;
        });
        
        redis.Setup(x => x.GetSessionAsync(It.IsAny<long>())).ReturnsAsync((long a) =>
        {
            if (!sessions.ContainsKey(a))
            {
                return new byte[0];
            }
            return sessions[a];
        });
        redis.Setup(x => x.DeleteSessionAsync(It.IsAny<long>())).ReturnsAsync((long a) =>
        {
            sessions.Remove(a);
            return true;
        });
        Dictionary<long, byte[]> authKeys2 = new Dictionary<long, byte[]>();
        var cassandra = new Mock<IPersistentStore>();
        var tl = Assembly.Load("Ferrite.TL");
        var builder = new ContainerBuilder();
        builder.RegisterType<RandomGenerator>().As<IRandomGenerator>();
        builder.RegisterType<KeyProvider>().As<IKeyProvider>();
        builder.RegisterAssemblyTypes(tl)
            .Where(t => t.Namespace == "Ferrite.TL.mtproto")
            .AsSelf();
        builder.Register(_ => new Ferrite.TL.Int128());
        builder.Register(_ => new Int256());
        builder.RegisterType<TLObjectFactory>().As<ITLObjectFactory>();
        builder.RegisterMock(proto);
        builder.RegisterMock(cassandra);
        builder.RegisterMock(redis);
        builder.RegisterMock(logger);
        var container = builder.Build();
        return container;
    }
}