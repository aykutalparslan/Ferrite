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
public class PageBlockEmbedPostImpl : PageBlock
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PageBlockEmbedPostImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -229005301;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLString(_url);
            writer.WriteInt64(_webpageId, true);
            writer.WriteInt64(_authorPhotoId, true);
            writer.WriteTLString(_author);
            writer.WriteInt32(_date, true);
            writer.Write(_blocks.TLBytes, false);
            writer.Write(_caption.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
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

    private long _webpageId;
    public long WebpageId
    {
        get => _webpageId;
        set
        {
            serialized = false;
            _webpageId = value;
        }
    }

    private long _authorPhotoId;
    public long AuthorPhotoId
    {
        get => _authorPhotoId;
        set
        {
            serialized = false;
            _authorPhotoId = value;
        }
    }

    private string _author;
    public string Author
    {
        get => _author;
        set
        {
            serialized = false;
            _author = value;
        }
    }

    private int _date;
    public int Date
    {
        get => _date;
        set
        {
            serialized = false;
            _date = value;
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
        _url = buff.ReadTLString();
        _webpageId = buff.ReadInt64(true);
        _authorPhotoId = buff.ReadInt64(true);
        _author = buff.ReadTLString();
        _date = buff.ReadInt32(true);
        buff.Skip(4); _blocks  =  factory . Read < Vector < PageBlock > > ( ref  buff ) ; 
        _caption = (PageCaption)factory.Read(buff.ReadInt32(true), ref buff);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}