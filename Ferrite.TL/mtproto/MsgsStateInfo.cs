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
public class MsgsStateInfo : ITLObject
{
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private ITLObjectFactory factory;
    private bool serialized = false;
    public MsgsStateInfo(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => 81704317;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(reqMsgId, true);
            writer.WriteTLBytes(info);
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

    private byte[] info;
    public byte[] Info
    {
        get => info;
        set
        {
            serialized = false;
            info = value;
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
        info = buff.ReadTLBytes().ToArray();
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}