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

using System.Buffers;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Crypto;
using Ferrite.TL;
using Ferrite.TL.mtproto;
using Ferrite.TL.slim;
using Ferrite.TL.slim.mtproto;
using Ferrite.Utils;

namespace Ferrite.Core.Methods;

public class ReqDhParamsHandler : IQueryHandler
{
    private IKeyProvider keyProvider;
    private ILogger log;
    private IRandomGenerator random;
    private readonly int[] gs = new int[] { 3, 4, 7 };
    //TODO: Maybe change the DH_PRIME
    private const string dhPrime = "C71CAEB9C6B1C9048E6C522F70F13F73980D40238E3E21C14934D037563D930F48198A0AA7C14058229493D22530F4DBFA336F6E0AC925139543AED44CCE7C3720FD51F69458705AC68CD4FE6B6B13ABDC9746512969328454F18FAF8C595F642477FE96BB2A941D5BCD1D4AC8CC49880708FA9B378E3C4F3A9060BEE67CF9A4A4A695811051907E162753B56B0F6B410DBA74D8A84B2A14B3144E0EF1284754FD17ED950D5965B4B9DD46582DB1178D169C6BC465B0D6FF9CA3928FEF5B9AE4E418FC15E83EBEA0F87FA9FF5EED70050DED2849F47BF959D956850CE929851F0D8115F635B105EE2E4E15D04B2454BF6F4FADF034B10403119CD8E3B92FCC5B";
    public ReqDhParamsHandler(IKeyProvider provider, IRandomGenerator generator, ILogger logger)
    {
        keyProvider = provider;
        random = generator;
        this.log = logger;
    }
    public async Task<TLBytes?> Process(TLBytes q, TLExecutionContext ctx)
    {
        return ProcessInternal(new req_DH_params(q.AsSpan()), ctx);
    }

