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
using System.Buffers;
using System.Security.Cryptography;
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Data;
using Ferrite.Utils;
using Ferrite.Crypto;
using System.Numerics;
using System.Globalization;

namespace Ferrite.TL.mtproto;
public class SetClientDhParams : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IPersistentStore dataStore;
    private bool serialized = false;
    //TODO: Maybe change the DH_PRIME
    private const string dhPrime = "C71CAEB9C6B1C9048E6C522F70F13F73980D40238E3E21C14934D037563D930F48198A0AA7C14058229493D22530F4DBFA336F6E0AC925139543AED44CCE7C3720FD51F69458705AC68CD4FE6B6B13ABDC9746512969328454F18FAF8C595F642477FE96BB2A941D5BCD1D4AC8CC49880708FA9B378E3C4F3A9060BEE67CF9A4A4A695811051907E162753B56B0F6B410DBA74D8A84B2A14B3144E0EF1284754FD17ED950D5965B4B9DD46582DB1178D169C6BC465B0D6FF9CA3928FEF5B9AE4E418FC15E83EBEA0F87FA9FF5EED70050DED2849F47BF959D956850CE929851F0D8115F635B105EE2E4E15D04B2454BF6F4FADF034B10403119CD8E3B92FCC5B";
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

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        bool failed = false;
        var sessionNonce = (Int128)ctx.SessionBag["nonce"];
        var sessionServerNonce = (Int128)ctx.SessionBag["server_nonce"];
        if (nonce != sessionNonce || serverNonce != sessionServerNonce)
        {
            failed = true;
        }
        Aes aes = Aes.Create();
        aes.Key = (byte[])ctx.SessionBag["temp_aes_key"];
        aes.DecryptIge(encryptedData, ((byte[])ctx.SessionBag["temp_aes_iv"]).ToArray());
        var sha1Received = encryptedData.AsSpan().Slice(0, 20).ToArray();
        var dataWithPadding = encryptedData.AsMemory().Slice(20);
        SequenceReader reader = IAsyncBinaryReader.Create(dataWithPadding);
        int constructor = reader.ReadInt32(true);
        if(constructor == TLConstructor.ClientDhInnerData)
        {
            var clientDhInnerData = factory.Read<ClientDhInnerData>(ref reader);
            var sha1Actual = SHA1.HashData(clientDhInnerData.TLBytes.ToArray());
            if (!sha1Actual.SequenceEqual(sha1Received) ||
                sessionNonce != nonce || sessionServerNonce != serverNonce ||
                sessionNonce != clientDhInnerData.Nonce ||
                sessionServerNonce != clientDhInnerData.ServerNonce)
            {
                failed = true;
            }
            BigInteger prime = BigInteger.Parse("0" + dhPrime, NumberStyles.HexNumber);
            BigInteger g_b = new BigInteger(clientDhInnerData.GB, true, true);
            BigInteger g = new BigInteger((int)ctx.SessionBag["g"]);
            BigInteger a = new BigInteger((byte[])ctx.SessionBag["a"],true,true);
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
            BigInteger min = BigInteger.Pow(new BigInteger(2), 2048 - 64);
            BigInteger max = prime - min;
            if(g_b <=min || g_b >= max || failed)
            {
                var dhGenFail = factory.Resolve<DhGenFail>();
                dhGenFail.Nonce = sessionNonce;
                dhGenFail.ServerNonce = sessionServerNonce;
                dhGenFail.NewNonceHash3 = (Int128)newNonceHash3;
                return dhGenFail;
            }
            if (dataStore.GetAuthKey(authKeyHash) == null)
            {
                dataStore.SaveAuthKey(authKeyHash, authKey.AsSpan().Slice(0, 192));
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