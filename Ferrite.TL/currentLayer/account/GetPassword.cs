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
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer.account;
public class GetPassword : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    private readonly int[] gs = new int[] { 3, 4, 7 };
    //TODO: Maybe change the DH_PRIME
    private const string dhPrime = "C71CAEB9C6B1C9048E6C522F70F13F73980D40238E3E21C14934D037563D930F48198A0AA7C14058229493D22530F4DBFA336F6E0AC925139543AED44CCE7C3720FD51F69458705AC68CD4FE6B6B13ABDC9746512969328454F18FAF8C595F642477FE96BB2A941D5BCD1D4AC8CC49880708FA9B378E3C4F3A9060BEE67CF9A4A4A695811051907E162753B56B0F6B410DBA74D8A84B2A14B3144E0EF1284754FD17ED950D5965B4B9DD46582DB1178D169C6BC465B0D6FF9CA3928FEF5B9AE4E418FC15E83EBEA0F87FA9FF5EED70050DED2849F47BF959D956850CE929851F0D8115F635B105EE2E4E15D04B2454BF6F4FADF034B10403119CD8E3B92FCC5B";

    public GetPassword(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => 1418342645;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var rpcResult = factory.Resolve<RpcResult>();
        rpcResult.ReqMsgId = ctx.MessageId;
        var password = factory.Resolve<PasswordImpl>();
        var algo = factory.Resolve<PasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPowImpl>();
        algo.Salt1 = RandomNumberGenerator.GetBytes(8);
        algo.Salt2 = RandomNumberGenerator.GetBytes(16);
        algo.G = 3;
        BigInteger prime = BigInteger.Parse("0"+dhPrime, NumberStyles.HexNumber);
        algo.P = prime.ToByteArray(true,true);
        password.NewAlgo = algo;
        var secureAlgo = factory.Resolve<SecurePasswordKdfAlgoPBKDF2HMACSHA512iter100000Impl>();
        secureAlgo.Salt = RandomNumberGenerator.GetBytes(8);
        password.NewSecureAlgo = secureAlgo;
        password.SecureRandom = RandomNumberGenerator.GetBytes(256);
        rpcResult.Result = password;
        return rpcResult;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}