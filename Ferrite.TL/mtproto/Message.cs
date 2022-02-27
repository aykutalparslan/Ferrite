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
public class Message : ITLObject
{
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private ITLObjectFactory factory;
    private bool serialized = false;
    public Message(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => 1538843921;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(msgId, true);
            writer.WriteInt32(seqno, true);
            writer.WriteInt32(bytes, true);
            writer.Write(body.TLBytes, false);
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

    private int seqno;
    public int Seqno
    {
        get => seqno;
        set
        {
            serialized = false;
            seqno = value;
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

    private ITLObject body;
    public ITLObject Body
    {
        get => body;
        set
        {
            serialized = false;
            body = value;
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
        seqno = buff.ReadInt32(true);
        bytes = buff.ReadInt32(true);
        body = factory.Read(buff.ReadInt32(true), ref buff);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}