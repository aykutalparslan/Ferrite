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
using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ferrite.TL.slim;

public ref struct VectorBare
{
    private Span<byte> _buff;
    private int _position;
    private int _offset;
    public VectorBare()
    {
        _buff = new byte[512];
        SetConstructor(unchecked((int)0x1cb5c415));
        SetCount(0);
        _position = 4;
        _offset = 4;
    }
    public VectorBare(Span<byte> buffer)
    {
        _buff = buffer;
        _position = 4;
        _offset = 4;
    }
    public readonly int Constructor => MemoryMarshal.Read<int>(_buff);
    private void SetConstructor(int constructor)
    {
        MemoryMarshal.Write(_buff[..4], ref constructor);
    }
    public ReadOnlySpan<byte> ToReadOnlySpan() => _buff;
    public readonly int Count => MemoryMarshal.Read<int>(_buff.Slice(4, 4));
    public readonly int Length => _offset;
    private void SetCount(int count)
    {
        MemoryMarshal.Write(_buff.Slice(4, 4), ref count);
    }
    
    public static Span<byte> Read(Span<byte> data, int offset)
    {
        int count = MemoryMarshal.Read<int>(data.Slice(offset, 4));
        int len = 4;
        for (int i = 0; i < count; i++)
        {
            var sizeReader = ObjectReader.GetObjectSizeReader(
                MemoryMarshal.Read<int>(data.Slice(offset + len, 4)));
            len += sizeReader.Invoke(data, len);
        }
        return data.Slice(offset, len);
    }

    public static int ReadSize(Span<byte> data, int offset)
    {
        int count = MemoryMarshal.Read<int>(data.Slice(offset, 4));
        int len = 4;
        for (int i = 0; i < count; i++)
        {
            var sizeReader = ObjectReader.GetObjectSizeReader(
                MemoryMarshal.Read<int>(data.Slice(offset + len, 4)));
            len += sizeReader.Invoke(data, len);
        }
        return len;
    }
    
    public void Append(ReadOnlySpan<byte> value)
    {
        if (value.Length + _offset > _buff.Length)
        {
            var tmp = new byte[_buff.Length * 2];
            _buff.CopyTo(tmp);
            _buff = tmp;
        }
        value.CopyTo(_buff[_offset..]);
        _offset += value.Length;
    }
    public ReadOnlySpan<byte> Read()
    {
        if (_position == _offset)
        {
            throw new EndOfStreamException();
        }
        ObjectReaderDelegate? reader = ObjectReader.GetObjectReader(
            MemoryMarshal.Read<int>(_buff.Slice(_position, 4)));
        if (reader == null)
        {
            throw new NotSupportedException();
        }

        var result = reader.Invoke(_buff, _position);
        _position += result.Length;
        return result;
    }
    public void Reset()
    {
        _position = 4;
    }
}