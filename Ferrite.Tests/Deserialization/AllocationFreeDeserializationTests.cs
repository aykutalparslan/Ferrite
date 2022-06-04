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
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Autofac;
using Autofac.Core.Lifetime;
using Autofac.Extras.Moq;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.TL;
using Ferrite.TL.mtproto;
using Ferrite.TL.slim;
using Ferrite.Utils;
using Moq;
using Xunit;
using VectorOfDouble = Ferrite.TL.VectorOfDouble;
using VectorOfInt = Ferrite.TL.VectorOfInt;
using VectorOfLong = Ferrite.TL.VectorOfLong;

namespace Ferrite.Tests.Deserialization;

public class AllocationFreeDeserializationTests
{
    [Fact]
    public void ReqDhParams_Should_AccessMemberData()
    {
        var container = BuildContainer();
        var tmp = container.Resolve<ReqDhParams>();
        tmp.Nonce = (Int128)RandomNumberGenerator.GetBytes(16);
        tmp.ServerNonce = (Int128)RandomNumberGenerator.GetBytes(16);
        tmp.P = RandomNumberGenerator.GetBytes(4);
        tmp.Q = RandomNumberGenerator.GetBytes(4);
        tmp.EncryptedData = RandomNumberGenerator.GetBytes(24);
        tmp.PublicKeyFingerprint = 123741692374192L;
        byte[] data = tmp.TLBytes.ToArray();
        Ferrite.TL.slim.mtproto.req_DH_params reqDhParams =
            Ferrite.TL.slim.mtproto.req_DH_params.Read(data, 0, out var bytesRead);
        Assert.Equal(tmp.Constructor, reqDhParams.Constructor);
        Assert.Equal((byte[])tmp.Nonce, reqDhParams.nonce.ToArray());
        Assert.Equal((byte[])tmp.ServerNonce, reqDhParams.server_nonce.ToArray());
        Assert.Equal(tmp.P, reqDhParams.p.ToArray());
        Assert.Equal(tmp.Q, reqDhParams.q.ToArray());
        Assert.Equal(tmp.EncryptedData, reqDhParams.encrypted_data.ToArray());
        Assert.Equal(tmp.PublicKeyFingerprint, reqDhParams.public_key_fingerprint);
    }
    [Fact]
    public void Vector_Should_Read()
    {
        var container = BuildContainer();
        var vecTmp = new Ferrite.TL.Vector<ReqDhParams>(container.Resolve<ITLObjectFactory>());
        for (int i = 0; i < 10; i++)
        {
            var tmp = container.Resolve<ReqDhParams>();
            tmp.Nonce = (Int128)RandomNumberGenerator.GetBytes(16);
            tmp.ServerNonce = (Int128)RandomNumberGenerator.GetBytes(16);
            tmp.P = RandomNumberGenerator.GetBytes(4);
            tmp.Q = RandomNumberGenerator.GetBytes(4);
            tmp.EncryptedData = RandomNumberGenerator.GetBytes(27);
            tmp.PublicKeyFingerprint = 123741692374192L+i;
            vecTmp.Add(tmp);
        }
        byte[] data = vecTmp.TLBytes.ToArray();
        var vec = Ferrite.TL.slim.Vector<Ferrite.TL.slim.mtproto.req_DH_params>.Read(data, 0, out var bytesRead);
        for (int i = 0; i < vec.Count; i++)
        {
            var tmp = vecTmp[i];
            var reqDhParams = vec.Read();
            Assert.Equal(tmp.Constructor, reqDhParams.Constructor);
            Assert.Equal((byte[])tmp.Nonce, reqDhParams.nonce.ToArray());
            Assert.Equal((byte[])tmp.ServerNonce, reqDhParams.server_nonce.ToArray());
            Assert.Equal(tmp.P, reqDhParams.p.ToArray());
            Assert.Equal(tmp.Q, reqDhParams.q.ToArray());
            Assert.Equal(tmp.EncryptedData, reqDhParams.encrypted_data.ToArray());
            Assert.Equal(tmp.PublicKeyFingerprint, reqDhParams.public_key_fingerprint);
        }
    }
    [Fact]
    public void VectorOfString_Should_Read()
    {
        var vecTmp = new VectorOfString();
        for (int i = 0; i < 50; i++)
        {
            var len = Random.Shared.Next(512);
            StringBuilder sb = new StringBuilder(len);
            for (int j = 0; j < len; j++)
            {
                sb.Append(j);
            }
            vecTmp.Add(sb.ToString()+i);
        }
        byte[] data = vecTmp.TLBytes.ToArray();
        var vec = Ferrite.TL.slim.Vector<TLString>.Read(data, 0, out var bytesRead);
        for (int i = 0; i < vec.Count; i++)
        {
            var expected = vecTmp[i];
            var actual = Encoding.UTF8.GetString(vec.Read().GetValueBytes());
            Assert.Equal(expected, actual);
        }
    }
    [Fact]
    public void VectorOfInt_Should_Read()
    {
        var vecTmp = new VectorOfInt();
        for (int i = 0; i < 100; i++)
        {
            vecTmp.Add(i);
        }
        byte[] data = vecTmp.TLBytes.ToArray();
        var vec = Ferrite.TL.slim.VectorOfInt.Read(data, 0, out var bytesRead);
        for (int i = 0; i < vec.Count; i++)
        {
            var tmp = vecTmp[i];
            var tmp2 = vec[i];
            Assert.Equal(tmp, tmp2);
        }
    }
    [Fact]
    public void VectorOfLong_Should_Read()
    {
        var vecTmp = new VectorOfLong();
        for (int i = 0; i < 100; i++)
        {
            vecTmp.Add(i*10000000000L);
        }
        byte[] data = vecTmp.TLBytes.ToArray();
        var vec = Ferrite.TL.slim.VectorOfLong.Read(data, 0, out var bytesRead);
        for (int i = 0; i < vecTmp.Count; i++)
        {
            var tmp = vecTmp[i];
            var tmp2 = vec[i];
            Assert.Equal(tmp, tmp2);
        }
    }
    [Fact]
    public void VectorOfDouble_Should_Read()
    {
        var vecTmp = new VectorOfDouble();
        for (int i = 0; i < 100; i++)
        {
            vecTmp.Add(i+0.3);
        }
        byte[] data = vecTmp.TLBytes.ToArray();
        var vec = Ferrite.TL.slim.VectorOfDouble.Read(data, 0, out var bytesRead);
        for (int i = 0; i < vec.Count; i++)
        {
            var tmp = vecTmp[i];
            var tmp2 = vec[i];
            Assert.Equal(tmp, tmp2);
        }
    }
    private static IContainer BuildContainer()
    {
        var logger = new Mock<ILogger>();
        Dictionary<long, byte[]> authKeys = new Dictionary<long, byte[]>();
        Dictionary<long, byte[]> sessions = new Dictionary<long, byte[]>();
        var redis = new Mock<IDistributedCache>();
        redis.Setup(x => x.PutAuthKeyAsync(It.IsAny<long>(), It.IsAny<byte[]>())).ReturnsAsync((long a, byte[] b) =>
        {
            authKeys.Add(a, b);
            return true;
        });
        redis.Setup(x => x.PutSessionAsync(It.IsAny<long>(), It.IsAny<byte[]>(), It.IsAny<TimeSpan>())).ReturnsAsync((long a, byte[] b, TimeSpan c) =>
        {
            sessions.Add(a, b);
            return true;
        });
        redis.Setup(x => x.GetAuthKeyAsync(It.IsAny<long>())).ReturnsAsync((long a) =>
        {
            if (!authKeys.ContainsKey(a))
            {
                return new byte[0];
            }
            return authKeys[a];
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
        cassandra.Setup(x => x.SaveAuthKeyAsync(It.IsAny<long>(), It.IsAny<byte[]>())).ReturnsAsync((long a, byte[] b) =>
        {
            authKeys2.Add(a, b);
            return true;
        });
        cassandra.Setup(x => x.GetAuthKeyAsync(It.IsAny<long>())).ReturnsAsync((long a) =>
        {
            if (!authKeys2.ContainsKey(a))
            {
                return new byte[0];
            }
            return authKeys[a];
        });
        var tl = Assembly.Load("Ferrite.TL");
        var builder = new ContainerBuilder();
        builder.RegisterType<RandomGenerator>().As<IRandomGenerator>();
        builder.RegisterType<KeyProvider>().As<IKeyProvider>();
        builder.RegisterAssemblyTypes(tl)
            .Where(t => t.Namespace == "Ferrite.TL.mtproto")
            .AsSelf();
        builder.Register(_ => new Int128());
        builder.Register(_ => new Int256());
        builder.RegisterType<TLObjectFactory>().As<ITLObjectFactory>();
        builder.RegisterMock(cassandra);
        builder.RegisterMock(redis);
        builder.RegisterMock(logger);
        var container = builder.Build();
        return container;
    }
}