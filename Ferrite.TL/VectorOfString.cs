﻿/*
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

public class VectorOfString : ITLObject, ICollection<string>
{
    private SparseBufferWriter<byte> writer =
        new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private List<string> list;
    private bool serialized = false;

    public VectorOfString()
    {
        list = new List<string>();
    }
    public VectorOfString(int capacity)
    {
        list = new List<string>(capacity);
    }

    public string this[int index] { get => list[index]; }

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
                writer.WriteTLString(item);
            }

            return writer.ToReadOnlySequence();
        }
    }

    public void Parse(ref SequenceReader buff)
    {
        int size = buff.ReadInt32(true);

        for (int i = 0; i < size; i++)
        {
            list.Add(buff.ReadTLString());
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

    public void Add(string item)
    {
        serialized = false;
        list.Add(item);
    }

    public void Clear()
    {
        serialized = false;
        list.Clear();
    }

    public bool Contains(string item)
    {
        return list.Contains(item);
    }

    public void CopyTo(string[] array, int arrayIndex)
    {
        list.CopyTo(array, arrayIndex);
    }

    public IEnumerator<string> GetEnumerator()
    {
        return list.GetEnumerator();
    }

    public bool Remove(string item)
    {
        serialized = false;
        return list.Remove(item);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return list.AsEnumerable<string>().GetEnumerator();
    }

    public ITLObject Execute(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }
}

