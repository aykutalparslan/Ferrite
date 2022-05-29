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
    public int Length { get; }
    public ReadOnlySpan<byte> ToReadOnlySpan();
    public static abstract T Read(Span<byte> data, in int offset, out int bytesRead);
    public static abstract int ReadSize(Span<byte> data, in int offset);
    public static int CalculateTLBytesLength(in int length)
    {
        if (length < 254)
        {
            return length + 1 + (4 - (length+ 1) % 4) % 4;
        }
        return length + 4 + (4 - length % 4) % 4;
    }
    public static unsafe int WriteLenBytes(byte* buffer, ReadOnlySpan<byte> value, int offset, int bufferLength)
    {
        var lenBytes = 0;
        if (value.Length < 254)
        {
            lenBytes++;
            *(buffer + offset) = (byte)value.Length;
        }
        else
        {
            *(buffer + offset) = (byte)254;
            *(buffer + offset + 1) = (byte)(value.Length & 0xff);
            *(buffer + offset + 2) = (byte)((value.Length >> 8) & 0xff);
            *(buffer + offset + 3) = (byte)((value.Length >> 16) & 0xff);
            lenBytes += 4;
        }

        return lenBytes;
    }
    /// <summary>
    /// Decodes the size of a TL serialized bare string including the padding bytes.
    /// </summary>
    /// <param name="buffer">Pointer to the source buffer.</param>
    /// <param name="offset">Offset in the source buffer.</param>
    /// <param name="length">Length of the source buffer.</param>
    /// <returns></returns>
    public static unsafe int GetTLBytesLength(byte* buffer, in int offset, in int length)
    {
        if (offset > length)
        {
            return 0;
        }

        var b = buffer + offset;
        int len = *b;
        var rem = (4 - ((len + 1) % 4)) % 4;
        if (len != 254) return len + rem + 1;
        len = (*(b+1) & 0xff) |
              ((*(b+2) & 0xff) << 8) |
              ((*(b+3) & 0xff) << 16);
        rem = (4 - len % 4) % 4;
        return len + 4 + rem;
    }
    /// <summary>
    /// Decodes the value of a TL serialized bare string.
    /// </summary>
    /// <param name="buffer">Pointer to the source buffer.</param>
    /// <param name="offset">Offset in the source buffer.</param>
    /// <param name="length">Length of the source buffer.</param>
    /// <returns></returns>
    public static unsafe Span<byte> GetTLBytes(byte* buffer, in int offset, in int length)
    {
        if (offset >= length) return new Span<byte>();
        var b = buffer + offset;
        int len = *b;
        if (offset + len > length) return new Span<byte>();
        if (len != 254) return new Span<byte>(b + 1, len);
        len = (*(b+1) & 0xff) |
              ((*(b+2) & 0xff) << 8) |
              ((*(b+3) & 0xff) << 16);
        if (offset + len > length) return new Span<byte>();
        return new Span<byte>(b + 4, len);
    }
}