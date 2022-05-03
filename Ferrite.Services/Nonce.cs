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
using xxHash;

namespace Ferrite.Services;

public struct Nonce : IEquatable<Nonce>
{
    private byte[] _value;
    public Nonce()
    {
        _value = new byte[16];
    }
    public Nonce(byte[] val)
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

    public static implicit operator byte[](Nonce i)
    {
        return i._value;
    }
    public static explicit operator Nonce(byte[] b) => new Nonce(b);

    public Span<byte> AsSpan()
    {
        return _value;
    }
   
    public static bool operator ==(Nonce left, Nonce right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Nonce left, Nonce right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return (int)AsSpan().GetXxHash32();
    }

    public bool Equals(Nonce other)
    {
        return _value.SequenceEqual(other._value);
    }

    public override bool Equals(object? obj)
    {
        return obj is Nonce && Equals((Nonce)obj);
    }
}


