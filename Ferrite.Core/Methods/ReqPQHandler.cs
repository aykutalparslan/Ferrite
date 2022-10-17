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
using System.Numerics;
using DotNext.Buffers;
using Ferrite.Crypto;
using Ferrite.TL;
using Ferrite.TL.slim;
using Ferrite.TL.slim.mtproto;

namespace Ferrite.Core.Methods;

public class ReqPQHandler : IQueryHandler
{
    private IRandomGenerator _randomGenerator;
    private IKeyProvider _keyPairProvider;
    public ReqPQHandler(IRandomGenerator generator, IKeyProvider provider)
    {
        _randomGenerator = generator;
        _keyPairProvider = provider;
    }

    public async Task<TLBytes?> Process(TLBytes q, TLExecutionContext ctx)
    {

        byte[] serverNonce;
        if (!ctx.SessionData.ContainsKey("nonce"))
        {
            ctx.SessionData.Add("nonce", new req_pq_multi(q.AsSpan()).nonce.ToArray());
            serverNonce = _randomGenerator.GetRandomBytes(16);
            ctx.SessionData.Add("server_nonce", serverNonce);
            await Task.Delay(100);
        }
        else if (!((byte[])ctx.SessionData["nonce"]).AsSpan().SequenceEqual(new req_pq_multi(q.AsSpan()).nonce))
        {
            ctx.SessionData["nonce"] = new req_pq_multi(q.AsSpan()).nonce.ToArray();
            serverNonce = _randomGenerator.GetRandomBytes(16);
            ctx.SessionData["server_nonce"] = serverNonce;
            return null;
        }

        serverNonce = (byte[])ctx.SessionData["server_nonce"];
        return ProcessInternal(serverNonce, new req_pq_multi(q.AsSpan()), ctx);
    }

    private TLBytes? ProcessInternal(byte[] serverNonce, req_pq_multi query, TLExecutionContext ctx)
    {
        byte[] nonce = (byte[])ctx.SessionData["nonce"];
        if (ctx.SessionData.ContainsKey("p"))
        {
            ctx.SessionData.Remove("p");
        }
        if (ctx.SessionData.ContainsKey("q"))
        {
            ctx.SessionData.Remove("q");
        }
        int a = _randomGenerator.GetRandomPrime();
        int b = _randomGenerator.GetRandomPrime();
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

        byte[] Pq = pq.ToByteArray(isBigEndian: true);

        var tmp = _keyPairProvider.GetRSAFingerprints();
        var fingerprints = new TL.slim.VectorOfLong();
        foreach (var f in tmp)
        {
            fingerprints.Append(f);
        }
        var resPq = new resPQ(nonce, 
            serverNonce, Pq, fingerprints);
        return resPq.TLBytes;
    }
}