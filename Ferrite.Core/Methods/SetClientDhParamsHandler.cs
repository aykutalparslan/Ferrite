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

using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.Exceptions;
using Ferrite.TL.mtproto;
using Ferrite.TL.slim;
using Ferrite.TL.slim.mtproto;

namespace Ferrite.Core.Methods;

public class SetClientDhParamsHandler : IQueryHandler
{
    private readonly IMTProtoService _mtproto;
    private bool serialized = false;
    //TODO: Maybe change the DH_PRIME
    private const string dhPrime = "C71CAEB9C6B1C9048E6C522F70F13F73980D40238E3E21C14934D037563D930F48198A0AA7C14058229493D22530F4DBFA336F6E0AC925139543AED44CCE7C3720FD51F69458705AC68CD4FE6B6B13ABDC9746512969328454F18FAF8C595F642477FE96BB2A941D5BCD1D4AC8CC49880708FA9B378E3C4F3A9060BEE67CF9A4A4A695811051907E162753B56B0F6B410DBA74D8A84B2A14B3144E0EF1284754FD17ED950D5965B4B9DD46582DB1178D169C6BC465B0D6FF9CA3928FEF5B9AE4E418FC15E83EBEA0F87FA9FF5EED70050DED2849F47BF959D956850CE929851F0D8115F635B105EE2E4E15D04B2454BF6F4FADF034B10403119CD8E3B92FCC5B";
    public SetClientDhParamsHandler(IMTProtoService mtproto)
    {
        _mtproto = mtproto;
    }
    public async Task<TLBytes?> Process(TLBytes q, TLExecutionContext ctx)
    {
        return ProcessInternal(new set_client_DH_params(q.AsSpan()), ctx);
    }

    private TLBytes? ProcessInternal(set_client_DH_params query, TLExecutionContext ctx)
    {
        bool failed = false;
        var sessionNonce = (byte[])ctx.SessionData["nonce"];
        var sessionServerNonce = (byte[])ctx.SessionData["server_nonce"];
        if (!query.nonce.SequenceEqual(sessionNonce) ||
            !query.server_nonce.SequenceEqual(sessionServerNonce))
        {
            failed = true;
        }

        Aes aes = Aes.Create();
        aes.Key = (byte[])ctx.SessionData["temp_aes_key"];
        using var encryptedData = UnmanagedMemoryAllocator.Allocate<byte>(query.encrypted_data.Length);
        aes.DecryptIge(query.encrypted_data, ((byte[])ctx.SessionData["temp_aes_iv"]).ToArray(),
            encryptedData.Span);
        var sha1Received = encryptedData.Span[..20].ToArray();
        var dataWithPadding = encryptedData.Memory[20..];
        var len = client_DH_inner_data.ReadSize(dataWithPadding.Span, 0);
        var clientDhInnerData = new client_DH_inner_data(dataWithPadding.Span[..len]);
        var sha1Actual = SHA1.HashData(clientDhInnerData.ToReadOnlySpan());
        if (!sha1Actual.SequenceEqual(sha1Received) ||
            !query.nonce.SequenceEqual(sessionNonce) ||
            !query.server_nonce.SequenceEqual(sessionServerNonce) ||
            !clientDhInnerData.nonce.SequenceEqual(sessionNonce) ||
            !clientDhInnerData.server_nonce.SequenceEqual(sessionServerNonce))
        {
            failed = true;
        }

        BigInteger prime = BigInteger.Parse("0" + dhPrime, NumberStyles.HexNumber);
        BigInteger g_b = new BigInteger(clientDhInnerData.g_b, true, true);
        BigInteger g = new BigInteger((int)ctx.SessionData["g"]);
        BigInteger a = new BigInteger((byte[])ctx.SessionData["a"], true, true);
        var authKey = BigInteger.ModPow(g_b, a, prime).ToByteArray(true, true);
        ctx.SessionData.Add("auth_key", authKey);
        var authKeySHA1 = SHA1.HashData(authKey);
        var authKeyHash = MemoryMarshal.Cast<byte, long>(authKeySHA1.AsSpan().Slice(12))[0];
        var authKeyAuxHash = authKeySHA1.Take(8).ToArray();
        var newNonceHash1 = SHA1.HashData(((byte[])ctx.SessionData["new_nonce"]).Concat(new byte[1] { 1 })
            .Concat(authKeyAuxHash).ToArray()).Skip(4).ToArray();
        var newNonceHash3 = SHA1.HashData(((byte[])ctx.SessionData["new_nonce"])
                .Concat(new byte[1] { 2 }).Concat(authKeyAuxHash).ToArray())
            .Skip(4).ToArray();
        BigInteger min = BigInteger.Pow(new BigInteger(2), 2048 - 64);
        BigInteger max = prime - min;
        if (g_b <= min || g_b >= max || failed)
        {
            var dhGenFail = new dh_gen_fail(sessionNonce, sessionServerNonce, newNonceHash3);
            return dhGenFail.TLBytes;
        }

        bool temp_auth_key = false;
        if(ctx.SessionData.TryGetValue("temp_auth_key", out var key))
        {
            temp_auth_key = (bool)key;
        }
        
        var existingKey = temp_auth_key
            ? _mtproto.GetTempAuthKey(authKeyHash)
            : _mtproto.GetAuthKey(authKeyHash);
        if (existingKey == null || existingKey.Length == 0)
        {
            var authKeyTrimmed = authKey.AsSpan().Slice(0, 192).ToArray();
            if (temp_auth_key)
            {
                int expiresIn = (int)ctx.SessionData["temp_auth_key_expires_in"]; 
                _mtproto.PutTempAuthKey(authKeyHash, authKeyTrimmed, new TimeSpan(0, 0, expiresIn));
            }
            else
            {
                _mtproto.PutAuthKey(authKeyHash, authKeyTrimmed);
            }

            var dhGenOk = new dh_gen_ok(sessionNonce, sessionServerNonce, newNonceHash1);
            ctx.SessionData.Clear();
            return dhGenOk.TLBytes;
        }
        else
        {
            var newNonceHash2 = SHA1.HashData(((byte[])ctx.SessionData["new_nonce"])
                    .Concat(new byte[1] { 2 }).Concat(authKeyAuxHash).ToArray())
                .Skip(4).ToArray();
            var dhGenRetry = new dh_gen_retry(sessionNonce, sessionServerNonce, newNonceHash2);
            return dhGenRetry.TLBytes;
        }
    }
}