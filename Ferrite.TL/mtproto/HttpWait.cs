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
public class HttpWait : ITLObject
{
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private ITLObjectFactory factory;
    private bool serialized = false;
    public HttpWait(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -1835453025;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt32(maxDelay, true);
            writer.WriteInt32(waitAfter, true);
            writer.WriteInt32(maxWait, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private int maxDelay;
    public int MaxDelay
    {
        get => maxDelay;
        set
        {
            serialized = false;
            maxDelay = value;
        }
    }

    private int waitAfter;
    public int WaitAfter
    {
        get => waitAfter;
        set
        {
            serialized = false;
            waitAfter = value;
        }
    }

    private int maxWait;
    public int MaxWait
    {
        get => maxWait;
        set
        {
            serialized = false;
            maxWait = value;
        }
    }

    public bool IsMethod => true;
    public ITLObject Execute(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        maxDelay = buff.ReadInt32(true);
        waitAfter = buff.ReadInt32(true);
        maxWait = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}