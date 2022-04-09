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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DotNext.IO;
using xxHash;

namespace Ferrite.TL;

public struct Int128 : ITLObject, IEquatable<Int128>
{
    private byte[] _value;
    public Int128()
    {
        _value = new byte[16];
    }
    public Int128(byte[] val)
    {
        if (val.Length == 16)
        {
            _value = val;
        }
        else
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    public static implicit operator byte[](Int128 i)
    {
        return i._value;
    }
    public static explicit operator Int128(byte[] b) => new Int128(b);

    public readonly int Constructor => unchecked((int)0x84ccf7b7);

    public ReadOnlySequence<byte> TLBytes => new ReadOnlySequence<byte>((byte[])this);

    public Span<byte> AsSpan()
    {
        return _value;
    }

    public void Parse(ref SequenceReader buff)
    {
        buff.Read(_value);
    }

    public void WriteTo(Span<byte> buff)
    {
        _value.CopyTo(buff);
    }
   
    public static bool operator ==(Int128 left, Int128 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Int128 left, Int128 right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return (int)this.AsSpan().GetXxHash32();
    }

    public bool Equals(Int128 other)
    {
        return this.AsSpan().SequenceEqual(other.AsSpan());
    }

    public override bool Equals(object? obj)
    {
        return obj is Int128 && Equals((Int128)obj);
    }
}


