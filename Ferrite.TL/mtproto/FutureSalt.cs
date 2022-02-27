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
public class FutureSalt : ITLObject
{
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private ITLObjectFactory factory;
    private bool serialized = false;
    public FutureSalt(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => 155834844;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt32(validSince, true);
            writer.WriteInt32(validUntil, true);
            writer.WriteInt64(salt, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private int validSince;
    public int ValidSince
    {
        get => validSince;
        set
        {
            serialized = false;
            validSince = value;
        }
    }

    private int validUntil;
    public int ValidUntil
    {
        get => validUntil;
        set
        {
            serialized = false;
            validUntil = value;
        }
    }

    private long salt;
    public long Salt
    {
        get => salt;
        set
        {
            serialized = false;
            salt = value;
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
        validSince = buff.ReadInt32(true);
        validUntil = buff.ReadInt32(true);
        salt = buff.ReadInt64(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}