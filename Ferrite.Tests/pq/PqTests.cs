/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
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

namespace Ferrite.Tests.pq;

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
    private readonly int[] gs = new int[] { 2, 3, 4, 5, 6, 7 };
    private readonly byte[] dhPrime = new byte[]
    {
        0xC7, 0x1C, 0xAE, 0xB9, 0xC6, 0xB1, 0xC9, 0x04, 0x8E, 0x6C, 0x52, 0x2F,
        0x70, 0xF1, 0x3F, 0x73, 0x98, 0x0D, 0x40, 0x23, 0x8E, 0x3E, 0x21, 0xC1,
        0x49, 0x34, 0xD0, 0x37, 0x56, 0x3D, 0x93, 0x0F, 0x48, 0x19, 0x8A, 0x0A,
        0xA7, 0xC1, 0x40, 0x58, 0x22, 0x94, 0x93, 0xD2, 0x25, 0x30, 0xF4, 0xDB,
        0xFA, 0x33, 0x6F, 0x6E, 0x0A, 0xC9, 0x25, 0x13, 0x95, 0x43, 0xAE, 0xD4,
        0x4C, 0xCE, 0x7C, 0x37, 0x20, 0xFD, 0x51, 0xF6, 0x94, 0x58, 0x70, 0x5A,
        0xC6, 0x8C, 0xD4, 0xFE, 0x6B, 0x6B, 0x13, 0xAB, 0xDC, 0x97, 0x46, 0x51,
        0x29, 0x69, 0x32, 0x84, 0x54, 0xF1, 0x8F, 0xAF, 0x8C, 0x59, 0x5F, 0x64,
        0x24, 0x77, 0xFE, 0x96, 0xBB, 0x2A, 0x94, 0x1D, 0x5B, 0xCD, 0x1D, 0x4A,
        0xC8, 0xCC, 0x49, 0x88, 0x07, 0x08, 0xFA, 0x9B, 0x37, 0x8E, 0x3C, 0x4F,
        0x3A, 0x90, 0x60, 0xBE, 0xE6, 0x7C, 0xF9, 0xA4, 0xA4, 0xA6, 0x95, 0x81,
        0x10, 0x51, 0x90, 0x7E, 0x16, 0x27, 0x53, 0xB5, 0x6B, 0x0F, 0x6B, 0x41,
        0x0D, 0xBA, 0x74, 0xD8, 0xA8, 0x4B, 0x2A, 0x14, 0xB3, 0x14, 0x4E, 0x0E,
        0xF1, 0x28, 0x47, 0x54, 0xFD, 0x17, 0xED, 0x95, 0x0D, 0x59, 0x65, 0xB4,
        0xB9, 0xDD, 0x46, 0x58, 0x2D, 0xB1, 0x17, 0x8D, 0x16, 0x9C, 0x6B, 0xC4,
        0x65, 0xB0, 0xD6, 0xFF, 0x9C, 0xA3, 0x92, 0x8F, 0xEF, 0x5B, 0x9A, 0xE4,
        0xE4, 0x18, 0xFC, 0x15, 0xE8, 0x3E, 0xBE, 0xA0, 0xF8, 0x7F, 0xA9, 0xFF,
        0x5E, 0xED, 0x70, 0x05, 0x0D, 0xED, 0x28, 0x49, 0xF4, 0x7B, 0xF9, 0x59,
        0xD9, 0x56, 0x85, 0x0C, 0xE9, 0x29, 0x85, 0x1F, 0x0D, 0x81, 0x15, 0xF6,
        0x35, 0xB1, 0x05, 0xEE, 0x2E, 0x4E, 0x15, 0xD0, 0x4B, 0x24, 0x54, 0xBF,
        0x6F, 0x4F, 0xAD, 0xF0, 0x34, 0xB1, 0x04, 0x03, 0x11, 0x9C, 0xD8, 0xE3,
        0xB9, 0x2F, 0xCC, 0x5B
    };

    [Fact]
    public void ReqPqMulti_ShouldReturnResPq()
    {
        var tl = Assembly.Load("Ferrite.TL");
        var builder = new ContainerBuilder();
        builder.RegisterType<MockRandomGenerator>().As<IRandomGenerator>();
        builder.RegisterType<MockKeyPairProvider>().As<IKeyProvider>();
        builder.RegisterAssemblyTypes(tl)
            .Where(t => t.Namespace == "Ferrite.TL.mtproto")
            .AsSelf();
        builder.RegisterType<Int128>();
        builder.RegisterType<Int256>();
        builder.RegisterType<TLObjectFactory>().As<ITLObjectFactory>();
        builder.RegisterType<SerilogLogger>().As<ILogger>().SingleInstance();
        var container = builder.Build();

        var reqpq = container.Resolve<ReqPqMulti>();
        reqpq.Nonce = (Int128)(new byte[] { 0x3E, 0x05, 0x49, 0x82, 0x8C, 0xCA, 0x27, 0xE9,
            0x66, 0xB3, 0x01, 0xA4, 0x8F, 0xEC, 0xE2, 0xFC });

        TLExecutionContext context = new TLExecutionContext();
        var respq = reqpq.Execute(context);

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
        IContainer container = BuildContainer();

        var reqpq = container.Resolve<ReqPqMulti>();
        reqpq.Nonce = (Int128)nonce;

        TLExecutionContext context = new TLExecutionContext();
        var respq = reqpq.Execute(context);

        Assert.Equal(0x494C553B, (int)context.SessionBag["p"]);
        Assert.Equal(0x53911073, (int)context.SessionBag["q"]);

        Assert.Equal(nonce, (Int128)context.SessionBag["nonce"]);
        Assert.Equal(server_nonce, (Int128)context.SessionBag["server_nonce"]);
    }

    private static IContainer BuildContainer()
    {
        var tl = Assembly.Load("Ferrite.TL");
        var builder = new ContainerBuilder();
        builder.RegisterType<MockRandomGenerator>().As<IRandomGenerator>();
        builder.RegisterType<MockKeyPairProvider>().As<IKeyProvider>();
        builder.RegisterAssemblyTypes(tl)
            .Where(t => t.Namespace == "Ferrite.TL.mtproto")
            .AsSelf();
        builder.RegisterType<Int128>();
        builder.RegisterType<Int256>();
        builder.RegisterType<TLObjectFactory>().As<ITLObjectFactory>();
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

    [Fact]
    public void ReqDhParams_ShouldReturnServerDhParams()
    {
        IContainer container = BuildContainer();

        TLExecutionContext context = new TLExecutionContext();
        context.SessionBag.Add("nonce", nonce);
        context.SessionBag.Add("server_nonce", server_nonce);
        context.SessionBag.Add("p", 0x494C553B);
        context.SessionBag.Add("q", 0x53911073);

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


        PQInnerDataDc pQInnerDataDc = container.Resolve<PQInnerDataDc>();
        pQInnerDataDc.Pq = pq;
        pQInnerDataDc.P = pBytes;
        pQInnerDataDc.Q = qBytes;
        pQInnerDataDc.Nonce = (Int128)nonce;
        pQInnerDataDc.ServerNonce = (Int128)server_nonce;
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        pQInnerDataDc.NewNonce = (Int256)randomBytes;
        pQInnerDataDc.Dc = 1;
        byte[] sha1Received, sha1Actual;
        int constructor;
        ServerDhInnerData serverDhInnerData;
        ProcessReqDhParams(container, context, reqDhParams, pQInnerDataDc,
            out sha1Received, out constructor, out serverDhInnerData, out sha1Actual);

        Assert.Equal((int)TLConstructor.ServerDhInnerData, constructor);
        Assert.Equal(sha1Actual, sha1Received);
        Assert.Equal(serverDhInnerData.G, (int)context.SessionBag["g"]);
        Assert.Equal(serverDhInnerData.GA, (byte[])context.SessionBag["g_a"]);
        Assert.Equal(serverDhInnerData.DhPrime, dhPrime);
        Assert.True(gs.Contains(serverDhInnerData.G));
    }
    [Fact]
    public void ReqDhParamsTemp_ShouldReturnServerDhParams()
    {
        IContainer container = BuildContainer();

        TLExecutionContext context = new TLExecutionContext();
        context.SessionBag.Add("nonce", nonce);
        context.SessionBag.Add("server_nonce", server_nonce);
        context.SessionBag.Add("p", 0x494C553B);
        context.SessionBag.Add("q", 0x53911073);

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
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        pQInnerDataDc.NewNonce = (Int256)randomBytes;
        pQInnerDataDc.Dc = 1;
        pQInnerDataDc.ExpiresIn = 1000;
        byte[] sha1Received, sha1Actual;
        int constructor;
        ServerDhInnerData serverDhInnerData;
        ProcessReqDhParams2(container, context, reqDhParams, pQInnerDataDc,
            out sha1Received, out constructor, out serverDhInnerData, out sha1Actual);
        Assert.True(context.SessionBag.ContainsKey("valid_until"));
        Assert.Equal((int)TLConstructor.ServerDhInnerData, constructor);
        Assert.Equal(sha1Actual, sha1Received);
        Assert.Equal(serverDhInnerData.G, (int)context.SessionBag["g"]);
        Assert.Equal(serverDhInnerData.GA, (byte[])context.SessionBag["g_a"]);
        Assert.Equal(serverDhInnerData.DhPrime, dhPrime);
        Assert.True(gs.Contains(serverDhInnerData.G));
    }

    [Fact]
    public void SetClientDhParams_ShouldReturnSetClientDhParamsAnswer()
    {
        IContainer container = BuildContainer();

        TLExecutionContext context = new TLExecutionContext();
        context.SessionBag.Add("nonce", nonce);
        context.SessionBag.Add("server_nonce", server_nonce);
        context.SessionBag.Add("p", 0x494C553B);
        context.SessionBag.Add("q", 0x53911073);
        BigInteger prime = new BigInteger(dhPrime, true, true);
        var aBytes = RandomNumberGenerator.GetBytes(2048);
        BigInteger a = new BigInteger(aBytes, true, true);
        while (a < prime)
        {
            aBytes = RandomNumberGenerator.GetBytes(2048);
            a = new BigInteger(aBytes, true, true);
        }
        BigInteger g = new BigInteger(gs[RandomNumberGenerator.GetInt32(6)]);
        BigInteger g_a = BigInteger.ModPow(g, a, prime);
        context.SessionBag.Add("g", (int)g);
        context.SessionBag.Add("g_a", g_a.ToByteArray(true, true));
        context.SessionBag.Add("new_nonce", RandomNumberGenerator.GetBytes(32));

        var setClientDhParams = container.Resolve<SetClientDhParams>();
        setClientDhParams.Nonce = (Int128)nonce;
        setClientDhParams.ServerNonce = (Int128)server_nonce;
    }

    private void ProcessReqDhParams(IContainer container, TLExecutionContext context, ReqDhParams reqDhParams, PQInnerDataDc pQInnerDataDc, out byte[] sha1Received, out int constructor, out ServerDhInnerData serverDhInnerData, out byte[] sha1Actual)
    {
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

        ServerDhParamsOk serverDhParamsOk = (ServerDhParamsOk)reqDhParams.Execute(context);
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

        Aes aes = Aes.Create();
        aes.Key = tmpAesKey;
        aes.DecryptIge(serverDhParamsOk.EncryptedAnswer, tmpAesIV);
        ITLObjectFactory factory = container.Resolve<ITLObjectFactory>();

        sha1Received = serverDhParamsOk.EncryptedAnswer.AsSpan().Slice(0, 20).ToArray();
        SequenceReader reader = IAsyncBinaryReader.Create(serverDhParamsOk.EncryptedAnswer.AsSpan().Slice(20).ToArray());
        constructor = reader.ReadInt32(true);
        serverDhInnerData = (ServerDhInnerData)factory.Read(constructor, ref reader);
        sha1Actual = SHA1.HashData(serverDhInnerData.TLBytes.ToArray());
    }
    private void ProcessReqDhParams2(IContainer container, TLExecutionContext context, ReqDhParams reqDhParams, PQInnerDataTempDc pQInnerDataDc, out byte[] sha1Received, out int constructor, out ServerDhInnerData serverDhInnerData, out byte[] sha1Actual)
    {
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

        ServerDhParamsOk serverDhParamsOk = (ServerDhParamsOk)reqDhParams.Execute(context);
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

        Aes aes = Aes.Create();
        aes.Key = tmpAesKey;
        aes.DecryptIge(serverDhParamsOk.EncryptedAnswer, tmpAesIV);
        ITLObjectFactory factory = container.Resolve<ITLObjectFactory>();

        sha1Received = serverDhParamsOk.EncryptedAnswer.AsSpan().Slice(0, 20).ToArray();
        SequenceReader reader = IAsyncBinaryReader.Create(serverDhParamsOk.EncryptedAnswer.AsSpan().Slice(20).ToArray());
        constructor = reader.ReadInt32(true);
        serverDhInnerData = (ServerDhInnerData)factory.Read(constructor, ref reader);
        sha1Actual = SHA1.HashData(serverDhInnerData.TLBytes.ToArray());
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
