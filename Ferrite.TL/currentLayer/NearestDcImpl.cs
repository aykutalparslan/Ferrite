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
public class NearestDcImpl : NearestDc
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public NearestDcImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1910892683;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLString(_country);
            writer.WriteInt32(_thisDc, true);
            writer.WriteInt32(_nearestDc, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private string _country;
    public string Country
    {
        get => _country;
        set
        {
            serialized = false;
            _country = value;
        }
    }

    private int _thisDc;
    public int ThisDc
    {
        get => _thisDc;
        set
        {
            serialized = false;
            _thisDc = value;
        }
    }

    private int _nearestDc;
    public int NearestDc
    {
        get => _nearestDc;
        set
        {
            serialized = false;
            _nearestDc = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _country = buff.ReadTLString();
        _thisDc = buff.ReadInt32(true);
        _nearestDc = buff.ReadInt32(true);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}