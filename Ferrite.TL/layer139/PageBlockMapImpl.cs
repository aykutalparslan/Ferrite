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
public class PageBlockMapImpl : PageBlock
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PageBlockMapImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1538310410;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_geo.TLBytes, false);
            writer.WriteInt32(_zoom, true);
            writer.WriteInt32(_w, true);
            writer.WriteInt32(_h, true);
            writer.Write(_caption.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private GeoPoint _geo;
    public GeoPoint Geo
    {
        get => _geo;
        set
        {
            serialized = false;
            _geo = value;
        }
    }

    private int _zoom;
    public int Zoom
    {
        get => _zoom;
        set
        {
            serialized = false;
            _zoom = value;
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

    private PageCaption _caption;
    public PageCaption Caption
    {
        get => _caption;
        set
        {
            serialized = false;
            _caption = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        buff.Skip(4); _geo  =  factory . Read < GeoPoint > ( ref  buff ) ; 
        _zoom = buff.ReadInt32(true);
        _w = buff.ReadInt32(true);
        _h = buff.ReadInt32(true);
        buff.Skip(4); _caption  =  factory . Read < PageCaption > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}