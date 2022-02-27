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
public class RpcError : ITLObject
{
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private ITLObjectFactory factory;
    private bool serialized = false;
    public RpcError(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => 558156313;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt32(errorCode, true);
            writer.WriteTLString(errorMessage);
            serialized = true;
            return writer.ToReadOnlySequence();
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

    private string errorMessage;
    public string ErrorMessage
    {
        get => errorMessage;
        set
        {
            serialized = false;
            errorMessage = value;
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
        errorCode = buff.ReadInt32(true);
        errorMessage = buff.ReadTLString();
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}