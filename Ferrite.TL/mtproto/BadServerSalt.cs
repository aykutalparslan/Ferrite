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
public class BadServerSalt : ITLObject
{
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private ITLObjectFactory factory;
    private bool serialized = false;
    public BadServerSalt(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -307542917;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(badMsgId, true);
            writer.WriteInt32(badMsgSeqno, true);
            writer.WriteInt32(errorCode, true);
            writer.WriteInt64(newServerSalt, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private long badMsgId;
    public long BadMsgId
    {
        get => badMsgId;
        set
        {
            serialized = false;
            badMsgId = value;
        }
    }

    private int badMsgSeqno;
    public int BadMsgSeqno
    {
        get => badMsgSeqno;
        set
        {
            serialized = false;
            badMsgSeqno = value;
        }
    }

    private int errorCode;
    public int ErrorCode
    {
        get => errorCode;
        set
        {
            serialized = false;
            errorCode = value;
        }
    }

    private long newServerSalt;
    public long NewServerSalt
    {
        get => newServerSalt;
        set
        {
            serialized = false;
            newServerSalt = value;
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
        badMsgId = buff.ReadInt64(true);
        badMsgSeqno = buff.ReadInt32(true);
        errorCode = buff.ReadInt32(true);
        newServerSalt = buff.ReadInt64(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}