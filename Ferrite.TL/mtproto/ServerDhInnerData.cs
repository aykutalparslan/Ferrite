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
public class ServerDhInnerData : ITLObject
{
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private ITLObjectFactory factory;
    private bool serialized = false;
    public ServerDhInnerData(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -1249309254;
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
            writer.WriteInt32(g, true);
            writer.WriteTLBytes(dhPrime);
            writer.WriteTLBytes(gA);
            writer.WriteInt32(serverTime, true);
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

    private int g;
    public int G
    {
        get => g;
        set
        {
            serialized = false;
            g = value;
        }
    }

    private byte[] dhPrime;
    public byte[] DhPrime
    {
        get => dhPrime;
        set
        {
            serialized = false;
            dhPrime = value;
        }
    }

    private byte[] gA;
    public byte[] GA
    {
        get => gA;
        set
        {
            serialized = false;
            gA = value;
        }
    }

    private int serverTime;
    public int ServerTime
    {
        get => serverTime;
        set
        {
            serialized = false;
            serverTime = value;
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
        g = buff.ReadInt32(true);
        dhPrime = buff.ReadTLBytes().ToArray();
        gA = buff.ReadTLBytes().ToArray();
        serverTime = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}