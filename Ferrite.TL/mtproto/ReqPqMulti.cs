/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using System;
using System.Buffers;
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Crypto;
using System.Numerics;

namespace Ferrite.TL.mtproto;
public class ReqPqMulti : ITLObject
{
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    ITLObjectFactory factory;
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

    public bool IsMethod => true;
    public ITLObject Execute(TLExecutionContext ctx)
    {
        ctx.SessionBag.Add("nonce", nonce);
        Respq respq = factory.Resolve<Respq>();
        respq.Nonce = nonce;
        respq.ServerNonce = (Int128)randomGenerator.GetRandomBytes(16);
        ctx.SessionBag.Add("server_nonce", respq.ServerNonce);
        int a = randomGenerator.GetRandomPrime();
        int b = randomGenerator.GetRandomPrime();
        BigInteger pq = new BigInteger(a) * b;
        if (a < b)
        {
            ctx.SessionBag.Add("p", a);
            ctx.SessionBag.Add("q", b);
        }
        else
        {
            ctx.SessionBag.Add("p", b);
            ctx.SessionBag.Add("q", a);
        }

        respq.Pq = pq.ToByteArray(isBigEndian:true);
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