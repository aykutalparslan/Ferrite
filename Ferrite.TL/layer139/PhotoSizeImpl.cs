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
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Utils;

namespace Ferrite.TL.layer139;
public class PhotoSizeImpl : PhotoSize
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PhotoSizeImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 1976012384;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLString(_type);
            writer.WriteInt32(_w, true);
            writer.WriteInt32(_h, true);
            writer.WriteInt32(_size, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private string _type;
    public string Type
    {
        get => _type;
        set
        {
            serialized = false;
            _type = value;
        }
    }

    private int _w;
    public int W
    {
        get => _w;
        set
        {
            serialized = false;
            _w = value;
        }
    }

    private int _h;
    public int H
    {
        get => _h;
        set
        {
            serialized = false;
            _h = value;
        }
    }

    private int _size;
    public int Size
    {
        get => _size;
        set
        {
            serialized = false;
            _size = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _type = buff.ReadTLString();
        _w = buff.ReadInt32(true);
        _h = buff.ReadInt32(true);
        _size = buff.ReadInt32(true);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}