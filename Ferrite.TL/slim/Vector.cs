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

namespace Ferrite.TL.slim;

public unsafe struct Vector<T> : ITLObjectReader, ITLSerializable, IDisposable where T : ITLObjectReader, ITLSerializable
{
    private readonly byte* _buff;
    private readonly IMemoryOwner<byte>? _memoryOwner;
    private Vector(Span<byte> buffer, IMemoryOwner<byte> memoryOwner)
    {
        _buff = (byte*)Unsafe.AsPointer(ref buffer[0]);
        Length = buffer.Length;
        _position = 8;
        _memoryOwner = memoryOwner;
    }
    private Vector(byte* buffer, in int length, IMemoryOwner<byte> memoryOwner)
    {
        _buff = buffer;
        Length = length;
        _position = 8;
        _memoryOwner = memoryOwner;
    }
    public ref readonly int Constructor => ref Unsafe.AsRef<int>((int*)_buff);
    private void SetConstructor(int constructor)
    {
        var p = (int*)_buff;
        *p = constructor;
    }
    public ReadOnlySpan<byte> ToReadOnlySpan() => new ReadOnlySpan<byte>(_buff, Length);
    public readonly ref int Count => ref Unsafe.AsRef<int>((int*)(_buff + 4));
    private void SetCount(int count)
    {
        var p = (int*)(_buff + 4);
        *p = count;
    }
    public int Length { get; }
    private int _position;
    public static ITLSerializable? Read(Span<byte> data, in int offset, out int bytesRead)
    {
        var ptr = (byte*)Unsafe.AsPointer(ref data.Slice(offset)[0]);
        ptr += 4;
        int count = *ptr & 0xff | (*++ptr & 0xff) << 8 | (*++ptr & 0xff) << 16| (*++ptr & 0xff) << 24;
        int len = 8;
        for (int i = 0; i < count; i++)
        {
            len += T.ReadSize(data, len);
        }
        bytesRead = len;
        var obj = new Vector<T>(data.Slice(offset, bytesRead), null);
        return obj;
    }

    public static ITLSerializable? Read(byte* buffer, in int length, in int offset, out int bytesRead)
    {
        var ptr = buffer+offset;
        ptr += 4;
        int count = *ptr & 0xff | (*++ptr & 0xff) << 8 | (*++ptr & 0xff) << 16| (*++ptr & 0xff) << 24;
        int len = 8;
        for (int i = 0; i < count; i++)
        {
            len += T.ReadSize(buffer, length, offset + len);
        }
        bytesRead = len;
        var obj = new Vector<T>(buffer + offset, bytesRead, null);
        return obj;
    }

    public static int ReadSize(Span<byte> data, in int offset)
    {
        var ptr = (byte*)Unsafe.AsPointer(ref data.Slice(offset)[0]);
        ptr += 4;
        int count = *ptr & 0xff | (*++ptr & 0xff) << 8 | (*++ptr & 0xff) << 16| (*++ptr & 0xff) << 24;
        int len = 8;
        for (int i = 0; i < count; i++)
        {
            len += T.ReadSize(data, len);
        }
        return len;
    }

    public static int ReadSize(byte* buffer, in int length, in int offset)
    {
        var ptr = buffer + offset;
        ptr += 4;
        int count = *ptr & 0xff | (*++ptr & 0xff) << 8 | (*++ptr & 0xff) << 16| (*++ptr & 0xff) << 24;
        int len = 8;
        for (int i = 0; i < count; i++)
        {
            len += T.ReadSize(buffer, length, len);
        }
        return len;
    }

    public static Vector<T> Create(ICollection<T> items, MemoryPool<byte>? pool = null)
    {
        var length = 8;
        foreach (var item in items)
        {
            length += item.Length;
        }
        var memory = pool != null ? pool.Rent(length) : MemoryPool<byte>.Shared.Rent(length);
        var obj = new Vector<T>(memory.Memory.Span[..length], memory);
        obj.SetConstructor(unchecked((int)0x1cb5c415));
        obj.SetCount(items.Count);
        int offset = 8;
        foreach (var item in items)
        {
            obj.Write(item.ToReadOnlySpan(), offset);
            offset += item.Length;
        }
        return obj;
    }

    private void Write(ReadOnlySpan<byte> value, int offset)
    {
        fixed (byte* p = value)
        {
            Buffer.MemoryCopy(p, _buff + offset,
                Length - offset,value.Length);
        }
    }
    public T Read()
    {
        if (_position == Length)
        {
            throw new EndOfStreamException();
        }
        var obj = T.Read(new Span<byte>(_buff + _position, 
            Length - _position), 0, out var bytesRead);
         _position += bytesRead;
        return (T)obj;
    }
    public void Reset()
    {
        _position = 8;
    }

    public void Dispose()
    {
        _memoryOwner?.Dispose();
    }
}