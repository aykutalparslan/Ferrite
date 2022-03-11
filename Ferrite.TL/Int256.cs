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
using DotNext.IO;
using xxHash;

namespace Ferrite.TL;

public class Int256 : ITLObject, IEquatable<Int256>
{
    public Int256()
    {
        value = new byte[32];
    }
    public Int256(byte[] val)
    {
        if (val.Length == 32)
        {
            value = val;
        }
        else if (val.Length <= 32)
        {
            for (int i = 0; i < val.Length; i++)
            {
                value[i] = val[i];
            }
        }
        else
        {
            for (int i = 0; i < 32; i++)
            {
                value[i] = val[i];
            }
        }
    }
    private byte[] value = new byte[32];

    public int Constructor => unchecked((int)0x85d2fe54);

    public static implicit operator byte[](Int256 i) =>i.value;
    public static explicit operator Int256(byte[] i) => new Int256(i);

    public ReadOnlySequence<byte> TLBytes => new ReadOnlySequence<byte>(value);

    public bool IsMethod => false;

    public ITLObject Execute(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        buff.Read(value);
    }

    public void WriteTo(Span<byte> buff)
    {
        value.CopyTo(buff);
    }

    public bool Equals([NotNullWhen(true)] Int256? other)
    {
        return value.SequenceEqual(other.value);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return Equals(obj);
    }

    public static bool operator ==(Int256 left, Int256 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Int256 left, Int256 right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return (int)value.GetXxHash32();
    }
}


