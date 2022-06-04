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

using System.Runtime.CompilerServices;

namespace Ferrite.TL.slim.mtproto;

public readonly unsafe struct ReqPqMulti : ITLStruct<ReqPqMulti>
{
    private readonly byte* _buff;
    private ReqPqMulti(Span<byte> buffer)
    {
        _buff = (byte*)Unsafe.AsPointer(ref buffer[0]);
        Length = buffer.Length;
    }
    public ref readonly int Constructor => ref Unsafe.As<byte, int>(ref Unsafe.AsRef<byte>(_buff));
    public int Length { get; }
    public ReadOnlySpan<byte> ToReadOnlySpan() => new ReadOnlySpan<byte>(_buff, Length);
    public static ReqPqMulti Read(Span<byte> data, in int offset, out int bytesRead)
    {
        throw new NotImplementedException();
    }

    public static ReqPqMulti Read(byte* buffer, in int length, in int offset, out int bytesRead)
    {
        throw new NotImplementedException();
    }

    public static int ReadSize(Span<byte> data, in int offset)
    {
        throw new NotImplementedException();
    }

    public static int ReadSize(byte* buffer, in int length, in int offset)
    {
        throw new NotImplementedException();
    }

    public static ReqPqMulti Init(Span<byte> data, in int offset, in int length)
    {
        throw new NotImplementedException();
    }

    public static ReqPqMulti Read(Span<byte> data, int offset, out int bytesRead)
    {
        bytesRead = GetOffset(7, (byte*)Unsafe.AsPointer(ref data[offset..][0]), data.Length);
        var obj = new ReqPqMulti(data.Slice(offset, bytesRead));
        return obj;
    }
    public int GetRequiredBufferSize()
    {
        return 12;
    }
    public static int ReadSize(Span<byte> data, int offset)
    {
        return GetOffset(7, (byte*)Unsafe.AsPointer(ref data[offset..][0]), data.Length);
    }

    public ReadOnlySpan<byte> Nonce => new ((byte*)_buff + GetOffset(1, _buff, Length), 16);
    
    private static int GetOffset(int index, byte* buffer, int length)
    {
        int offset = 0;
        
        if(index >= 1) offset += 4;
        
        return offset;
    }
}