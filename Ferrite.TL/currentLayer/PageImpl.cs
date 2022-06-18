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

namespace Ferrite.TL.currentLayer;
public class PageImpl : Page
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PageImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1738178803;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteTLString(_url);
            writer.Write(_blocks.TLBytes, false);
            writer.Write(_photos.TLBytes, false);
            writer.Write(_documents.TLBytes, false);
            if (_flags[3])
            {
                writer.WriteInt32(_views, true);
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

    public bool Part
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool Rtl
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
        }
    }

    public bool V2
    {
        get => _flags[2];
        set
        {
            serialized = false;
            _flags[2] = value;
        }
    }

    private string _url;
    public string Url
    {
        get => _url;
        set
        {
            serialized = false;
            _url = value;
        }
    }

    private Vector<PageBlock> _blocks;
    public Vector<PageBlock> Blocks
    {
        get => _blocks;
        set
        {
            serialized = false;
            _blocks = value;
        }
    }

    private Vector<Photo> _photos;
    public Vector<Photo> Photos
    {
        get => _photos;
        set
        {
            serialized = false;
            _photos = value;
        }
    }

    private Vector<Document> _documents;
    public Vector<Document> Documents
    {
        get => _documents;
        set
        {
            serialized = false;
            _documents = value;
        }
    }

    private int _views;
    public int Views
    {
        get => _views;
        set
        {
            serialized = false;
            _flags[3] = true;
            _views = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _url = buff.ReadTLString();
        buff.Skip(4); _blocks  =  factory . Read < Vector < PageBlock > > ( ref  buff ) ; 
        buff.Skip(4); _photos  =  factory . Read < Vector < Photo > > ( ref  buff ) ; 
        buff.Skip(4); _documents  =  factory . Read < Vector < Document > > ( ref  buff ) ; 
        if (_flags[3])
        {
            _views = buff.ReadInt32(true);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}