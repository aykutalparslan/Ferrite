//  <auto-generated>
//  This file was auto-generated by the Ferrite TL Generator.
//  Please do not modify as all changes will be lost.
//  <auto-generated/>

#nullable enable

using System.Buffers;
using System.Runtime.CompilerServices;
using Ferrite.Utils;

namespace Ferrite.TL.slim.mtproto;

public readonly unsafe struct bad_msg_notification : ITLObjectReader, ITLSerializable
{
    private readonly byte* _buff;
    private readonly IMemoryOwner<byte>? _memoryOwner;
    private bad_msg_notification(Span<byte> buffer, IMemoryOwner<byte> memoryOwner)
    {
        _buff = (byte*)Unsafe.AsPointer(ref buffer[0]);
        Length = buffer.Length;
        _memoryOwner = memoryOwner;
    }
    private bad_msg_notification(byte* buffer, in int length, IMemoryOwner<byte> memoryOwner)
    {
        _buff = buffer;
        Length = length;
        _memoryOwner = memoryOwner;
    }
    
    public BadMsgNotification GetAsBadMsgNotification()
    {
        return new BadMsgNotification(_buff, Length, _memoryOwner);
    }
    public ref readonly int Constructor => ref *(int*)_buff;

    private void SetConstructor(int constructor)
    {
        var p = (int*)_buff;
        *p = constructor;
    }
    public int Length { get; }
    public ReadOnlySpan<byte> ToReadOnlySpan() => new (_buff, Length);
    public static ITLSerializable? Read(Span<byte> data, in int offset, out int bytesRead)
    {
        bytesRead = GetOffset(4, (byte*)Unsafe.AsPointer(ref data[offset..][0]), data.Length);
        var obj = new bad_msg_notification(data.Slice(offset, bytesRead), null);
        return obj;
    }
    public static ITLSerializable? Read(byte* buffer, in int length, in int offset, out int bytesRead)
    {
        bytesRead = GetOffset(4, buffer + offset, length);
        var obj = new bad_msg_notification(buffer + offset, bytesRead, null);
        return obj;
    }

    public static int GetRequiredBufferSize()
    {
        return 4 + 8 + 4 + 4;
    }
    public static bad_msg_notification Create(long bad_msg_id, int bad_msg_seqno, int error_code, MemoryPool<byte>? pool = null)
    {
        var length = GetRequiredBufferSize();
        var memory = pool != null ? pool.Rent(length) : MemoryPool<byte>.Shared.Rent(length);
        memory.Memory.Span.Clear();
        var obj = new bad_msg_notification(memory.Memory.Span[..length], memory);
        obj.SetConstructor(unchecked((int)0xa7eff811));
        obj.Set_bad_msg_id(bad_msg_id);
        obj.Set_bad_msg_seqno(bad_msg_seqno);
        obj.Set_error_code(error_code);
        return obj;
    }
    public static int ReadSize(Span<byte> data, in int offset)
    {
        return GetOffset(4, (byte*)Unsafe.AsPointer(ref data[offset..][0]), data.Length);
    }

    public static int ReadSize(byte* buffer, in int length, in int offset)
    {
        return GetOffset(4, buffer + offset, length);
    }
    public ref readonly long bad_msg_id => ref *(long*)(_buff + GetOffset(1, _buff, Length));
    private void Set_bad_msg_id(in long value)
    {
        var p = (long*)(_buff + GetOffset(1, _buff, Length));
        *p = value;
    }
    public ref readonly int bad_msg_seqno => ref *(int*)(_buff + GetOffset(2, _buff, Length));
    private void Set_bad_msg_seqno(in int value)
    {
        var p = (int*)(_buff + GetOffset(2, _buff, Length));
        *p = value;
    }
    public ref readonly int error_code => ref *(int*)(_buff + GetOffset(3, _buff, Length));
    private void Set_error_code(in int value)
    {
        var p = (int*)(_buff + GetOffset(3, _buff, Length));
        *p = value;
    }
    private static int GetOffset(int index, byte* buffer, int length)
    {
        int offset = 4;
        if(index >= 2) offset += 8;
        if(index >= 3) offset += 4;
        if(index >= 4) offset += 4;
        return offset;
    }
    public void Dispose()
    {
        _memoryOwner?.Dispose();
    }
}
