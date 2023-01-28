// 
// Project Ferrite is an Implementation of the Telegram Server API
// Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 

using System.Buffers;
using System.Runtime.InteropServices;

namespace Ferrite.TL.slim;

public readonly struct TLBool : IDisposable
{
    private readonly TLBytes _tlBytes;
    private readonly int _constructor;

    public TLBool(IMemoryOwner<byte> memoryOwner, int offset, int length)
    {
        _constructor = MemoryMarshal.Read<int>(memoryOwner.Memory.Span[offset..]);
        ThrowIfInvalid();
        _tlBytes = new TLBytes(memoryOwner, offset, length);
    }

    public TLBool(Memory<byte> memory, int offset, int length)
    {
        _constructor = MemoryMarshal.Read<int>(memory.Span[offset..]);
        ThrowIfInvalid();
        _tlBytes = new TLBytes(memory, offset, length);
    }

    private TLBool(TLBytes bytes)
    {
        _constructor = bytes.Constructor;
        ThrowIfInvalid();
        _tlBytes = bytes;
    }
    
    private void ThrowIfInvalid()
    {
        if (_constructor != unchecked((int)0x997275b5) &&
            _constructor != unchecked((int)0xbc799737))
        {
            throw new InvalidCastException();
        }
    }

    public int Constructor => _tlBytes.Constructor;
    
    public TLBytes TLBytes => _tlBytes;

    public static implicit operator TLBool(TLBytes b) => new (b);
    
    public static implicit operator TLBytes(TLBool b) => b._tlBytes;
    
    public BoolTrue AsBoolTrue() => (BoolTrue)_tlBytes.AsSpan();
    
    public BoolFalse AsBoolFalse() => (BoolFalse)_tlBytes.AsSpan();
    
    public BoolType Type => _constructor switch
    {
        unchecked((int)0x997275b5) => BoolType.True,
        _ => BoolType.False
    };

    public Span<byte> AsSpan() => _tlBytes.AsSpan();

    public enum BoolType
    {
        True,
        False
    }

    public void Dispose()
    {
        _tlBytes.Dispose();
    }
}