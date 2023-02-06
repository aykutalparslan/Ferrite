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

/// <summary>
/// This is a wrapper around an IMemoryOwner that contains
/// the serialized form of a TLObject
/// </summary>
public readonly struct TLBytes: IDisposable
{
    private readonly IMemoryOwner<byte>? _memoryOwner;
    private readonly Memory<byte> _memory;
    private readonly int _offset;
    private readonly int _length;
    public TLBytes(IMemoryOwner<byte> memoryOwner, int offset, int length)
    {
        _memoryOwner = memoryOwner;
        _memory = _memoryOwner.Memory;
        _offset = offset;
        _length = length;
    }
    public TLBytes(Memory<byte> memory, int offset, int length)
    {
        _memory = memory;
        _offset = offset;
        _length = length;
    }
    public int Constructor => MemoryMarshal.Read<int>(_memory.Span[_offset..]);
    public Span<byte> AsSpan()
    {
        if (_offset == 0 && _length == _memory.Span.Length)
        {
            return _memory.Span;
        }
        if (_memory.Span.Length < _offset + _length)
        {
            return new Span<byte>();
        }
        var slice = _memory.Span.Slice(_offset, _length);
        return slice;
    }
    public Memory<byte> AsMemory()
    {
        if (_offset == 0 && _length == _memory.Span.Length)
        {
            return _memory;
        }
        if (_memory.Span.Length < _offset + _length)
        {
            return new Memory<byte>();
        }
        var slice = _memory.Slice(_offset, _length);
        return slice;
    }
    public void Dispose()
    {
        _memoryOwner?.Dispose();
    }
}