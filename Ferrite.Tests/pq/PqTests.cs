/*
 *   Project Ferrite is an Implementation Telegram Server API
 *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Affero General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Affero General Public License for more details.
 *
 *   You should have received a copy of the GNU Affero General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using Xunit;
using Ferrite.TL.mtproto;
using Ferrite.Crypto;
using Autofac;
using System.Collections.Generic;
using Ferrite.TL;
using System.Buffers;
using System.Security.Cryptography;
using System.Linq;
using System.Numerics;
using Ferrite.Utils;
using System.Reflection;
using DotNext.IO;
using DotNext.Buffers;
using Ferrite.Data;
using System.Globalization;
using System.Threading.Tasks;

namespace Ferrite.Tests.PQ;

class MockDataStore : IPersistentStore
{
    public async Task<byte[]?> GetAuthKeyAsync(long authKeyId)
    {
        return RandomNumberGenerator.GetBytes(192);
    }

    public Task SaveAuthKeyAsync(long authKeyId, byte[] authKey)
    {
        throw new NotImplementedException();
    }

    public async Task SaveAuthKeyAysnc(byte[] authKeyId, byte[] authKey)
    {
        throw new NotImplementedException();
    }
}

class MockRedis : IDistributedStore
{
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
class MockRandomGenerator : IRandomGenerator
{
    byte[] random = new byte[] { 0xA5, 0xCF, 0x4D, 0x33, 0xF4, 0xA1, 0x1E, 0xA8,
        0x77, 0xBA, 0x4A, 0xA5, 0x73, 0x90, 0x73, 0x30 };

    public byte[] GetRandomBytes(int count)
    {
        return random;
    }

    public int GetRandomNumber(int toExclusive)
    {
        return 0;
    }

    
    public int GetRandomNumber(int fromInclusive, int toExclusive)
    {
        return 0;
    }

    int[] primes = new int[] { 0x494C553B, 0x53911073 };
    bool first = true;
    public int GetRandomPrime()
    {
        if (first)
        {
            first = false;
            return primes[0];
            
        } else
        {
            first = true;
            return primes[1];
        }
    }

    public BigInteger GetRandomInteger(BigInteger min, BigInteger max)
    {
        throw new NotImplementedException();
    }
}

class MockKeyPairProvider : IKeyProvider
{
    public IRSAKey? GetKey(long fingerprint)
    {
        throw new NotImplementedException();
    }

    public IList<long> GetRSAFingerprints()
    {
        List<long> l= new();
        l.Add(unchecked((long)0xc3b42b026ce86b21));
        return l;
    }
}

public class PqTests
{
    //private readonly int[] gs = new int[] { 2, 3, 4, 5, 6, 7 };
    private readonly int[] gs = new int[] { 3, 4, 7 };
    private const string dhPrime = "C71CAEB9C6B1C9048E6C522F70F13F73980D40238E3E21C14934D037563D930F48198A0AA7C14058229493D22530F4DBFA336F6E0AC925139543AED44CCE7C3720FD51F69458705AC68CD4FE6B6B13ABDC9746512969328454F18FAF8C595F642477FE96BB2A941D5BCD1D4AC8CC49880708FA9B378E3C4F3A9060BEE67CF9A4A4A695811051907E162753B56B0F6B410DBA74D8A84B2A14B3144E0EF1284754FD17ED950D5965B4B9DD46582DB1178D169C6BC465B0D6FF9CA3928FEF5B9AE4E418FC15E83EBEA0F87FA9FF5EED70050DED2849F47BF959D956850CE929851F0D8115F635B105EE2E4E15D04B2454BF6F4FADF034B10403119CD8E3B92FCC5B";

    [Fact]
    public async Task ReqPqMulti_ShouldReturnResPqAsync()
    {
        var tl = Assembly.Load("Ferrite.TL");
        var builder = new ContainerBuilder();
        builder.RegisterType<MockRandomGenerator>().As<IRandomGenerator>();
        builder.RegisterType<MockKeyPairProvider>().As<IKeyProvider>();
        builder.RegisterAssemblyTypes(tl)
            .Where(t => t.Namespace == "Ferrite.TL.mtproto")
            .AsSelf();
        builder.Register(_ => new Int128());
        builder.Register(_ => new Int256());
        builder.RegisterType<TLObjectFactory>().As<ITLObjectFactory>();
        builder.RegisterType<SerilogLogger>().As<ILogger>().SingleInstance();
        var container = builder.Build();

        var reqpq = container.Resolve<ReqPqMulti>();
        reqpq.Nonce = (Int128)(new byte[] { 0x3E, 0x05, 0x49, 0x82, 0x8C, 0xCA, 0x27, 0xE9,
            0x66, 0xB3, 0x01, 0xA4, 0x8F, 0xEC, 0xE2, 0xFC });

        TLExecutionContext context = new TLExecutionContext(new Dictionary<string, object>());
        var respq = await reqpq.ExecuteAsync(context);

        byte[] expected = new byte[]
        {
            0x63, 0x24, 0x16, 0x05, 0x3E, 0x05, 0x49, 0x82, 0x8C, 0xCA, 0x27, 0xE9, 0x66, 0xB3, 0x01, 0xA4,
            0x8F, 0xEC, 0xE2, 0xFC, 0xA5, 0xCF, 0x4D, 0x33, 0xF4, 0xA1, 0x1E, 0xA8, 0x77, 0xBA, 0x4A, 0xA5,
            0x73, 0x90, 0x73, 0x30, 0x08, 0x17, 0xED, 0x48, 0x94, 0x1A, 0x08, 0xF9, 0x81, 0x00, 0x00, 0x00,
            0x15, 0xC4, 0xB5, 0x1C, 0x01, 0x00, 0x00, 0x00, 0x21, 0x6B, 0xE8, 0x6C, 0x02, 0x2B, 0xB4, 0xC3
        };
        byte[] val = respq.TLBytes.ToArray();
        Assert.Equal(expected, val);
    }

    byte[] nonce = new byte[] { 0x3E, 0x05, 0x49, 0x82, 0x8C, 0xCA, 0x27, 0xE9,
            0x66, 0xB3, 0x01, 0xA4, 0x8F, 0xEC, 0xE2, 0xFC };

    byte[] server_nonce = new byte[] { 0xA5, 0xCF, 0x4D, 0x33, 0xF4, 0xA1, 0x1E, 0xA8,
        0x77, 0xBA, 0x4A, 0xA5, 0x73, 0x90, 0x73, 0x30 };

    byte[] pq = new byte[] { 0x17, 0xED, 0x48, 0x94, 0x1A, 0x08, 0xF9, 0x81 };

    [Fact]
    public void ReqPqMulti_ShouldModifyTLExecutionContext()
    {
        var tl = Assembly.Load("Ferrite.TL");
        var builder = new ContainerBuilder();
        builder.RegisterType<MockRandomGenerator>().As<IRandomGenerator>();
        builder.RegisterType<MockKeyPairProvider>().As<IKeyProvider>();
        builder.RegisterAssemblyTypes(tl)
            .Where(t => t.Namespace == "Ferrite.TL.mtproto")
            .AsSelf();
        builder.Register(_ => new Int128());
        builder.Register(_ => new Int256());
        builder.RegisterType<TLObjectFactory>().As<ITLObjectFactory>();
        builder.RegisterType<SerilogLogger>().As<ILogger>().SingleInstance();
        var container = builder.Build();

        var reqpq = container.Resolve<ReqPqMulti>();
        reqpq.Nonce = (Int128)nonce;

        TLExecutionContext context = new TLExecutionContext(new Dictionary<string, object>());
        var respq = reqpq.ExecuteAsync(context);

        Assert.Equal(0x494C553B, (int)context.SessionData["p"]);
        Assert.Equal(0x53911073, (int)context.SessionData["q"]);

        Assert.Equal(nonce, (Int128)context.SessionData["nonce"]);
        Assert.Equal(server_nonce, (Int128)context.SessionData["server_nonce"]);
    }

    [Fact]
    public void ReqPqMulti_ShouldNotModifyTLExecutionContext()
    {
        var tl = Assembly.Load("Ferrite.TL");
        var builder = new ContainerBuilder();
        builder.RegisterType<MockRandomGenerator>().As<IRandomGenerator>();
        builder.RegisterType<MockKeyPairProvider>().As<IKeyProvider>();
        builder.RegisterAssemblyTypes(tl)
            .Where(t => t.Namespace == "Ferrite.TL.mtproto")
            .AsSelf();
        builder.Register(_ => new Int128());
        builder.Register(_ => new Int256());
        builder.RegisterType<TLObjectFactory>().As<ITLObjectFactory>();
        builder.RegisterType<SerilogLogger>().As<ILogger>().SingleInstance();
        var container = builder.Build();

        var reqpq = container.Resolve<ReqPqMulti>();
        reqpq.Nonce = (Int128)nonce;

        TLExecutionContext context = new TLExecutionContext(new Dictionary<string, object>());
        var respq = reqpq.ExecuteAsync(context);

        Assert.Equal(0x494C553B, (int)context.SessionData["p"]);
        Assert.Equal(0x53911073, (int)context.SessionData["q"]);

        Assert.Equal(nonce, (Int128)context.SessionData["nonce"]);
        Assert.Equal(server_nonce, (Int128)context.SessionData["server_nonce"]);

        var reqpqNew = container.Resolve<ReqPqMulti>();
        reqpqNew.Nonce = (Int128)nonce;
        var respqNew = reqpqNew.ExecuteAsync(context);
        Assert.Equal(server_nonce, (Int128)context.SessionData["server_nonce"]);
    }

    private static IContainer BuildContainer()
    {
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
        builder.RegisterType<MockCassandra>().As<IPersistentStore>();
        builder.RegisterType<MockRedis>().As<IDistributedStore>();
        builder.RegisterType<SerilogLogger>().As<ILogger>().SingleInstance();
        var container = builder.Build();
        return container;
    }

    [Fact]
    public void RSAEncryptionWorks()
    {
        var keyProvider = new KeyProvider();
        var key = keyProvider.GetKey(keyProvider.GetRSAFingerprints()[0]);
        byte[] data = new byte[] {0x1,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xa,0xb,0xc,0xd,0xe,0xf,
                                0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xa,0xb,0xc,0xd,0xe,0xf,
                                0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xa,0xb,0xc,0xd,0xe,0xf,
                                0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xa,0xb,0xc,0xd,0xe,0xf,
                                0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xa,0xb,0xc,0xd,0xe,0xf,
                                0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xa,0xb,0xc,0xd,0xe,0xf,
                                0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xa,0xb,0xc,0xd,0xe,0xf,
                                0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xa,0xb,0xc,0xd,0xe,0xf,
                                0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xa,0xb,0xc,0xd,0xe,0xf,
                                0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xa,0xb,0xc,0xd,0xe,0xf,
                                0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xa,0xb,0xc,0xd,0xe,0xf,
                                0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xa,0xb,0xc,0xd,0xe,0xf,
                                0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xa,0xb,0xc,0xd,0xe,0xf,
                                0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xa,0xb,0xc,0xd,0xe,0xf,
                                0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xa,0xb,0xc,0xd,0xe,0xf,
                                0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xa,0xb,0xc,0xd,0xe,0xf};

        byte[] encrypted = key.EncryptBlock(data);
        Assert.NotEqual(data, encrypted);
        byte[] decrypted = key.DecryptBlock(encrypted);
        Assert.Equal(data, decrypted);
    }
    //https://stackoverflow.com/a/321404/2015348
    public static byte[] StringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                         .ToArray();
    }

    [Fact]
    public async Task ReqDhParams_ShouldReturnServerDhParamsAsync()
    {
        IContainer container = BuildContainer();

        TLExecutionContext context = new TLExecutionContext(new Dictionary<string, object>());
        byte[] n = new byte[] { 0x3E, 0x05, 0x49, 0x82, 0x8C, 0xCA, 0x27, 0xE9,
            0x66, 0xB3, 0x01, 0xA4, 0x8F, 0xEC, 0xE2, 0xFC };
        byte[] sn = new byte[] { 0xA5, 0xCF, 0x4D, 0x33, 0xF4, 0xA1, 0x1E, 0xA8,
            0x77, 0xBA, 0x4A, 0xA5, 0x73, 0x90, 0x73, 0x30 };
        byte[] nn = new byte[] { 0x31, 0x1C, 0x85, 0xDB, 0x23, 0x4A, 0xA2, 0x64,
            0x0A, 0xFC, 0x4A, 0x76, 0xA7, 0x35, 0xCF, 0x5B,
            0x1F, 0x0F, 0xD6, 0x8B, 0xD1, 0x7F, 0xA1, 0x81,
            0xE1, 0x22, 0x9A, 0xD8, 0x67, 0xCC, 0x02, 0x4D };
        context.SessionData.Add("nonce", (Int128)n);
        context.SessionData.Add("server_nonce", (Int128)sn);
        context.SessionData.Add("p", 0x494C553B);
        context.SessionData.Add("q", 0x53911073);

        var reqDhParams = container.Resolve<ReqDhParams>();
        reqDhParams.Nonce = (Int128)n;
        reqDhParams.ServerNonce = (Int128)sn;
        var pBytes = BitConverter.GetBytes(0x494C553B);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(pBytes);
        }
        reqDhParams.P = pBytes;
        var qBytes = BitConverter.GetBytes(0x53911073);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(qBytes);
        }
        reqDhParams.Q = qBytes;


        PQInnerDataDc pQInnerDataDc = container.Resolve<PQInnerDataDc>();
        pQInnerDataDc.Pq = pq;
        pQInnerDataDc.P = pBytes;
        pQInnerDataDc.Q = qBytes;
        pQInnerDataDc.Nonce = (Int128)n;
        pQInnerDataDc.ServerNonce = (Int128)sn;
        var newNonceBytes = RandomNumberGenerator.GetBytes(32);
        pQInnerDataDc.NewNonce = (Int256)nn;
        pQInnerDataDc.Dc = 1;
        byte[] sha1Received, sha1Actual;
        int constructor;
        ServerDhInnerData serverDhInnerData;
        byte[] tempAesKey;
        byte[] tempAesIV;

        byte[] data = pQInnerDataDc.TLBytes.ToArray();
        byte[] paddingBytes = RandomNumberGenerator.GetBytes(192 - data.Length);
        byte[] dataWithPadding = data.Concat(paddingBytes).ToArray();
        byte[] dataPadReversed = dataWithPadding.Reverse().ToArray();
        byte[] keyAesEncrypted = encrypt(dataWithPadding, dataPadReversed);

        var keyProvider = container.Resolve<IKeyProvider>();
        var key = keyProvider.GetKey(keyProvider.GetRSAFingerprints()[0]);
        BigInteger modulus = new BigInteger(key.PublicKeyParameters.Modulus, true, true);
        BigInteger numEncrypted = new BigInteger(keyAesEncrypted, true, true);
        while (numEncrypted >= modulus)
        {
            keyAesEncrypted = encrypt(dataWithPadding, dataPadReversed);
            numEncrypted = new BigInteger(keyAesEncrypted, true, true);
        }

        byte[] rsaEncrypted = key.EncryptBlock(keyAesEncrypted);

        reqDhParams.EncryptedData = rsaEncrypted;
        reqDhParams.PublicKeyFingerprint = key.Fingerprint;

        ServerDhParamsOk serverDhParamsOk = (ServerDhParamsOk)await reqDhParams.ExecuteAsync(context);
        Assert.Equal(nonce, serverDhParamsOk.Nonce);
        Assert.Equal(server_nonce, serverDhParamsOk.ServerNonce);

        var newNonceServerNonce = SHA1.HashData(((byte[])pQInnerDataDc.NewNonce)
                .Concat(server_nonce).ToArray());
        var serverNonceNewNonce = SHA1.HashData(server_nonce
            .Concat((byte[])pQInnerDataDc.NewNonce).ToArray());
        var newNonceNewNonce = SHA1.HashData(((byte[])pQInnerDataDc.NewNonce)
            .Concat((byte[])pQInnerDataDc.NewNonce).ToArray());
        var tmpAesKey = newNonceServerNonce
            .Concat(serverNonceNewNonce.SkipLast(8)).ToArray();
        var tmpAesIV = serverNonceNewNonce.Skip(12)
            .Concat(newNonceNewNonce).Concat(((byte[])pQInnerDataDc.NewNonce).SkipLast(28)).ToArray();
        tempAesKey = tmpAesKey.ToArray();
        tempAesIV = tmpAesIV.ToArray();
        Assert.Equal(592, serverDhParamsOk.EncryptedAnswer.Length);

        Aes aes = Aes.Create();
        aes.Key = tmpAesKey;
        aes.DecryptIge(serverDhParamsOk.EncryptedAnswer, tmpAesIV);
        ITLObjectFactory factory = container.Resolve<ITLObjectFactory>();

        sha1Received = serverDhParamsOk.EncryptedAnswer.AsSpan().Slice(0, 20).ToArray();
        SequenceReader reader = IAsyncBinaryReader.Create(serverDhParamsOk.EncryptedAnswer.AsSpan().Slice(20).ToArray());
        constructor = reader.ReadInt32(true);
        serverDhInnerData = (ServerDhInnerData)factory.Read(constructor, ref reader);
        sha1Actual = SHA1.HashData(serverDhInnerData.TLBytes.ToArray());

        Assert.Equal(tempAesKey,
            StringToByteArray("F011280887C7BB01DF0FC4E17830E0B91FBB8BE4B2267CB985AE25F33B527253"));

        Assert.Equal(tempAesIV,
            StringToByteArray("3212D579EE35452ED23E0D0C92841AA7D31B2E9BDEF2151E80D15860311C85DB"));
        
        Assert.True(context.SessionData.ContainsKey("a"));
        BigInteger prime = BigInteger.Parse("0" + dhPrime, NumberStyles.HexNumber);
        BigInteger a = new BigInteger((byte[])context.SessionData["a"], true, true);
        BigInteger g_a = BigInteger.ModPow(new BigInteger((int)context.SessionData["g"]), a, prime);

        bool mod_ok;
        uint mod_r;
        switch ((int)context.SessionData["g"])
        {
            case 2:
                mod_ok = prime % 8 == 7u;
                break;
            case 3:
                mod_ok = prime % 3 == 2u;
                break;
            case 4:
                mod_ok = true;
                break;
            case 5:
                mod_ok = (mod_r = (uint)(prime % 5)) == 1u || mod_r == 4u;
                break;
            case 6:
                mod_ok = (mod_r = (uint)(prime % 24)) == 19u || mod_r == 23u;
                break;
            case 7:
                mod_ok = (mod_r = (uint)(prime % 7)) == 3u || mod_r == 5u || mod_r == 6u;
                break;
            default:
                mod_ok = false;
                break;
        }
        Assert.True(mod_ok);
        Assert.Equal((int)TLConstructor.ServerDhInnerData, constructor);
        Assert.Equal(sha1Actual, sha1Received);
        Assert.Equal(nn, (byte[])context.SessionData["new_nonce"]);
        Assert.Equal(tempAesKey, (byte[])context.SessionData["temp_aes_key"]);
        Assert.Equal(tempAesIV, (byte[])context.SessionData["temp_aes_iv"]);
        Assert.Equal(serverDhInnerData.G, (int)context.SessionData["g"]);
        Assert.Equal(serverDhInnerData.GA, (byte[])context.SessionData["g_a"]);
        Assert.Equal(serverDhInnerData.DhPrime, StringToByteArray(dhPrime));
        Assert.Contains(serverDhInnerData.G, gs);
    }
    [Fact]
    public async Task ReqDhParamsTemp_ShouldReturnServerDhParamsAsync()
    {
        IContainer container = BuildContainer();

        TLExecutionContext context = new TLExecutionContext(new Dictionary<string, object>());
        context.SessionData.Add("nonce", (Int128)nonce);
        context.SessionData.Add("server_nonce", (Int128)server_nonce);
        context.SessionData.Add("p", 0x494C553B);
        context.SessionData.Add("q", 0x53911073);

        var reqDhParams = container.Resolve<ReqDhParams>();
        reqDhParams.Nonce = (Int128)nonce;
        reqDhParams.ServerNonce = (Int128)server_nonce;
        var pBytes = BitConverter.GetBytes(0x494C553B);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(pBytes);
        }
        reqDhParams.P = pBytes;
        var qBytes = BitConverter.GetBytes(0x53911073);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(qBytes);
        }
        reqDhParams.Q = qBytes;


        PQInnerDataTempDc pQInnerDataDc = container.Resolve<PQInnerDataTempDc>();
        pQInnerDataDc.Pq = pq;
        pQInnerDataDc.P = pBytes;
        pQInnerDataDc.Q = qBytes;
        pQInnerDataDc.Nonce = (Int128)nonce;
        pQInnerDataDc.ServerNonce = (Int128)server_nonce;
        var newNonceBytes = RandomNumberGenerator.GetBytes(32);
        pQInnerDataDc.NewNonce = (Int256)newNonceBytes;
        pQInnerDataDc.Dc = 1;
        pQInnerDataDc.ExpiresIn = 1000;
        byte[] sha1Received, sha1Actual;
        int constructor;
        ServerDhInnerData serverDhInnerData;
        byte[] tempAesKey;
        byte[] tempAesIV;

        byte[] data = pQInnerDataDc.TLBytes.ToArray();
        byte[] paddingBytes = RandomNumberGenerator.GetBytes(192 - data.Length);
        byte[] dataWithPadding = data.Concat(paddingBytes).ToArray();
        byte[] dataPadReversed = dataWithPadding.Reverse().ToArray();
        byte[] keyAesEncrypted = encrypt(dataWithPadding, dataPadReversed);

        var keyProvider = container.Resolve<IKeyProvider>();
        var key = keyProvider.GetKey(keyProvider.GetRSAFingerprints()[0]);
        BigInteger modulus = new BigInteger(key.PublicKeyParameters.Modulus, true, true);
        BigInteger numEncrypted = new BigInteger(keyAesEncrypted, true, true);
        while (numEncrypted >= modulus)
        {
            keyAesEncrypted = encrypt(dataWithPadding, dataPadReversed);
            numEncrypted = new BigInteger(keyAesEncrypted, true, true);
        }

        byte[] rsaEncrypted = key.EncryptBlock(keyAesEncrypted);

        reqDhParams.EncryptedData = rsaEncrypted;
        reqDhParams.PublicKeyFingerprint = key.Fingerprint;

        ServerDhParamsOk serverDhParamsOk = (ServerDhParamsOk)await reqDhParams.ExecuteAsync(context);
        Assert.Equal(nonce, serverDhParamsOk.Nonce);
        Assert.Equal(server_nonce, serverDhParamsOk.ServerNonce);

        var newNonceServerNonce = SHA1.HashData(((byte[])pQInnerDataDc.NewNonce)
                .Concat(server_nonce).ToArray());
        var serverNonceNewNonce = SHA1.HashData(server_nonce
            .Concat((byte[])pQInnerDataDc.NewNonce).ToArray());
        var newNonceNewNonce = SHA1.HashData(((byte[])pQInnerDataDc.NewNonce)
            .Concat((byte[])pQInnerDataDc.NewNonce).ToArray());
        var tmpAesKey = newNonceServerNonce
            .Concat(serverNonceNewNonce.SkipLast(8)).ToArray();
        var tmpAesIV = serverNonceNewNonce.Skip(12)
            .Concat(newNonceNewNonce).Concat(((byte[])pQInnerDataDc.NewNonce).SkipLast(28)).ToArray();
        tempAesKey = tmpAesKey.ToArray();
        tempAesIV = tmpAesIV.ToArray();

        Aes aes = Aes.Create();
        aes.Key = tmpAesKey;
        aes.DecryptIge(serverDhParamsOk.EncryptedAnswer, tmpAesIV);
        ITLObjectFactory factory = container.Resolve<ITLObjectFactory>();

        sha1Received = serverDhParamsOk.EncryptedAnswer.AsSpan().Slice(0, 20).ToArray();
        SequenceReader reader = IAsyncBinaryReader.Create(serverDhParamsOk.EncryptedAnswer.AsSpan().Slice(20).ToArray());
        constructor = reader.ReadInt32(true);
        serverDhInnerData = (ServerDhInnerData)factory.Read(constructor, ref reader);
        sha1Actual = SHA1.HashData(serverDhInnerData.TLBytes.ToArray());

        Assert.True(context.SessionData.ContainsKey("valid_until"));
        Assert.True(context.SessionData.ContainsKey("a"));
        BigInteger prime = BigInteger.Parse("0" + dhPrime, NumberStyles.HexNumber);
        BigInteger a = new BigInteger((byte[])context.SessionData["a"], true, true);
        BigInteger g_a = BigInteger.ModPow(new BigInteger((int)context.SessionData["g"]), a, prime);

        bool mod_ok;
        uint mod_r;
        switch ((int)context.SessionData["g"])
        {
            case 2:
                mod_ok = prime % 8 == 7u;
                break;
            case 3:
                mod_ok = prime % 3 == 2u;
                break;
            case 4:
                mod_ok = true;
                break;
            case 5:
                mod_ok = (mod_r = (uint)(prime % 5)) == 1u || mod_r == 4u;
                break;
            case 6:
                mod_ok = (mod_r = (uint)(prime % 24)) == 19u || mod_r == 23u;
                break;
            case 7:
                mod_ok = (mod_r = (uint)(prime % 7)) == 3u || mod_r == 5u || mod_r == 6u;
                break;
            default:
                mod_ok = false;
                break;
        }
        Assert.True(mod_ok);

        Assert.Equal(sha1Actual, sha1Received);
        Assert.Equal((int)TLConstructor.ServerDhInnerData, constructor);
        Assert.Equal(newNonceBytes, (byte[])context.SessionData["new_nonce"]);
        Assert.Equal(tempAesKey, (byte[])context.SessionData["temp_aes_key"]);
        Assert.Equal(tempAesIV, (byte[])context.SessionData["temp_aes_iv"]);
        Assert.Equal(serverDhInnerData.G, (int)context.SessionData["g"]);
        Assert.Equal(serverDhInnerData.GA, (byte[])context.SessionData["g_a"]);
        Assert.Equal(g_a.ToByteArray(true,true),(byte[])context.SessionData["g_a"]);
        Assert.Equal(serverDhInnerData.DhPrime, StringToByteArray(dhPrime));
        Assert.Contains(serverDhInnerData.G, gs);
    }
    

    [Fact]
    public async Task SetClientDhParams_ShouldReturnDhGenRetryAsync()
    {
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
        builder.RegisterType<MockDataStore>().As<IPersistentStore>();
        builder.RegisterType<MockRedis>().As<IDistributedStore>();
        builder.RegisterType<SerilogLogger>().As<ILogger>().SingleInstance();
        var container = builder.Build();
        
        var random = container.Resolve<IRandomGenerator>();
        byte[] n = new byte[] { 0x3E, 0x05, 0x49, 0x82, 0x8C, 0xCA, 0x27, 0xE9,
            0x66, 0xB3, 0x01, 0xA4, 0x8F, 0xEC, 0xE2, 0xFC };
        byte[] sn = new byte[] { 0xA5, 0xCF, 0x4D, 0x33, 0xF4, 0xA1, 0x1E, 0xA8,
            0x77, 0xBA, 0x4A, 0xA5, 0x73, 0x90, 0x73, 0x30 };
        byte[] nn = new byte[] { 0x31, 0x1C, 0x85, 0xDB, 0x23, 0x4A, 0xA2, 0x64,
            0x0A, 0xFC, 0x4A, 0x76, 0xA7, 0x35, 0xCF, 0x5B,
            0x1F, 0x0F, 0xD6, 0x8B, 0xD1, 0x7F, 0xA1, 0x81,
            0xE1, 0x22, 0x9A, 0xD8, 0x67, 0xCC, 0x02, 0x4D };

        TLExecutionContext context = new TLExecutionContext(new Dictionary<string, object>());
        context.SessionData.Add("nonce", (Int128)n);
        context.SessionData.Add("server_nonce", (Int128)sn);
        context.SessionData.Add("p", 0x494C553B);
        context.SessionData.Add("q", 0x53911073);
        BigInteger prime = BigInteger.Parse("0" + dhPrime, NumberStyles.HexNumber);
        BigInteger min = BigInteger.Pow(new BigInteger(2), 2048 - 64);
        BigInteger max = prime - min;
        BigInteger a = random.GetRandomInteger(2, prime - 2);
        BigInteger g = new BigInteger(gs[random.GetRandomNumber(gs.Length)]);
        BigInteger g_a = BigInteger.ModPow(g, a, prime);
        while (g_a <= min || g_a >= max)
        {
            a = random.GetRandomInteger(2, prime - 2);
            g_a = BigInteger.ModPow(g, a, prime);
        }

        context.SessionData.Add("g", (int)g);
        context.SessionData.Add("g_a", g_a.ToByteArray(true, true));
        context.SessionData.Add("a", a.ToByteArray(true, true));
        byte[] newNonce = RandomNumberGenerator.GetBytes(32);
        context.SessionData.Add("new_nonce", nn);

        BigInteger b = random.GetRandomInteger(2, prime - 2);
        BigInteger g_b = BigInteger.ModPow(g, b, prime);
        while (g_a <= min || g_a >= max)
        {
            b = random.GetRandomInteger(2, prime - 2);
            g_b = BigInteger.ModPow(g, b, prime);
        }

        var clientDhInnerData = container.Resolve<ClientDhInnerData>();
        clientDhInnerData.GB = g_b.ToByteArray(true, true);
        clientDhInnerData.Nonce = (Int128)n;
        clientDhInnerData.ServerNonce = (Int128)sn;
        clientDhInnerData.RetryId = 0;

        var buff = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
        buff.Write(SHA1.HashData(clientDhInnerData.TLBytes.ToArray()));
        buff.Write(clientDhInnerData.TLBytes.ToArray());
        if (buff.WrittenCount % 16 != 0)
        {
            int pad = (int)(16 - buff.WrittenCount % 16);
            for (int i = 0; i < pad; i++)
            {
                buff.Write((byte)0);
            }
        }

        var newNonceServerNonce = SHA1.HashData(nn.Concat(sn).ToArray());
        var serverNonceNewNonce = SHA1.HashData(sn.Concat(nn).ToArray());
        var newNonceNewNonce = SHA1.HashData(nn.Concat(nn).ToArray());
        var tmpAesKey = newNonceServerNonce
            .Concat(serverNonceNewNonce.SkipLast(8)).ToArray();
        var tmpAesIV = serverNonceNewNonce.Skip(12)
            .Concat(newNonceNewNonce).Concat(newNonce).SkipLast(28).ToArray();
        context.SessionData.Add("temp_aes_key", tmpAesKey.ToArray());
        context.SessionData.Add("temp_aes_iv", tmpAesIV.ToArray());
        Aes aes = Aes.Create();
        aes.Key = tmpAesKey;
        byte[] encrypted = new byte[buff.WrittenCount];
        aes.EncryptIge(buff.ToReadOnlySequence().ToArray(), tmpAesIV, encrypted);
        ITLObjectFactory factory = container.Resolve<ITLObjectFactory>();

        var setClientDhParams = factory.Resolve<SetClientDhParams>();
        setClientDhParams.Nonce = (Int128)n;
        setClientDhParams.ServerNonce = (Int128)sn;
        setClientDhParams.EncryptedData = encrypted;
        var result = (DhGenRetry) await setClientDhParams.ExecuteAsync(context);

        var authKey = BigInteger.ModPow(g_a, b, prime).ToByteArray(true, true);
        var authKeySHA1 = SHA1.HashData(authKey);
        var authKeyHash = authKeySHA1.Skip(12).ToArray();
        var authKeyAuxHash = authKeySHA1.Take(8).ToArray();

        var str = nn.Concat(new byte[1] { (byte)2 })
            .Concat(authKeyAuxHash).ToArray();
        var newNonceHash2 = SHA1.HashData(str).Skip(4).ToArray();



        Assert.Equal(authKey, (byte[])context.SessionData["auth_key"]);
        Assert.Equal(newNonceHash2, result.NewNonceHash2);
    }


    [Fact]
    public async Task SetClientDhParams_ShouldReturnDhGenOkAsync()
    {
        IContainer container = BuildContainer();
        var random = container.Resolve<IRandomGenerator>();
        byte[] n = new byte[] { 0x3E, 0x05, 0x49, 0x82, 0x8C, 0xCA, 0x27, 0xE9,
            0x66, 0xB3, 0x01, 0xA4, 0x8F, 0xEC, 0xE2, 0xFC };
        byte[] sn = new byte[] { 0xA5, 0xCF, 0x4D, 0x33, 0xF4, 0xA1, 0x1E, 0xA8,
            0x77, 0xBA, 0x4A, 0xA5, 0x73, 0x90, 0x73, 0x30 };
        byte[] nn = new byte[] { 0x31, 0x1C, 0x85, 0xDB, 0x23, 0x4A, 0xA2, 0x64,
            0x0A, 0xFC, 0x4A, 0x76, 0xA7, 0x35, 0xCF, 0x5B,
            0x1F, 0x0F, 0xD6, 0x8B, 0xD1, 0x7F, 0xA1, 0x81,
            0xE1, 0x22, 0x9A, 0xD8, 0x67, 0xCC, 0x02, 0x4D };

        TLExecutionContext context = new TLExecutionContext(new Dictionary<string, object>());
        context.SessionData.Add("nonce", (Int128)n);
        context.SessionData.Add("server_nonce", (Int128)sn);
        context.SessionData.Add("p", 0x494C553B);
        context.SessionData.Add("q", 0x53911073);
        BigInteger prime = BigInteger.Parse("0" + dhPrime, NumberStyles.HexNumber);
        BigInteger min = BigInteger.Pow(new BigInteger(2), 2048 - 64);
        BigInteger max = prime - min;
        BigInteger a = random.GetRandomInteger(2, prime - 2);
        BigInteger g = new BigInteger(gs[random.GetRandomNumber(gs.Length)]);
        BigInteger g_a = BigInteger.ModPow(g, a, prime);
        while (g_a <= min || g_a >= max)
        {
            a = random.GetRandomInteger(2, prime - 2);
            g_a = BigInteger.ModPow(g, a, prime);
        }

        context.SessionData.Add("g", (int)g);
        context.SessionData.Add("g_a", g_a.ToByteArray(true, true));
        context.SessionData.Add("a", a.ToByteArray(true, true));
        byte[] newNonce = RandomNumberGenerator.GetBytes(32);
        context.SessionData.Add("new_nonce", nn);

        BigInteger b = random.GetRandomInteger(2, prime - 2);
        BigInteger g_b = BigInteger.ModPow(g, b, prime);
        while (g_a <= min || g_a >= max)
        {
            b = random.GetRandomInteger(2, prime - 2);
            g_b = BigInteger.ModPow(g, b, prime);
        }

        var clientDhInnerData = container.Resolve<ClientDhInnerData>();
        clientDhInnerData.GB = g_b.ToByteArray(true, true);
        clientDhInnerData.Nonce = (Int128)n;
        clientDhInnerData.ServerNonce = (Int128)sn;
        clientDhInnerData.RetryId = 0;

        var buff = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
        buff.Write(SHA1.HashData(clientDhInnerData.TLBytes.ToArray()));
        buff.Write(clientDhInnerData.TLBytes.ToArray());
        if (buff.WrittenCount % 16 != 0)
        {
            int pad = (int)(16 - buff.WrittenCount % 16);
            for (int i = 0; i < pad; i++)
            {
                buff.Write((byte)0);
            }
        }

        var newNonceServerNonce = SHA1.HashData(nn.Concat(sn).ToArray());
        var serverNonceNewNonce = SHA1.HashData(sn.Concat(nn).ToArray());
        var newNonceNewNonce = SHA1.HashData(nn.Concat(nn).ToArray());
        var tmpAesKey = newNonceServerNonce
            .Concat(serverNonceNewNonce.SkipLast(8)).ToArray();
        var tmpAesIV = serverNonceNewNonce.Skip(12)
            .Concat(newNonceNewNonce).Concat(newNonce).SkipLast(28).ToArray();
        context.SessionData.Add("temp_aes_key", tmpAesKey.ToArray());
        context.SessionData.Add("temp_aes_iv", tmpAesIV.ToArray());
        Aes aes = Aes.Create();
        aes.Key = tmpAesKey;
        byte[] encrypted = new byte[buff.WrittenCount];
        aes.EncryptIge(buff.ToReadOnlySequence().ToArray(), tmpAesIV, encrypted);
        ITLObjectFactory factory = container.Resolve<ITLObjectFactory>();

        var setClientDhParams = factory.Resolve<SetClientDhParams>();
        setClientDhParams.Nonce = (Int128)n;
        setClientDhParams.ServerNonce = (Int128)sn;
        setClientDhParams.EncryptedData = encrypted;
        var result = (DhGenOk) await setClientDhParams.ExecuteAsync(context);
        
        var authKey = BigInteger.ModPow(g_a, b, prime).ToByteArray(true, true);
        var authKeySHA1 = SHA1.HashData(authKey);
        var authKeyHash = authKeySHA1.Skip(12).ToArray();
        var authKeyAuxHash = authKeySHA1.Take(8).ToArray();

        var str = nn.Concat(new byte[1] { (byte)1 })
            .Concat(authKeyAuxHash).ToArray();
        var newNonceHash1 = SHA1.HashData(str).Skip(4).ToArray();



        Assert.Equal(authKey, (byte[])context.SessionData["auth_key"]);
        Assert.Equal(newNonceHash1, result.NewNonceHash1);
    }

    

    private static byte[] encrypt(byte[] dataWithPadding, byte[] dataPadReversed)
    {
        byte[] tempKey = RandomNumberGenerator.GetBytes(32);
        byte[] dataWithHash = dataPadReversed.Concat(SHA256.HashData(tempKey.Concat(dataWithPadding).ToArray())).ToArray();
        Aes aes = Aes.Create();
        aes.Key = tempKey;
        byte[] aesEncrypted = new byte[dataWithHash.Length];
        aes.EncryptIge(dataWithHash, new byte[32], aesEncrypted);
        byte[] temKeyXor = new byte[32];
        byte[] sha256AesEncrypted = SHA256.HashData(aesEncrypted);
        for (int i = 0; i < 32; i++)
        {
            temKeyXor[i] = (byte)(tempKey[i] ^ sha256AesEncrypted[i]);
        }
        byte[] keyAesEncrypted = temKeyXor.Concat(aesEncrypted).ToArray();
        return keyAesEncrypted;
    }
}
