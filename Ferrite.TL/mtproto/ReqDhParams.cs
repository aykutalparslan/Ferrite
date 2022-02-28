/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using System;
using System.Buffers;
using System.Numerics;
using System.Security.Cryptography;
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Crypto;
using Ferrite.TL.Exceptions;
using Ferrite.Utils;

namespace Ferrite.TL.mtproto;
public class ReqDhParams : ITLObject
{
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    ITLObjectFactory factory;
    private bool serialized = false;
    public int Constructor => -686627650;
    private IKeyProvider keyProvider;
    private ILogger log;
    private IRandomGenerator random;
    private readonly int[] gs = new int[] { 2, 3, 4, 5, 6, 7 };
    //TODO: Maybe change the DH_PRIME
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
    public ReqDhParams(ITLObjectFactory objectFactory, IKeyProvider provider,
        IRandomGenerator generator, ILogger logger)
    {
        factory = objectFactory;
        keyProvider = provider;
        random = generator;
        this.log = logger;
    }
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(nonce.TLBytes, false);
            writer.Write(serverNonce.TLBytes, false);
            writer.WriteTLBytes(p);
            writer.WriteTLBytes(q);
            writer.WriteInt64(publicKeyFingerprint, true);
            writer.WriteTLBytes(encryptedData);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private Int128 nonce;
    public Int128 Nonce
    {
        get => nonce;
        set
        {
            serialized = false;
            nonce = value;
        }
    }

    private Int128 serverNonce;
    public Int128 ServerNonce
    {
        get => serverNonce;
        set
        {
            serialized = false;
            serverNonce = value;
        }
    }

    private byte[] p;
    public byte[] P
    {
        get => p;
        set
        {
            serialized = false;
            p = value;
        }
    }

    private byte[] q;
    public byte[] Q
    {
        get => q;
        set
        {
            serialized = false;
            q = value;
        }
    }

    private long publicKeyFingerprint;
    public long PublicKeyFingerprint
    {
        get => publicKeyFingerprint;
        set
        {
            serialized = false;
            publicKeyFingerprint = value;
        }
    }

    private byte[] encryptedData;
    public byte[] EncryptedData
    {
        get => encryptedData;
        set
        {
            serialized = false;
            encryptedData = value;
        }
    }

    public bool IsMethod => true;
    public ITLObject Execute(TLExecutionContext ctx)
    {
        RpcError rpcError;
        ServerDhParamsOk serverDhParamsOk = factory.Resolve<ServerDhParamsOk>();
        var rsaKey = keyProvider.GetKey(this.publicKeyFingerprint);
        if (rsaKey == null)
        {
            log.Debug("Could not obtain the RSA Key.");
            rpcError = factory.Resolve<RpcError>();
            rpcError.ErrorCode = -404;
            return rpcError;
        }
        Memory<byte> data;
        Span<byte> sha256;
        RSAPad(rsaKey, out data, out sha256);

        if (!sha256.SequenceEqual(data.Slice(224).Span))
        {
            log.Debug("SHA256 did not match.");
            rpcError = factory.Resolve<RpcError>();
            rpcError.ErrorCode = -404;
            return rpcError;
        }

        SequenceReader reader = IAsyncBinaryReader.Create(data.Slice(32, 192));

        int constructor = reader.ReadInt32(true);
        var sessionNonce = (Int128)(byte[])ctx.SessionBag["nonce"];
        var sessionServerNonce = (Int128)(byte[])ctx.SessionBag["server_nonce"];
        if (constructor == (int)TLConstructor.PQInnerDataDc)
        {
            var obj = factory.Read<PQInnerDataDc>(ref reader);
            ctx.SessionBag.Add("new_nonce", (byte[])obj.NewNonce);
            if (nonce != obj.Nonce ||
                nonce != sessionNonce ||
                serverNonce != obj.ServerNonce ||
                serverNonce != sessionServerNonce)
            {
                rpcError = factory.Resolve<RpcError>();
                rpcError.ErrorCode = -404;
                return rpcError;
            }
            var newNonceServerNonce = SHA1.HashData(((byte[])obj.NewNonce)
                .Concat((byte[])sessionServerNonce).ToArray());
            var serverNonceNewNonce = SHA1.HashData(((byte[])sessionServerNonce)
                .Concat((byte[])obj.NewNonce).ToArray());
            var newNonceNewNonce = SHA1.HashData(((byte[])obj.NewNonce)
                .Concat((byte[])obj.NewNonce).ToArray());
            var tmpAesKey = newNonceServerNonce
                .Concat(serverNonceNewNonce.SkipLast(8)).ToArray();
            var tmpAesIV = serverNonceNewNonce.Skip(12)
                .Concat(newNonceNewNonce).Concat(((byte[])obj.NewNonce).SkipLast(28)).ToArray();
            ctx.SessionBag.Add("temp_aes_key", tmpAesKey.ToArray());
            ctx.SessionBag.Add("temp_aes_iv", tmpAesIV.ToArray());
            byte[] answer = GenerateEncryptedAnswer(ctx, serverDhParamsOk, sessionNonce, sessionServerNonce, tmpAesKey, tmpAesIV);
            serverDhParamsOk.EncryptedAnswer = answer;

            return serverDhParamsOk;
        }
        else if (constructor == (int)TLConstructor.PQInnerDataTempDc)
        {
            var obj = factory.Read<PQInnerDataTempDc>(ref reader);
            ctx.SessionBag.Add("new_nonce", (byte[])obj.NewNonce);
            if (nonce != obj.Nonce ||
                nonce != sessionNonce ||
                serverNonce != obj.ServerNonce ||
                serverNonce != sessionServerNonce)
            {
                rpcError = factory.Resolve<RpcError>();
                rpcError.ErrorCode = -404;
                return rpcError;
            }
            var newNonceServerNonce = SHA1.HashData(((byte[])obj.NewNonce)
                .Concat((byte[])sessionServerNonce).ToArray());
            var serverNonceNewNonce = SHA1.HashData(((byte[])sessionServerNonce)
                .Concat((byte[])obj.NewNonce).ToArray());
            var newNonceNewNonce = SHA1.HashData(((byte[])obj.NewNonce)
                .Concat((byte[])obj.NewNonce).ToArray());
            var tmpAesKey = newNonceServerNonce
                .Concat(serverNonceNewNonce.SkipLast(8)).ToArray();
            var tmpAesIV = serverNonceNewNonce.Skip(12)
                .Concat(newNonceNewNonce).Concat(((byte[])obj.NewNonce).SkipLast(28)).ToArray();
            ctx.SessionBag.Add("temp_aes_key", tmpAesKey.ToArray());
            ctx.SessionBag.Add("temp_aes_iv", tmpAesIV.ToArray());
            byte[] answer = GenerateEncryptedAnswer(ctx, serverDhParamsOk, sessionNonce, sessionServerNonce, tmpAesKey, tmpAesIV);
            serverDhParamsOk.EncryptedAnswer = answer;
            ctx.SessionBag.Add("valid_until", DateTime.Now.AddSeconds(obj.ExpiresIn));
            return serverDhParamsOk;
        }

        return serverDhParamsOk;
    }

