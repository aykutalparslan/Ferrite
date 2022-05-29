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
using DotNext;

namespace Ferrite.TL.slim;

public readonly unsafe struct TLString : ITLStruct<TLString>
{
    private readonly byte* _buff;

    private TLString(ReadOnlySpan<byte> buffer)
    {
        fixed (byte* p = &buffer[0])
        {
            _buff = p;
        }

        Length = buffer.Length;
    }

    public int Length { get; }
    public ReadOnlySpan<byte> ToReadOnlySpan() => new(_buff, Length);
    public ReadOnlySpan<byte> GetValueBytes() => ITLStruct<TLString>.GetTLBytes(_buff, 0, Length);

    public static TLString Read(Span<byte> data, in int offset, out int bytesRead)
    {
        var buffer = (byte*)Unsafe.AsPointer(ref data[offset..][0]);
        bytesRead = ITLStruct<TLString>.GetTLBytesLength(buffer, offset, data.Length);
        return new TLString(data.Slice(offset, bytesRead));
    }

    public static int ReadSize(Span<byte> data, in int offset)
    {
        var buffer = (byte*)Unsafe.AsPointer(ref data[offset..][0]);
        return ITLStruct<TLString>.GetTLBytesLength(buffer, 0, data.Length);
    }

    public static TLString Create(ReadOnlySpan<byte> value)
    {
        return new TLString(value);
    }
}