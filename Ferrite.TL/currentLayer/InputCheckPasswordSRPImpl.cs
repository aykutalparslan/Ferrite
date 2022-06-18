/*
 *   Project Ferrite is an Implementation Telegram Server API
 *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Affero General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Affero General Public License for more details.
 *
 *   You should have received a copy of the GNU Affero General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Buffers;
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer;
public class InputCheckPasswordSRPImpl : InputCheckPasswordSRP
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public InputCheckPasswordSRPImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -763367294;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(_srpId, true);
            writer.WriteTLBytes(_A);
            writer.WriteTLBytes(_M1);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private long _srpId;
    public long SrpId
    {
        get => _srpId;
        set
        {
            serialized = false;
            _srpId = value;
        }
    }

    private byte[] _A;
    public byte[] A
    {
        get => _A;
        set
        {
            serialized = false;
            _A = value;
        }
    }

    private byte[] _M1;
    public byte[] M1
    {
        get => _M1;
        set
        {
            serialized = false;
            _M1 = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _srpId = buff.ReadInt64(true);
        _A = buff.ReadTLBytes().ToArray();
        _M1 = buff.ReadTLBytes().ToArray();
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}