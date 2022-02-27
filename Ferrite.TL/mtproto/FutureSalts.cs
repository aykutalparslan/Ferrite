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
public class FutureSalts : ITLObject
{
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private ITLObjectFactory factory;
    private bool serialized = false;
    public FutureSalts(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -1370486635;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(reqMsgId, true);
            writer.WriteInt32(now, true);
            writer.Write(salts.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private long reqMsgId;
    public long ReqMsgId
    {
        get => reqMsgId;
        set
        {
            serialized = false;
            reqMsgId = value;
        }
    }

    private int now;
    public int Now
    {
        get => now;
        set
        {
            serialized = false;
            now = value;
        }
    }

    private VectorBare<FutureSalt> salts;
    public VectorBare<FutureSalt> Salts
    {
        get => salts;
        set
        {
            serialized = false;
            salts = value;
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
        reqMsgId = buff.ReadInt64(true);
        now = buff.ReadInt32(true);
        buff.Skip(4); salts  =  factory . Read < VectorBare < FutureSalt > > ( ref  buff ) ; 
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}