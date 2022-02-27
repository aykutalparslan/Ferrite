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

public class VectorOfDouble : ITLObject, ICollection<double>
{
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private List<double> list;
    private bool serialized = false;

    public VectorOfDouble()
    {
        list = new List<double>();
    }
    public VectorOfDouble(int capacity)
    {
        list = new List<double>(capacity);
    }

    public double this[int index] { get => list[index]; }

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
                writer.WriteInt64((long)item, true);
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

    public void Add(double item)
    {
        serialized = false;
        list.Add(item);
    }

    public void Clear()
    {
        serialized = false;
        list.Clear();
    }

    public bool Contains(double item)
    {
        return list.Contains(item);
    }

    public void CopyTo(double[] array, int arrayIndex)
    {
        list.CopyTo(array, arrayIndex);
    }

    public IEnumerator<double> GetEnumerator()
    {
        return list.GetEnumerator();
    }

    public bool Remove(double item)
    {
        serialized = false;
        return list.Remove(item);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return list.AsEnumerable<double>().GetEnumerator();
    }

    public ITLObject Execute(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }
}