    private TLBytes? ProcessInternal(req_DH_params query, TLExecutionContext ctx)
    {
        var rsaKey = keyProvider.GetKey(query.public_key_fingerprint);
        if (rsaKey == null)
        {
            var rpcError = new rpc_error(-404, ""u8);
            log.Debug("Could not obtain the RSA Key.");
            return rpcError.TLBytes;
        }
        if(!ctx.SessionData.ContainsKey("nonce") || 
                !ctx.SessionData.ContainsKey("server_nonce"))
        {
            var rpcError = new rpc_error(-404, ""u8);
            log.Debug("Session is empty.");
            return rpcError.TLBytes;
        }
        Memory<byte> data;
        byte[] sha256;
        RSAPad(query.encrypted_data.ToArray() ,rsaKey, out data, out sha256);

        if (!sha256.AsSpan().SequenceEqual(data.Span.Slice(224)))
        {
            log.Debug("SHA256 did not match.");
            var rpcError = new rpc_error(-404, ""u8);
            return rpcError.TLBytes;
        }

        var constructor = MemoryMarshal.Read<int>(data.Span[32..]);
        
        var sessionNonce = (byte[])ctx.SessionData["nonce"];
        var sessionServerNonce = (byte[])ctx.SessionData["server_nonce"];
        if (constructor == Constructors.p_q_inner_data)
        {
            var len = p_q_inner_data.ReadSize(data.Span, 32);
            var pQInnerData = new p_q_inner_data(data.Span.Slice(32, len));
            ctx.SessionData.Add("new_nonce", pQInnerData.new_nonce.ToArray());
            if (!query.nonce.SequenceEqual(pQInnerData.nonce) ||
                !query.nonce.SequenceEqual(sessionNonce) ||
                !query.server_nonce.SequenceEqual(pQInnerData.server_nonce) ||
                !query.server_nonce.SequenceEqual(sessionServerNonce))
            {
                var rpcError = new rpc_error(-404, "Nonce values did not match."u8);
                return rpcError.TLBytes;
            }
            var inner_new_nonce = pQInnerData.new_nonce.ToArray();
            var newNonceServerNonce = SHA1.HashData((inner_new_nonce)
                .Concat((byte[])sessionServerNonce).ToArray());
            var serverNonceNewNonce = SHA1.HashData(((byte[])sessionServerNonce)
                .Concat(inner_new_nonce).ToArray());
            var newNonceNewNonce = SHA1.HashData((inner_new_nonce)
                .Concat(inner_new_nonce).ToArray());
            var tmpAesKey = newNonceServerNonce
                .Concat(serverNonceNewNonce.SkipLast(8)).ToArray();
            var tmpAesIV = serverNonceNewNonce.Skip(12)
                .Concat(newNonceNewNonce).Concat((inner_new_nonce).SkipLast(28)).ToArray();
            ctx.SessionData.Add("temp_aes_key", tmpAesKey.ToArray());
            ctx.SessionData.Add("temp_aes_iv", tmpAesIV.ToArray());
            using var answer = GenerateEncryptedAnswer(ctx, sessionNonce, sessionServerNonce, tmpAesKey, tmpAesIV);
            var serverDhParamsOk = new server_DH_params_ok(query.nonce, query.server_nonce,answer.Memory.Span);
            
            return serverDhParamsOk.TLBytes;
        }
        else if (constructor == Constructors.p_q_inner_data_dc)
        {
            var len = p_q_inner_data_dc.ReadSize(data.Span, 32);
            var pQInnerDataDc = new p_q_inner_data_dc(data.Span.Slice(32, len));
            ctx.SessionData.Add("new_nonce", pQInnerDataDc.new_nonce.ToArray());
            if (!query.nonce.SequenceEqual(pQInnerDataDc.nonce) ||
                !query.nonce.SequenceEqual(sessionNonce) ||
                !query.server_nonce.SequenceEqual(pQInnerDataDc.server_nonce) ||
                !query.server_nonce.SequenceEqual(sessionServerNonce))
            {
                var rpcError = new rpc_error(-404, "Nonce values did not match."u8);
                return rpcError.TLBytes;
            }
            var inner_new_nonce = pQInnerDataDc.new_nonce.ToArray();
            var newNonceServerNonce = SHA1.HashData((inner_new_nonce)
                .Concat((byte[])sessionServerNonce).ToArray());
            var serverNonceNewNonce = SHA1.HashData(((byte[])sessionServerNonce)
                .Concat(inner_new_nonce).ToArray());
            var newNonceNewNonce = SHA1.HashData((inner_new_nonce)
                .Concat(inner_new_nonce).ToArray());
            var tmpAesKey = newNonceServerNonce
                .Concat(serverNonceNewNonce.SkipLast(8)).ToArray();
            var tmpAesIV = serverNonceNewNonce.Skip(12)
                .Concat(newNonceNewNonce).Concat((inner_new_nonce).SkipLast(28)).ToArray();
            ctx.SessionData.Add("temp_aes_key", tmpAesKey.ToArray());
            ctx.SessionData.Add("temp_aes_iv", tmpAesIV.ToArray());
            using var answer = GenerateEncryptedAnswer(ctx, sessionNonce, sessionServerNonce, tmpAesKey, tmpAesIV);
            var serverDhParamsOk = new server_DH_params_ok(query.nonce, query.server_nonce,answer.Memory.Span);
            return serverDhParamsOk.TLBytes;
        }
        else if (constructor == Constructors.p_q_inner_data_temp_dc)
        {
            var len = p_q_inner_data_temp_dc.ReadSize(data.Span, 32);
            var pQInnerDataTempDc = new p_q_inner_data_temp_dc(data.Span.Slice(32, len));
            ctx.SessionData.Add("temp_auth_key", true);
            ctx.SessionData.Add("temp_auth_key_expires_in", pQInnerDataTempDc.expires_in);
            ctx.SessionData.Add("new_nonce", pQInnerDataTempDc.new_nonce.ToArray());
            if (!query.nonce.SequenceEqual(pQInnerDataTempDc.nonce) ||
                !query.nonce.SequenceEqual(sessionNonce) ||
                !query.server_nonce.SequenceEqual(pQInnerDataTempDc.server_nonce) ||
                !query.server_nonce.SequenceEqual(sessionServerNonce))
            {
                var rpcError = new rpc_error(-404, "Nonce values did not match."u8);
                return rpcError.TLBytes;
            }
            var inner_new_nonce = pQInnerDataTempDc.new_nonce.ToArray();
            var newNonceServerNonce = SHA1.HashData((inner_new_nonce)
                .Concat((byte[])sessionServerNonce).ToArray());
            var serverNonceNewNonce = SHA1.HashData(((byte[])sessionServerNonce)
                .Concat(inner_new_nonce).ToArray());
            var newNonceNewNonce = SHA1.HashData((inner_new_nonce)
                .Concat(inner_new_nonce).ToArray());
            var tmpAesKey = newNonceServerNonce
                .Concat(serverNonceNewNonce.SkipLast(8)).ToArray();
            var tmpAesIV = serverNonceNewNonce.Skip(12)
                .Concat(newNonceNewNonce).Concat((inner_new_nonce).SkipLast(28)).ToArray();
            ctx.SessionData.Add("temp_aes_key", tmpAesKey.ToArray());
            ctx.SessionData.Add("temp_aes_iv", tmpAesIV.ToArray());
            using var answer = GenerateEncryptedAnswer(ctx, sessionNonce, sessionServerNonce, tmpAesKey, tmpAesIV);
            ctx.SessionData.Add("valid_until", DateTime.Now.AddSeconds(pQInnerDataTempDc.expires_in));
            var serverDhParamsOk = new server_DH_params_ok(query.nonce, query.server_nonce,answer.Memory.Span);
            return serverDhParamsOk.TLBytes;
        }
        else if (constructor == Constructors.p_q_inner_data_temp)
        {
            var len = p_q_inner_data_temp.ReadSize(data.Span, 32);
            var pQInnerDataTemp = new p_q_inner_data_temp(data.Span.Slice(32, len));
            ctx.SessionData.Add("temp_auth_key", true);
            ctx.SessionData.Add("temp_auth_key_expires_in", pQInnerDataTemp.expires_in);
            ctx.SessionData.Add("new_nonce", pQInnerDataTemp.new_nonce.ToArray());
            if (!query.nonce.SequenceEqual(pQInnerDataTemp.nonce) ||
                !query.nonce.SequenceEqual(sessionNonce) ||
                !query.server_nonce.SequenceEqual(pQInnerDataTemp.server_nonce) ||
                !query.server_nonce.SequenceEqual(sessionServerNonce))
            {
                var rpcError = new rpc_error(-404, "Nonce values did not match."u8);
                return rpcError.TLBytes;
            }
            var inner_new_nonce = pQInnerDataTemp.new_nonce.ToArray();
            var newNonceServerNonce = SHA1.HashData((inner_new_nonce)
                .Concat((byte[])sessionServerNonce).ToArray());
            var serverNonceNewNonce = SHA1.HashData(((byte[])sessionServerNonce)
                .Concat(inner_new_nonce).ToArray());
            var newNonceNewNonce = SHA1.HashData((inner_new_nonce)
                .Concat(inner_new_nonce).ToArray());
            var tmpAesKey = newNonceServerNonce
                .Concat(serverNonceNewNonce.SkipLast(8)).ToArray();
            var tmpAesIV = serverNonceNewNonce.Skip(12)
                .Concat(newNonceNewNonce).Concat((inner_new_nonce).SkipLast(28)).ToArray();
            ctx.SessionData.Add("temp_aes_key", tmpAesKey.ToArray());
            ctx.SessionData.Add("temp_aes_iv", tmpAesIV.ToArray());
            using var answer = GenerateEncryptedAnswer(ctx, sessionNonce, sessionServerNonce, tmpAesKey, tmpAesIV);
            ctx.SessionData.Add("valid_until", DateTime.Now.AddSeconds(pQInnerDataTemp.expires_in));
            var serverDhParamsOk = new server_DH_params_ok(query.nonce, query.server_nonce,answer.Memory.Span);
            return serverDhParamsOk.TLBytes;
        }
        return null;
    }
    private IMemoryOwner<byte> GenerateEncryptedAnswer(TLExecutionContext ctx, byte[] sessionNonce, byte[] sessionServerNonce, byte[] tmpAesKey, byte[] tmpAesIV)
    {
        BigInteger prime = BigInteger.Parse("0"+dhPrime, NumberStyles.HexNumber);
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
        
        var innerNonce = sessionNonce;
        var innerServerNonce = sessionServerNonce;
        var innerDhPrime = prime.ToByteArray(true,true);
        var innerG = (int)g;
        var innerGA = g_a.ToByteArray(true, true);
        var innerServerTime = (int)DateTimeOffset.Now.ToUnixTimeSeconds();

        using var serverDhInnerData = new server_DH_inner_data(innerNonce, innerServerNonce, innerG,
            innerDhPrime, innerGA, innerServerTime);
        
        ctx.SessionData.Add("g", innerG);
        ctx.SessionData.Add("a", a.ToByteArray(true,true));
        ctx.SessionData.Add("g_a", innerGA);
        int len = 20 + serverDhInnerData.Length;
        while (len % 16 != 0)
        {
            len++;
        }

        var answerWithHash = UnmanagedMemoryAllocator.Allocate<byte>(len);
        var innerSpan = serverDhInnerData.ToReadOnlySpan();
        SHA1.HashData( innerSpan, answerWithHash.Span[..20]);
        innerSpan.CopyTo(answerWithHash.Span[20..]);

        Aes aes = Aes.Create();
        aes.Key = tmpAesKey;
        aes.EncryptIge(answerWithHash.Span, tmpAesIV);
        return answerWithHash;
    }

    private void RSAPad(byte[] encryptedData, IRSAKey rsaKey, out Memory<byte> data, out byte[] sha256)
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
        sha256 = SHA256.HashData(data.Slice(0, 224).Span);
    }
}