//  <auto-generated>
//  This file was auto-generated by the Ferrite TL Generator.
//  Please do not modify as all changes will be lost.
//  <auto-generated/>

#nullable enable

using System.Buffers;
using System.Runtime.CompilerServices;
using Ferrite.Utils;

namespace Ferrite.TL.slim.mtproto;

public readonly unsafe struct rpc_result : ITLObjectReader, ITLSerializable
{
    private readonly byte* _buff;
    private readonly IMemoryOwner<byte>? _memoryOwner;
    private rpc_result(Span<byte> buffer, IMemoryOwner<byte> memoryOwner)
    {
        _buff = (byte*)Unsafe.AsPointer(ref buffer[0]);
        Length = buffer.Length;
        _memoryOwner = memoryOwner;
    }
    private rpc_result(byte* buffer, in int length, IMemoryOwner<byte> memoryOwner)
    {
        _buff = buffer;
        Length = length;
        _memoryOwner = memoryOwner;
    }
    
    public RpcResult GetAsRpcResult()
    {
        return new RpcResult(_buff, Length, _memoryOwner);
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
        var obj = new rpc_result(data.Slice(offset, bytesRead), null);
        return obj;
    }
    public static ITLSerializable? Read(byte* buffer, in int length, in int offset, out int bytesRead)
    {
        bytesRead = GetOffset(3, buffer + offset, length);
        var obj = new rpc_result(buffer + offset, bytesRead, null);
        return obj;
    }

    public static int GetRequiredBufferSize(int len_result)
    {
        return 4 + 8 + len_result;
    }
    public static rpc_result Create(long req_msg_id, ITLSerializable result, MemoryPool<byte>? pool = null)
    {
        var length = GetRequiredBufferSize(result.Length);
        var memory = pool != null ? pool.Rent(length) : MemoryPool<byte>.Shared.Rent(length);
        var obj = new rpc_result(memory.Memory.Span[..length], memory);
        obj.SetConstructor(unchecked((int)0xf35c6d01));
        obj.Set_req_msg_id(req_msg_id);
        obj.Set_result(result.ToReadOnlySpan());
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
    public ref readonly long req_msg_id => ref *(long*)(_buff + GetOffset(1, _buff, Length));
    private void Set_req_msg_id(in long value)
    {
        var p = (long*)(_buff + GetOffset(1, _buff, Length));
        *p = value;
    }
    public ITLSerializable result => BoxedObject.Read(_buff, Length, GetOffset(2, _buff, Length), out var bytesRead);
    private void Set_result(ReadOnlySpan<byte> value)
    {
        fixed (byte* p = value)
        {
            int offset = GetOffset(2, _buff, Length);
            Buffer.MemoryCopy(p, _buff + offset,
                Length - offset, value.Length);
        }
    }
    private static int GetOffset(int index, byte* buffer, int length)
    {
        int offset = 4;
        if(index >= 2) offset += 8;
        if(index >= 3) offset += BoxedObject.ReadSize(buffer, length, offset);
        return offset;
    }
    public void Dispose()
    {
        _memoryOwner?.Dispose();
    }
}
