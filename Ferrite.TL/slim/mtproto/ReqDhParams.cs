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

namespace Ferrite.TL.slim.mtproto;

public readonly unsafe struct ReqDhParams : ITLStruct<ReqDhParams>, ITLBoxed
{
    private readonly byte* _buff;
    private ReqDhParams(Span<byte> buffer)
    {
        _buff = (byte*)Unsafe.AsPointer(ref buffer[0]);
        Length = buffer.Length;
    }
    public ref readonly int Constructor => ref *(int*)_buff;

    private void SetConstructor(int constructor)
    {
        var p = (int*)_buff;
        *p = constructor;
    }
    public int Length { get; }
    public ReadOnlySpan<byte> ToReadOnlySpan() => new (_buff, Length);
    public static ReqDhParams Read(Span<byte> data, in int offset, out int bytesRead)
    {
        bytesRead = GetOffset(7, (byte*)Unsafe.AsPointer(ref data[offset..][0]), data.Length);
        var obj = new ReqDhParams(data.Slice(offset, bytesRead));
        return obj;
    }

    public static int GetRequiredBufferSize(int lenP, int lenQ, int lenEncryptedData)
    {
        return 4 + 16 + 16 + BufferUtils.CalculateTLBytesLength(lenP) +
               BufferUtils.CalculateTLBytesLength(lenQ) + 8 +
               BufferUtils.CalculateTLBytesLength(lenEncryptedData);
    }
    public static int ReadSize(Span<byte> data, in int offset)
    {
        return GetOffset(7, (byte*)Unsafe.AsPointer(ref data[offset..][0]), data.Length);
    }

    public static ReqDhParams Create(MemoryPool<byte> pool, ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> serverNonce,
        ReadOnlySpan<byte> p, ReadOnlySpan<byte> q, in long publicKeyFingerprint, ReadOnlySpan<byte> encryptedData, 
        out IMemoryOwner<byte> memory)
    {
        var length = GetRequiredBufferSize(p.Length, q.Length, encryptedData.Length);
        memory = pool.Rent(length);
        var obj = new ReqDhParams(memory.Memory.Span[..length]);
        obj.SetConstructor(unchecked((int)0xd712e4be));
        obj.SetNonce(nonce);
        obj.SetServerNonce(serverNonce);
        obj.SetP(p);
        obj.SetQ(q);
        obj.SetPublicKeyFingerprint(publicKeyFingerprint);
        obj.SetEncryptedData(encryptedData);
        return obj;
    }

    public ReadOnlySpan<byte> Nonce => new ((byte*)_buff + GetOffset(1, _buff, Length), 16);
    private void SetNonce(ReadOnlySpan<byte> value)
    {
        fixed (byte* p = value)
        {
            int offset = GetOffset(1, _buff, Length);
            Buffer.MemoryCopy(p, _buff + offset,
                Length - offset,value.Length);
        }
    }
    public ReadOnlySpan<byte> ServerNonce => new ((byte*)_buff + GetOffset(2, _buff, Length), 16);
    private void SetServerNonce(ReadOnlySpan<byte> value)
    {
        fixed (byte* p = value)
        {
            int offset = GetOffset(2, _buff, Length);
            Buffer.MemoryCopy(p, _buff + offset,
                Length - offset,value.Length);
        }
    }
    public ReadOnlySpan<byte> P => BufferUtils.GetTLBytes(_buff,GetOffset(3, _buff, Length), Length);
    private void SetP(ReadOnlySpan<byte> value)
    {
        var offset = GetOffset(3, _buff, Length);
        var lenBytes = BufferUtils.WriteLenBytes(_buff, value, offset, Length);
        fixed (byte* p = value)
        {
            Buffer.MemoryCopy(p, _buff + offset + lenBytes,
                Length - offset,value.Length);
        }
    }
    public ReadOnlySpan<byte> Q => BufferUtils.GetTLBytes(_buff,GetOffset(4, _buff, Length), Length);
    private void SetQ(ReadOnlySpan<byte> value)
    {
        var offset = GetOffset(4, _buff, Length);
        var lenBytes = BufferUtils.WriteLenBytes(_buff, value, offset, Length);
        fixed (byte* p = value)
        {
            Buffer.MemoryCopy(p, _buff + offset + lenBytes,
                Length - offset,value.Length);
        }
    }
    public ref readonly long PublicKeyFingerprint => ref Unsafe.As<byte, long>(
        ref Unsafe.Add(ref Unsafe.AsRef<byte>(_buff), GetOffset(5, _buff, Length)));
    private void SetPublicKeyFingerprint(in long value)
    {
        var p = (long*)(_buff + GetOffset(5, _buff, Length));
        *p = value;
    }
    public ReadOnlySpan<byte> EncryptedData => BufferUtils.GetTLBytes(_buff,GetOffset(6, _buff, Length), Length);
    private void SetEncryptedData(ReadOnlySpan<byte> value)
    {
        var offset = GetOffset(6, _buff, Length);
        var lenBytes = BufferUtils.WriteLenBytes(_buff, value, offset, Length);
        fixed (byte* p = value)
        {
            Buffer.MemoryCopy(p, _buff + offset + lenBytes,
                Length - offset,value.Length);
        }
    }
    private static int GetOffset(int index, byte* buffer, int length)
    {
        int offset = 0;
        
        if(index >= 1) offset += 4;
        if(index >= 2) offset += 16;
        if(index >= 3) offset += 16;
        if(index >= 4) offset += BufferUtils.GetTLBytesLength(buffer, offset, length);
        if(index >= 5) offset += BufferUtils.GetTLBytesLength(buffer, offset, length);
        if(index >= 6) offset += 8;
        if(index >= 7) offset  += BufferUtils.GetTLBytesLength(buffer, offset, length);
        
        return offset;
    }
}