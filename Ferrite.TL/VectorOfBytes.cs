/*
 *   Project Ferrite is an Implementation Telegram Server API
 *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Affero General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Affero General Public License for more details.
 *
 *   You should have received a copy of the GNU Affero General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Buffers;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Utils;

namespace Ferrite.TL;

public class VectorOfBytes : ITLObject, ICollection<byte[]>
{
    private SparseBufferWriter<byte> writer =
        new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private List<byte[]> list;
    private bool serialized = false;

    public VectorOfBytes()
    {
        list = new List<byte[]>();
    }
    public VectorOfBytes(int capacity)
    {
        list = new List<byte[]>(capacity);
    }

    public byte[] this[int index] { get => list[index]; }

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
                writer.WriteTLBytes(item);
            }

            return writer.ToReadOnlySequence();
        }
    }

    public void Parse(ref SequenceReader buff)
    {
        int size = buff.ReadInt32(true);

        for (int i = 0; i < size; i++)
        {
            list.Add(buff.ReadTLBytes().ToArray());
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

    public void Add(byte[] item)
    {
        serialized = false;
        list.Add(item);
    }

    public void Clear()
    {
        serialized = false;
        list.Clear();
    }

    public bool Contains(byte[] item)
    {
        return list.Contains(item);
    }

    public void CopyTo(byte[][] array, int arrayIndex)
    {
        list.CopyTo(array, arrayIndex);
    }

    public IEnumerator<byte[]> GetEnumerator()
    {
        return list.GetEnumerator();
    }

    public bool Remove(byte[] item)
    {
        serialized = false;
        return list.Remove(item);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return list.AsEnumerable<byte[]>().GetEnumerator();
    }

    public ITLObject Execute(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }
}

