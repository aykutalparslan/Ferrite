/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using System;
using System.Buffers;
using System.Collections;
using System.Runtime.InteropServices;
using DotNext.Buffers;
using DotNext.IO;

namespace Ferrite.TL;

public class VectorOfLong : ITLObject, ICollection<long>
{
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private List<long> list;
    private bool serialized = false;

    public VectorOfLong()
    {
        list = new List<long>();
    }
    public VectorOfLong(int capacity)
    {
        list = new List<long>(capacity);
    }

    public long this[int index] { get => list[index]; }

    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
            {
                return writer.ToReadOnlySequence();
            }
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt32(list.Count, true);
            foreach (var item in list)
            {
                writer.WriteInt64(item, true);
            }

            return writer.ToReadOnlySequence();
        }
    }

    public void Parse(ref SequenceReader buff)
    {
        
        int size = buff.ReadInt32(true);

        for (int i = 0; i < size; i++)
        {
            list.Add(buff.ReadInt64(true));
        }
    }

    public void WriteTo(Span<byte> buff)
    {
        SpanWriter<byte> spanWriter = new SpanWriter<byte>(buff);
        foreach (var item in TLBytes)
        {
            spanWriter.Write(item.Span);
        }
    }

    public int Constructor => unchecked((int)0x1cb5c415);

    public int Count => list.Count;

    public bool IsReadOnly => false;

    public bool IsMethod => throw new NotImplementedException();

    public void Add(long item)
    {
        serialized = false;
        list.Add(item);
    }

    public void Clear()
    {
        serialized = false;
        list.Clear();
    }

    public bool Contains(long item)
    {
        return list.Contains(item);
    }

    public void CopyTo(long[] array, int arrayIndex)
    {
        list.CopyTo(array, arrayIndex);
    }

    public IEnumerator<long> GetEnumerator()
    {
        return list.GetEnumerator();
    }

    public bool Remove(long item)
    {
        serialized = false;
        return list.Remove(item);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return list.AsEnumerable<long>().GetEnumerator();
    }

    public ITLObject Execute(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }
}

