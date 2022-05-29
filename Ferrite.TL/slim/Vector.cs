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

using System.Collections;
using System.Runtime.CompilerServices;

namespace Ferrite.TL.slim;

public unsafe struct Vector<T> : ITLStruct<Vector<T>> where T: ITLStruct<T>
{
    private readonly byte* _buff;
    public Vector(Span<byte> buffer)
    {
        _buff = (byte*)Unsafe.AsPointer(ref buffer[0]);
        Count = *(_buff+4) & 0xff | (*(_buff + 5) & 0xff) << 8 | (*(_buff + 6) & 0xff) << 16 |
                    (*(_buff + 7) & 0xff) << 24;
        Length = buffer.Length;
        _position = 8;
    }
    public ref readonly int Constructor => ref Unsafe.As<byte, int>(ref Unsafe.AsRef<byte>(_buff));
    public int Count { get; }
    public int Length { get; }
    private int _position;
    public static Vector<T> Read(Span<byte> data, int offset, out int bytesRead)
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
        var obj = new Vector<T>(data.Slice(offset, bytesRead));
        return obj;
    }

    public static int ReadSize(Span<byte> data, int offset)
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

    public T Read()
    {
        if (_position == Length)
        {
            throw new EndOfStreamException();
        }
        var obj = T.Read(new Span<byte>(_buff + _position, 
            Length - _position), 0, out var bytesRead);
         _position += bytesRead;
        return obj;
    }
    public void Reset()
    {
        _position = 8;
    }
}