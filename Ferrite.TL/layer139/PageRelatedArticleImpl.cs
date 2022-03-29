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
public class PageRelatedArticleImpl : PageRelatedArticle
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PageRelatedArticleImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1282352120;
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
            writer.WriteInt64(_webpageId, true);
            if (_flags[0])
            {
                writer.WriteTLString(_title);
            }

            if (_flags[1])
            {
                writer.WriteTLString(_description);
            }

            if (_flags[2])
            {
                writer.WriteInt64(_photoId, true);
            }

            if (_flags[3])
            {
                writer.WriteTLString(_author);
            }

            if (_flags[4])
            {
                writer.WriteInt32(_publishedDate, true);
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

    private string _title;
    public string Title
    {
        get => _title;
        set
        {
            serialized = false;
            _flags[0] = true;
            _title = value;
        }
    }

    private string _description;
    public string Description
    {
        get => _description;
        set
        {
            serialized = false;
            _flags[1] = true;
            _description = value;
        }
    }

    private long _photoId;
    public long PhotoId
    {
        get => _photoId;
        set
        {
            serialized = false;
            _flags[2] = true;
            _photoId = value;
        }
    }

    private string _author;
    public string Author
    {
        get => _author;
        set
        {
            serialized = false;
            _flags[3] = true;
            _author = value;
        }
    }

    private int _publishedDate;
    public int PublishedDate
    {
        get => _publishedDate;
        set
        {
            serialized = false;
            _flags[4] = true;
            _publishedDate = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _url = buff.ReadTLString();
        _webpageId = buff.ReadInt64(true);
        if (_flags[0])
        {
            _title = buff.ReadTLString();
        }

        if (_flags[1])
        {
            _description = buff.ReadTLString();
        }

        if (_flags[2])
        {
            _photoId = buff.ReadInt64(true);
        }

        if (_flags[3])
        {
            _author = buff.ReadTLString();
        }

        if (_flags[4])
        {
            _publishedDate = buff.ReadInt32(true);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}