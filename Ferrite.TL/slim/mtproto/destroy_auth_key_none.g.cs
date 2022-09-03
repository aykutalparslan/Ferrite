//  <auto-generated>
//  This file was auto-generated by the Ferrite TL Generator.
//  Please do not modify as all changes will be lost.
//  <auto-generated/>

#nullable enable

using System.Buffers;
using System.Runtime.InteropServices;
using Ferrite.Utils;

namespace Ferrite.TL.slim.mtproto;

public readonly ref struct destroy_auth_key_none
{
    private readonly Span<byte> _buff;
    public destroy_auth_key_none(Span<byte> buff)
    {
        _buff = buff;
    }
    
    public readonly int Constructor => MemoryMarshal.Read<int>(_buff);

    private void SetConstructor(int constructor)
    {
        MemoryMarshal.Write(_buff.Slice(0, 4), ref constructor);
    }
    public int Length => _buff.Length;
    public ReadOnlySpan<byte> ToReadOnlySpan() => _buff;
    public static Span<byte> Read(Span<byte> data, int offset)
    {
        var bytesRead = GetOffset(1, data[offset..]);
        if (bytesRead > data.Length + offset)
        {
            return Span<byte>.Empty;
        }
        return data.Slice(offset, bytesRead);
    }

    public static int GetRequiredBufferSize()
    {
        return 4;
    }
    public static destroy_auth_key_none Create(out IMemoryOwner<byte> memory, MemoryPool<byte>? pool = null)
    {
        var length = GetRequiredBufferSize();
        memory = pool != null ? pool.Rent(length) : MemoryPool<byte>.Shared.Rent(length);
        memory.Memory.Span.Clear();
        var obj = new destroy_auth_key_none(memory.Memory.Span[..length]);
        obj.SetConstructor(unchecked((int)0x0a9f2259));
        return obj;
    }
    public static int ReadSize(Span<byte> data, int offset)
    {
        return GetOffset(1, data[offset..]);
    }
    private static int GetOffset(int index, Span<byte> buffer)
    {
        int offset = 4;
        return offset;
    }
}
