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
using System.Runtime.CompilerServices;
using Ferrite.Utils;

namespace Ferrite.TL.slim;

public readonly unsafe struct TLString : ITLObjectReader, ITLSerializable
{
    private readonly byte* _buff;
    private readonly IMemoryOwner<byte>? _memoryOwner;
    private TLString(ReadOnlySpan<byte> buffer, IMemoryOwner<byte> memoryOwner)
    {
        fixed (byte* p = &buffer[0])
        {
            _buff = p;
        }

        Length = buffer.Length;
        _memoryOwner = memoryOwner;
    }
    private TLString(byte* buffer, in int length, IMemoryOwner<byte> memoryOwner)
    {
        _buff = buffer;
        Length = length;
        _memoryOwner = memoryOwner;
    }

    public int Length { get; }
    public ReadOnlySpan<byte> ToReadOnlySpan() => new(_buff, Length);
    public ReadOnlySpan<byte> GetValueBytes() => BufferUtils.GetTLBytes(_buff, 0, Length);

    public static ITLSerializable? Read(Span<byte> data, in int offset, out int bytesRead)
    {
        var buffer = (byte*)Unsafe.AsPointer(ref data[offset..][0]);
        bytesRead = BufferUtils.GetTLBytesLength(buffer, offset, data.Length);
        return new TLString(data.Slice(offset, bytesRead), null);
    }

    public static ITLSerializable? Read(byte* buffer, in int length, in int offset, out int bytesRead)
    {
        bytesRead = BufferUtils.GetTLBytesLength(buffer, offset, length);
        return new TLString(buffer + offset, bytesRead, null);
    }

    public static int ReadSize(Span<byte> data, in int offset)
    {
        var buffer = (byte*)Unsafe.AsPointer(ref data[offset..][0]);
        return BufferUtils.GetTLBytesLength(buffer, 0, data.Length);
    }

    public static int ReadSize(byte* buffer, in int length, in int offset)
    {
        return BufferUtils.GetTLBytesLength(buffer, offset, length);
    }

    public static TLString Create(ReadOnlySpan<byte> value)
    {
        return new TLString(value, null);
    }
    public static TLString Create(ReadOnlySpan<byte> value, IMemoryOwner<byte> memoryOwner)
    {
        return new TLString(value, memoryOwner);
    }

    public ref readonly int Constructor => throw new NotImplementedException();

    public void Dispose()
    {
        _memoryOwner?.Dispose();
    }
}