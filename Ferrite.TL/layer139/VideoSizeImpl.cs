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
public class VideoSizeImpl : VideoSize
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public VideoSizeImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -567037804;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteTLString(_type);
            writer.WriteInt32(_w, true);
            writer.WriteInt32(_h, true);
            writer.WriteInt32(_size, true);
            if (_flags[0])
            {
                writer.WriteInt64((long)_videoStartTs, true);
            }

            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private Flags _flags;
    public Flags Flags
    {
        get => _flags;
        set
        {
            serialized = false;
            _flags = value;
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

    private double _videoStartTs;
    public double VideoStartTs
    {
        get => _videoStartTs;
        set
        {
            serialized = false;
            _flags[0] = true;
            _videoStartTs = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _type = buff.ReadTLString();
        _w = buff.ReadInt32(true);
        _h = buff.ReadInt32(true);
        _size = buff.ReadInt32(true);
        if (_flags[0])
        {
            _videoStartTs = buff.ReadInt64(true);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}