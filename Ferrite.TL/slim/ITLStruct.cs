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

using DotNext;

namespace Ferrite.TL.slim;

public interface ITLStruct<out T>
{
    public ref readonly int Constructor { get; }
    public int Length { get; }
    public static abstract T Read(Span<byte> data, int offset, out int bytesRead);
    public static abstract int ReadSize(Span<byte> data, int offset);
    public static unsafe int GetTLBytesLength(byte* buffer, in int offset, in int length)
    {
        if (offset >= length)
        {
            return 0;
        }

        var b = (byte*)buffer + offset;
        int len = *b;
        var rem = (4 - ((len + 1) % 4)) % 4;
        if (len != 254) return len + rem + 1;
        len = *++b |
              ((*++b & 0xff) << 8) |
              ((*++b & 0xff) << 16);
        rem = (4 - len % 4) % 4;
        return len + rem;
    }
    public static unsafe ReadOnlySpan<byte> GetTLBytes(byte* buffer, in int offset, in int length)
    {
        if (offset >= length) return new ReadOnlySpan<byte>();
        var b = (byte*)buffer + offset;
        int len = *b;
        if (offset + len >= length) return new ReadOnlySpan<byte>();
        if (len != 254) return new ReadOnlySpan<byte>(b + 1, len);
        len = *++b |
              ((*++b & 0xff) << 8) |
              ((*++b & 0xff) << 16);
        if (offset + len >= length) return new ReadOnlySpan<byte>();
        return new ReadOnlySpan<byte>(b + 4, len);
    }
}