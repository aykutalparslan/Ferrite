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
using Ferrite.Utils;

namespace Ferrite.TL.mtproto;
public class PQInnerDataDc : ITLObject
{
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private ITLObjectFactory factory;
    private bool serialized = false;
    public int Constructor => -1443537003;
    public PQInnerDataDc(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLBytes(pq);
            writer.WriteTLBytes(p);
            writer.WriteTLBytes(q);
            writer.Write(nonce.TLBytes, false);
            writer.Write(serverNonce.TLBytes, false);
            writer.Write(newNonce.TLBytes, false);
            writer.WriteInt32(dc, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private byte[] pq;
    public byte[] Pq
    {
        get => pq;
        set
        {
            serialized = false;
            pq = value;
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

    private Int256 newNonce;
    public Int256 NewNonce
    {
        get => newNonce;
        set
        {
            serialized = false;
            newNonce = value;
        }
    }

    private int dc;
    public int Dc
    {
        get => dc;
        set
        {
            serialized = false;
            dc = value;
        }
    }

    public bool IsMethod => false;
    public ITLObject Execute(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        pq = buff.ReadTLBytes().ToArray();
        p = buff.ReadTLBytes().ToArray();
        q = buff.ReadTLBytes().ToArray();
        nonce = factory.Read<Int128>(ref buff);
        serverNonce = factory.Read<Int128>(ref buff);
        newNonce = factory.Read<Int256>(ref buff);
        dc = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}