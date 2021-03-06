//  <auto-generated>
//  This file was auto-generated by the Ferrite TL Generator.
//  Please do not modify as all changes will be lost.
//  <auto-generated/>

#nullable enable

using System.Buffers;
using System.Runtime.CompilerServices;
using Ferrite.Utils;

namespace Ferrite.TL.slim.mtproto;

public readonly unsafe struct msg_container : ITLObjectReader, ITLSerializable
{
    private readonly byte* _buff;
    private readonly IMemoryOwner<byte>? _memoryOwner;
    private msg_container(Span<byte> buffer, IMemoryOwner<byte> memoryOwner)
    {
        _buff = (byte*)Unsafe.AsPointer(ref buffer[0]);
        Length = buffer.Length;
        _memoryOwner = memoryOwner;
    }
    private msg_container(byte* buffer, in int length, IMemoryOwner<byte> memoryOwner)
    {
        _buff = buffer;
        Length = length;
        _memoryOwner = memoryOwner;
    }
    
    public MessageContainer GetAsMessageContainer()
    {
        return new MessageContainer(_buff, Length, _memoryOwner);
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
        bytesRead = GetOffset(2, (byte*)Unsafe.AsPointer(ref data[offset..][0]), data.Length);
        var obj = new msg_container(data.Slice(offset, bytesRead), null);
        return obj;
    }
    public static ITLSerializable? Read(byte* buffer, in int length, in int offset, out int bytesRead)
    {
        bytesRead = GetOffset(2, buffer + offset, length);
        var obj = new msg_container(buffer + offset, bytesRead, null);
        return obj;
    }

    public static int GetRequiredBufferSize(int len_messages)
    {
        return 4 + len_messages;
    }
    public static msg_container Create(VectorBare<Message> messages, MemoryPool<byte>? pool = null)
    {
        var length = GetRequiredBufferSize(messages.Length);
        var memory = pool != null ? pool.Rent(length) : MemoryPool<byte>.Shared.Rent(length);
        var obj = new msg_container(memory.Memory.Span[..length], memory);
        obj.SetConstructor(unchecked((int)0x73f1f8dc));
        obj.Set_messages(messages.ToReadOnlySpan());
        return obj;
    }
    public static int ReadSize(Span<byte> data, in int offset)
    {
        return GetOffset(2, (byte*)Unsafe.AsPointer(ref data[offset..][0]), data.Length);
    }

    public static int ReadSize(byte* buffer, in int length, in int offset)
    {
        return GetOffset(2, buffer + offset, length);
    }
    public VectorBare<Message> messages => (VectorBare<Message>)VectorBare<Message>.Read(_buff, Length, GetOffset(1, _buff, Length), out var bytesRead);
    private void Set_messages(ReadOnlySpan<byte> value)
    {
        fixed (byte* p = value)
        {
            int offset = GetOffset(1, _buff, Length);
            Buffer.MemoryCopy(p, _buff + offset,
                Length - offset, value.Length);
        }
    }
    private static int GetOffset(int index, byte* buffer, int length)
    {
        int offset = 4;
        if(index >= 2) offset += VectorBare<Message>.ReadSize(buffer, length, offset);
        return offset;
    }
    public void Dispose()
    {
        _memoryOwner?.Dispose();
    }
}