    private byte[] GenerateEncryptedAnswer(TLExecutionContext ctx, ServerDhParamsOk serverDhParamsOk, Int128 sessionNonce, Int128 sessionServerNonce, byte[] tmpAesKey, byte[] tmpAesIV)
    {
        serverDhParamsOk.Nonce = sessionNonce;
        serverDhParamsOk.ServerNonce = sessionServerNonce;

        var serverDhInnerData = factory.Resolve<ServerDhInnerData>();
        BigInteger prime = new BigInteger(dhPrime, true, true);
        var aBytes = random.GetRandomBytes(2048);
        BigInteger a = new BigInteger(aBytes, true, true);
        while (a < prime)
        {
            aBytes = random.GetRandomBytes(2048);
            a = new BigInteger(aBytes, true, true);
        }
        BigInteger g = new BigInteger(gs[random.GetRandomNumber(6)]);
        BigInteger g_a = BigInteger.ModPow(g, a, prime);

        serverDhInnerData.Nonce = sessionNonce;
        serverDhInnerData.ServerNonce = sessionServerNonce;
        serverDhInnerData.DhPrime = prime.ToByteArray(true, true);
        serverDhInnerData.G = (int)g;
        serverDhInnerData.GA = g_a.ToByteArray(true, true);
        serverDhInnerData.ServerTime = (int)new TimeSpan(DateTime.Now.Ticks).TotalSeconds;

        ctx.SessionBag.Add("g", serverDhInnerData.G);
        ctx.SessionBag.Add("a", a.ToByteArray(true,true));
        ctx.SessionBag.Add("g_a", serverDhInnerData.GA);

        var buff = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
        buff.Write(SHA1.HashData(serverDhInnerData.TLBytes.IsSingleSegment ?
            serverDhInnerData.TLBytes.FirstSpan : serverDhInnerData.TLBytes.ToArray()));
        buff.Write(serverDhInnerData.TLBytes, false);
        if (buff.WrittenCount % 16 != 0)
        {
            for (int i = 0; i < 16 - buff.WrittenCount % 16; i++)
            {
                buff.Write((byte)0);
            }
        }

        Aes aes = Aes.Create();
        aes.Key = tmpAesKey;
        var answerWithHash = new byte[buff.WrittenCount];
        buff.ToReadOnlySequence().CopyTo(answerWithHash);
        aes.EncryptIge(answerWithHash, tmpAesIV);
        return answerWithHash;
    }

    private void RSAPad(IRSAKey rsaKey, out Memory<byte> data,
        out Span<byte> sha256)
    {
        data = rsaKey.DecryptBlock(encryptedData).AsMemory();
        // data: |-temp_key_xor(32)-|-|-aes_encrypted(224)-| 256 bytes
        Span<byte> tempKey = data.Slice(0, 32).Span;
        Span<byte> aesEncrypted = data.Slice(32).Span;

        byte[] sha256AesEncrypted = SHA256.HashData(aesEncrypted);
        for (int i = 0; i < 32; i++)
        {
            tempKey[i] = (byte)(tempKey[i] ^ sha256AesEncrypted[i]);
        }
        // data: |-temp_key(32)+aes_encrypted(224)-| 256 bytes
        Aes aes = Aes.Create();
        aes.Key = tempKey.ToArray();
        aes.DecryptIge(aesEncrypted, stackalloc byte[32]);
        // data: |-temp_key(32)+data_with_hash(224)-| 256 bytes
        // data_with_hash: |-data_pad_reversed(192)+
        //                   SHA256(temp_key+data_pad)(32)-| 256 bytes
        Span<byte> dataPadReversed = aesEncrypted.Slice(0, 192);
        dataPadReversed.Reverse();
        // data: |-temp_key(32)+data_pad(192)+
        //                   SHA256(temp_key+data_pad)(32)-| 256 bytes
        sha256 = SHA256.HashData(data.Slice(0, 224).Span).AsSpan();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        nonce = factory.Read<Int128>(ref buff);
        serverNonce = factory.Read<Int128>(ref buff);
        p = buff.ReadTLBytes().ToArray();
        q = buff.ReadTLBytes().ToArray();
        publicKeyFingerprint = buff.ReadInt64(true);
        encryptedData = buff.ReadTLBytes().ToArray();
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}