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
public class ClientDhInnerData : ITLObject
{
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private ITLObjectFactory factory;
    private bool serialized = false;
    public ClientDhInnerData(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => 1715713620;
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
            writer.WriteInt64(retryId, true);
            writer.WriteTLBytes(gB);
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

    private long retryId;
    public long RetryId
    {
        get => retryId;
        set
        {
            serialized = false;
            retryId = value;
        }
    }

    private byte[] gB;
    public byte[] GB
    {
        get => gB;
        set
        {
            serialized = false;
            gB = value;
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
        nonce = factory.Read<Int128>(ref buff);
        serverNonce = factory.Read<Int128>(ref buff);
        retryId = buff.ReadInt64(true);
        gB = buff.ReadTLBytes().ToArray();
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}