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
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Crypto;
using System.Numerics;

namespace Ferrite.TL.mtproto;
public class ReqPqMulti : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    private IRandomGenerator randomGenerator;
    private IKeyProvider keyPairProvider;
    public ReqPqMulti(ITLObjectFactory objectFactory, IRandomGenerator generator, IKeyProvider provider)
    {
        factory = objectFactory;
        randomGenerator = generator;
        keyPairProvider = provider;
    }

    public int Constructor => -1099002127;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(nonce.TLBytes, false);
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
    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        ResPQ respq = factory.Resolve<ResPQ>();
        if (!ctx.SessionData.ContainsKey("nonce") ||
            (Int128)ctx.SessionData["nonce"] != nonce)
        {
            ctx.SessionData.Add("nonce", (byte[])nonce);
            respq.ServerNonce = (Int128)randomGenerator.GetRandomBytes(16);
            ctx.SessionData.Add("server_nonce", (byte[])respq.ServerNonce);
        }
        else
        {
            respq.ServerNonce = (Int128)ctx.SessionData["server_nonce"];
        }
        respq.Nonce = nonce;
        if (ctx.SessionData.ContainsKey("p"))
        {
            ctx.SessionData.Remove("p");
        }
        if (ctx.SessionData.ContainsKey("q"))
        {
            ctx.SessionData.Remove("q");
        }

        int a = randomGenerator.GetRandomPrime();
        int b = randomGenerator.GetRandomPrime();
        BigInteger pq = new BigInteger(a) * b;
        if (a < b)
        {
            ctx.SessionData.Add("p", a);
            ctx.SessionData.Add("q", b);
        }
        else
        {
            ctx.SessionData.Add("p", b);
            ctx.SessionData.Add("q", a);
        }

        respq.Pq = pq.ToByteArray(isBigEndian: true);

        //ctx.SessionBag.Add("pq", respq.Pq);
        VectorOfLong fingerprints = new VectorOfLong();
        
        foreach (var val in keyPairProvider.GetRSAFingerprints())
        {
            fingerprints.Add(val);
        }
        respq.ServerPublicKeyFingerprints = fingerprints;
        return respq;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        nonce = factory.Read<Int128>(ref buff);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}