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
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.slim;
using Ferrite.TL.slim.mtproto;
using Ferrite.Utils;
using Moq;
using Xunit;
using ReqDhParams = Ferrite.TL.mtproto.ReqDhParams;
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
        tmp.Nonce = (Ferrite.TL.Int128)RandomNumberGenerator.GetBytes(16);
        tmp.ServerNonce = (Ferrite.TL.Int128)RandomNumberGenerator.GetBytes(16);
        tmp.P = RandomNumberGenerator.GetBytes(4);
        tmp.Q = RandomNumberGenerator.GetBytes(4);
        tmp.EncryptedData = RandomNumberGenerator.GetBytes(24);
        tmp.PublicKeyFingerprint = 123741692374192L;
        byte[] data = tmp.TLBytes.ToArray();
        Ferrite.TL.slim.mtproto.ReqDhParams reqDhParams =
           new(data);
        Assert.Equal(data, reqDhParams.ToReadOnlySpan().ToArray());
        Assert.Equal(tmp.Constructor, reqDhParams.Constructor);
        Assert.Equal((byte[])tmp.Nonce, reqDhParams.Nonce.ToArray());
        Assert.Equal((byte[])tmp.ServerNonce, reqDhParams.ServerNonce.ToArray());
        Assert.Equal(tmp.P, reqDhParams.P.ToArray());
        Assert.Equal(tmp.Q, reqDhParams.Q.ToArray());
        Assert.Equal(tmp.EncryptedData, reqDhParams.EncryptedData.ToArray());
        Assert.Equal(tmp.PublicKeyFingerprint, reqDhParams.PublicKeyFingerprint);
    }
    [Fact]
    public void ResPQ_Should_AccessMemberData()
    {
        var container = BuildContainer();
        var tmp = container.Resolve<Ferrite.TL.mtproto.ResPQ>();
        tmp.Nonce = (Ferrite.TL.Int128)RandomNumberGenerator.GetBytes(16);
        tmp.ServerNonce = (Ferrite.TL.Int128)RandomNumberGenerator.GetBytes(16);
        tmp.Pq = RandomNumberGenerator.GetBytes(8);
        var fingerprints = new VectorOfLong(3);
        fingerprints.Add(123416662344445L);
        fingerprints.Add(734657345673634L);
        fingerprints.Add(923874923784422L);
        tmp.ServerPublicKeyFingerprints = fingerprints;
        byte[] data = tmp.TLBytes.ToArray();
        Ferrite.TL.slim.mtproto.ResPQ value = new(data);
        Assert.Equal(data, value.ToReadOnlySpan().ToArray());
        Assert.Equal(tmp.Constructor, value.Constructor);
        Assert.Equal((byte[])tmp.Nonce, value.Nonce.ToArray());
        Assert.Equal((byte[])tmp.ServerNonce, value.ServerNonce.ToArray());
        Assert.Equal(tmp.Nonce, value.Nonce.ToArray());
        Assert.Equal(tmp.ServerNonce, value.ServerNonce.ToArray());
        Assert.Equal(tmp.Pq, value.Pq.ToArray());
        for (int i = 0; i < value.ServerPublicKeyFingerprints.Count; i++)
        {
            Assert.Equal(tmp.ServerPublicKeyFingerprints[i], 
                value.ServerPublicKeyFingerprints[i]);
        }
    }
    [Fact]
    public void Vector_Should_Read()
    {
        var container = BuildContainer();
        var vecTmp = new Ferrite.TL.Vector<ReqDhParams>(container.Resolve<ITLObjectFactory>());
        for (int i = 0; i < 10; i++)
        {
            var tmp = container.Resolve<ReqDhParams>();
            tmp.Nonce = (Ferrite.TL.Int128)RandomNumberGenerator.GetBytes(16);
            tmp.ServerNonce = (Ferrite.TL.Int128)RandomNumberGenerator.GetBytes(16);
            tmp.P = RandomNumberGenerator.GetBytes(4);
            tmp.Q = RandomNumberGenerator.GetBytes(4);
            tmp.EncryptedData = RandomNumberGenerator.GetBytes(27);
            tmp.PublicKeyFingerprint = 123741692374192L+i;
            vecTmp.Add(tmp);
        }
        byte[] data = vecTmp.TLBytes.ToArray();
        var vec = new Ferrite.TL.slim.Vector(data); 
        
        for (int i = 0; i < vec.Count; i++)
        {
            var tmp = vecTmp[i];
            var reqDhParams = new Ferrite.TL.slim.mtproto.ReqDhParams(vec.ReadTLObject());
            Assert.Equal(tmp.Constructor, reqDhParams.Constructor);
            Assert.Equal((byte[])tmp.Nonce, reqDhParams.Nonce.ToArray());
            Assert.Equal((byte[])tmp.ServerNonce, reqDhParams.ServerNonce.ToArray());
            Assert.Equal(tmp.P, reqDhParams.P.ToArray());
            Assert.Equal(tmp.Q, reqDhParams.Q.ToArray());
            Assert.Equal(tmp.EncryptedData, reqDhParams.EncryptedData.ToArray());
            Assert.Equal(tmp.PublicKeyFingerprint, reqDhParams.PublicKeyFingerprint);
        }
    }
    [Fact]
    public void VectorOfBytes_Should_Read()
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
        var vec = new Ferrite.TL.slim.Vector(data);
        for (int i = 0; i < vec.Count; i++)
        {
            var expected = vecTmp[i];
            var actual = Encoding.UTF8.GetString(vec.ReadTLBytes());
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
        var vec = new Ferrite.TL.slim.VectorOfInt(data);
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
        var vec = new Ferrite.TL.slim.VectorOfLong(data);
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
        var vec = new Ferrite.TL.slim.VectorOfDouble(data);
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
        builder.RegisterMock(logger);
        builder.RegisterMock(proto);
        var container = builder.Build();
        return container;
    }
}