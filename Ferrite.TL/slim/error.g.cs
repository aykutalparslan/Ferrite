//  <auto-generated>
//  This file was auto-generated by the Ferrite TL Generator.
//  Please do not modify as all changes will be lost.
//  <auto-generated/>

#nullable enable

using System.Buffers;
using System.Runtime.CompilerServices;
using Ferrite.Utils;

namespace Ferrite.TL.slim;

public readonly unsafe struct error : ITLObjectReader, ITLSerializable
{
    private readonly byte* _buff;
    private readonly IMemoryOwner<byte>? _memoryOwner;
    private error(Span<byte> buffer, IMemoryOwner<byte> memoryOwner)
    {
        _buff = (byte*)Unsafe.AsPointer(ref buffer[0]);
        Length = buffer.Length;
        _memoryOwner = memoryOwner;
    }
    private error(byte* buffer, in int length, IMemoryOwner<byte> memoryOwner)
    {
        _buff = buffer;
        Length = length;
        _memoryOwner = memoryOwner;
    }
    
    public Error GetAsError()
    {
        return new Error(_buff, Length, _memoryOwner);
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
        bytesRead = GetOffset(3, (byte*)Unsafe.AsPointer(ref data[offset..][0]), data.Length);
        var obj = new error(data.Slice(offset, bytesRead), null);
        return obj;
    }
    public static ITLSerializable? Read(byte* buffer, in int length, in int offset, out int bytesRead)
    {
        bytesRead = GetOffset(3, buffer + offset, length);
        var obj = new error(buffer + offset, bytesRead, null);
        return obj;
    }

    public static int GetRequiredBufferSize(int len_text)
    {
        return 4 + 4 + BufferUtils.CalculateTLBytesLength(len_text);
    }
    public static error Create(int code, ReadOnlySpan<byte> text, MemoryPool<byte>? pool = null)
    {
        var length = GetRequiredBufferSize(text.Length);
        var memory = pool != null ? pool.Rent(length) : MemoryPool<byte>.Shared.Rent(length);
        memory.Memory.Span.Clear();
        var obj = new error(memory.Memory.Span[..length], memory);
        obj.SetConstructor(unchecked((int)0xc4b9f9bb));
        obj.Set_code(code);
        obj.Set_text(text);
        return obj;
    }
    public static int ReadSize(Span<byte> data, in int offset)
    {
        return GetOffset(3, (byte*)Unsafe.AsPointer(ref data[offset..][0]), data.Length);
    }

    public static int ReadSize(byte* buffer, in int length, in int offset)
    {
        return GetOffset(3, buffer + offset, length);
    }
    public ref readonly int code => ref *(int*)(_buff + GetOffset(1, _buff, Length));
    private void Set_code(in int value)
    {
        var p = (int*)(_buff + GetOffset(1, _buff, Length));
        *p = value;
    }
    public ReadOnlySpan<byte> text => BufferUtils.GetTLBytes(_buff, GetOffset(2, _buff, Length), Length);
    private void Set_text(ReadOnlySpan<byte> value)
    {
        if(value.Length == 0)
        {
            return;
        }
        var offset = GetOffset(2, _buff, Length);
        var lenBytes = BufferUtils.WriteLenBytes(_buff, value, offset, Length);
        fixed (byte* p = value)
        {
            Buffer.MemoryCopy(p, _buff + offset + lenBytes,
                Length - offset, value.Length);
        }
    }
    private static int GetOffset(int index, byte* buffer, int length)
    {
        int offset = 4;
        if(index >= 2) offset += 4;
        if(index >= 3) offset += BufferUtils.GetTLBytesLength(buffer, offset, length);
        return offset;
    }
    public void Dispose()
    {
        _memoryOwner?.Dispose();
    }
}
