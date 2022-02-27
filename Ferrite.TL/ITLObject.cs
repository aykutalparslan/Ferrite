/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using System;
using System.Buffers;
using DotNext.IO;

namespace Ferrite.TL;

public interface ITLObject
{
    public abstract int Constructor { get; }

    public void Parse(ref SequenceReader buff);
    public void WriteTo(Span<byte> buff);

    public ReadOnlySequence<byte> TLBytes
    {
        get;
    }

    public bool IsMethod
    {
        get;
    }

    public ITLObject Execute(TLExecutionContext ctx);
}

