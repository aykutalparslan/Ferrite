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

public readonly unsafe struct ReqDhParams : ITLStruct<ReqDhParams>
{
    private readonly byte* _buff;
    private ReqDhParams(Span<byte> buffer)
    {
        _buff = (byte*)Unsafe.AsPointer(ref buffer[0]);
        Length = buffer.Length;
    }
    public ref readonly int Constructor => ref Unsafe.As<byte, int>(ref Unsafe.AsRef<byte>(_buff));
    public int Length { get; }
    public static ReqDhParams Read(Span<byte> data, int offset, out int bytesRead)
    {
        bytesRead = GetOffset(7, (byte*)Unsafe.AsPointer(ref data[offset..][0]), data.Length);
        var obj = new ReqDhParams(data.Slice(offset, bytesRead));
        return obj;
    }

    public static int ReadSize(Span<byte> data, int offset)
    {
        return GetOffset(7, (byte*)Unsafe.AsPointer(ref data[offset..][0]), data.Length);
    }

    public ReadOnlySpan<byte> Nonce => new ((byte*)_buff + GetOffset(1, _buff, Length), 16);
    public ReadOnlySpan<byte> ServerNonce => new ((byte*)_buff + GetOffset(2, _buff, Length), 16);
    public ReadOnlySpan<byte> P => ITLStruct<ReqDhParams>.GetTLBytes(_buff,GetOffset(3, _buff, Length), Length);
    public ReadOnlySpan<byte> Q => ITLStruct<ReqDhParams>.GetTLBytes(_buff,GetOffset(4, _buff, Length), Length);
    public ref readonly long PublicKeyFingerprint => ref Unsafe.As<byte, long>(
        ref Unsafe.Add(ref Unsafe.AsRef<byte>(_buff), GetOffset(5, _buff, Length)));
    public ReadOnlySpan<byte> EncryptedData => ITLStruct<ReqDhParams>.GetTLBytes(_buff,GetOffset(6, _buff, Length), Length);

    private static int GetOffset(int index, byte* buffer, int length)
    {
        int offset = 0;
        
        if(index >= 1) offset += 4;
        if(index >= 2) offset += 16;
        if(index >= 3) offset += 16;
        if(index >= 4) offset += ITLStruct<ReqDhParams>.GetTLBytesLength(buffer, offset, length);
        if(index >= 5) offset += ITLStruct<ReqDhParams>.GetTLBytesLength(buffer, offset, length);
        if(index >= 6) offset += 8;
        if(index >= 7) offset  += ITLStruct<ReqDhParams>.GetTLBytesLength(buffer, offset, length);
        
        return offset;
    }
}