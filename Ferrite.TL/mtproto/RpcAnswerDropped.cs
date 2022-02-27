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
public class RpcAnswerDropped : ITLObject
{
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private ITLObjectFactory factory;
    private bool serialized = false;
    public RpcAnswerDropped(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -1539647305;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(msgId, true);
            writer.WriteInt32(seqNo, true);
            writer.WriteInt32(bytes, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private long msgId;
    public long MsgId
    {
        get => msgId;
        set
        {
            serialized = false;
            msgId = value;
        }
    }

    private int seqNo;
    public int SeqNo
    {
        get => seqNo;
        set
        {
            serialized = false;
            seqNo = value;
        }
    }

    private int bytes;
    public int Bytes
    {
        get => bytes;
        set
        {
            serialized = false;
            bytes = value;
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
        msgId = buff.ReadInt64(true);
        seqNo = buff.ReadInt32(true);
        bytes = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}