/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using System;
using System.Buffers;
using System.Security.Cryptography;
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Data;
using Ferrite.Utils;
using Ferrite.Crypto;
using System.Numerics;

namespace Ferrite.TL.mtproto;
public class SetClientDhParams : ITLObject
{
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private ITLObjectFactory factory;
    private IPersistentStore dataStore;
    private bool serialized = false;
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
    public SetClientDhParams(ITLObjectFactory objectFactory, IPersistentStore store)
    {
        factory = objectFactory;
        dataStore = store;
    }

    public int Constructor => -184262881;
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
        var sessionNonce = (Int128)(byte[])ctx.SessionBag["nonce"];
        var sessionServerNonce = (Int128)(byte[])ctx.SessionBag["server_nonce"];
        if (nonce != sessionNonce || serverNonce != sessionServerNonce)
        {
            var dhGenFail = factory.Resolve<DhGenFail>();
            dhGenFail.Nonce = sessionNonce;
            dhGenFail.ServerNonce = sessionServerNonce;
            dhGenFail.NewNonceHash3 = new Int128();
            return dhGenFail;
        }
        Aes aes = Aes.Create();
        aes.Key = (byte[])ctx.SessionBag["temp_aes_key"];
        aes.DecryptIge(encryptedData, ((byte[])ctx.SessionBag["temp_aes_iv"]).ToArray());
        var sha1Received = encryptedData.AsSpan().Slice(0, 20);
        var dataWithPadding = encryptedData.AsMemory().Slice(20);
        SequenceReader reader = IAsyncBinaryReader.Create(dataWithPadding);
        int constructor = reader.ReadInt32(true);
        if(constructor == (int)TLConstructor.ClientDhInnerData)
        {
            var clientDhInnerData = factory.Read<ClientDhInnerData>(ref reader);
            //var sha1Actual = SHA1.HashData(encryptedData.AsSpan()
            //    .Slice(20, (int)clientDhInnerData.TLBytes.Length)).AsSpan();
            var sha1Actual = SHA1.HashData(clientDhInnerData.TLBytes.ToArray()).AsSpan();
            if (!sha1Actual.SequenceEqual(sha1Received) ||
                sessionNonce != nonce || sessionServerNonce != serverNonce ||
                sessionNonce != clientDhInnerData.Nonce ||
                sessionServerNonce != clientDhInnerData.ServerNonce)
            {
                return null;
            }
            BigInteger prime = new BigInteger(dhPrime, true, true);
            BigInteger g_b = new BigInteger(clientDhInnerData.GB, true, true);
            BigInteger g = new BigInteger((int)ctx.SessionBag["g"]);
            BigInteger a = new BigInteger((byte[])ctx.SessionBag["a"], true, true);
            var authKey = BigInteger.ModPow(g_b, a, prime).ToByteArray(true, true);
            ctx.SessionBag.Add("auth_key", authKey);
            var authKeySHA1 = SHA1.HashData(authKey);
            var authKeyHash = authKeySHA1.Skip(12).ToArray();
            var authKeyAuxHash = authKeySHA1.Take(8).ToArray();
            var newNonceHash1 = SHA1.HashData(((byte[])ctx.SessionBag["new_nonce"]).Concat(new byte[1] { 1 })
                .Concat(authKeyAuxHash).ToArray()).Skip(4).ToArray();
            var newNonceHash3 = SHA1.HashData(((byte[])ctx.SessionBag["new_nonce"])
                    .Concat(new byte[1] { 2 }).Concat(authKeyAuxHash).ToArray())
                    .Skip(4).ToArray();
            if (dataStore.GetAuthKey(authKeyHash) == null)
            {
                dataStore.SaveAuthKey(authKeyHash, authKey);
                var dhGenOk = factory.Resolve<DhGenOk>();
                dhGenOk.Nonce = sessionNonce;
                dhGenOk.ServerNonce = sessionServerNonce;
                dhGenOk.NewNonceHash1 = (Int128)newNonceHash1;
                return dhGenOk;
            }
            else
            {
                var newNonceHash2 = SHA1.HashData(((byte[])ctx.SessionBag["new_nonce"])
                    .Concat(new byte[1] { 2 }).Concat(authKeyAuxHash).ToArray())
                    .Skip(4).ToArray();
                var dhGenRetry = factory.Resolve<DhGenRetry>();
                dhGenRetry.Nonce = sessionNonce;
                dhGenRetry.ServerNonce = sessionServerNonce;
                dhGenRetry.NewNonceHash2 = (Int128)newNonceHash2;
                return dhGenRetry;
            }
        }
        return null;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        nonce = factory.Read<Int128>(ref buff);
        serverNonce = factory.Read<Int128>(ref buff);
        encryptedData = buff.ReadTLBytes().ToArray();
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}