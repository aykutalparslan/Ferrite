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
public class MsgDetailedInfo : ITLObject
{
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private ITLObjectFactory factory;
    private bool serialized = false;
    public MsgDetailedInfo(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => 661470918;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(msgId, true);
            writer.WriteInt64(answerMsgId, true);
            writer.WriteInt32(bytes, true);
            writer.WriteInt32(status, true);
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

    private long answerMsgId;
    public long AnswerMsgId
    {
        get => answerMsgId;
        set
        {
            serialized = false;
            answerMsgId = value;
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

    private int status;
    public int Status
    {
        get => status;
        set
        {
            serialized = false;
            status = value;
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
        answerMsgId = buff.ReadInt64(true);
        bytes = buff.ReadInt32(true);
        status = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}