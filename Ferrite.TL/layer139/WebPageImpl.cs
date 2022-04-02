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
public class WebPageImpl : WebPage
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public WebPageImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -392411726;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt64(_id, true);
            writer.WriteTLString(_url);
            writer.WriteTLString(_displayUrl);
            writer.WriteInt32(_hash, true);
            if (_flags[0])
            {
                writer.WriteTLString(_type);
            }

            if (_flags[1])
            {
                writer.WriteTLString(_siteName);
            }

            if (_flags[2])
            {
                writer.WriteTLString(_title);
            }

            if (_flags[3])
            {
                writer.WriteTLString(_description);
            }

            if (_flags[4])
            {
                writer.Write(_photo.TLBytes, false);
            }

            if (_flags[5])
            {
                writer.WriteTLString(_embedUrl);
            }

            if (_flags[5])
            {
                writer.WriteTLString(_embedType);
            }

            if (_flags[6])
            {
                writer.WriteInt32(_embedWidth, true);
            }

            if (_flags[6])
            {
                writer.WriteInt32(_embedHeight, true);
            }

            if (_flags[7])
            {
                writer.WriteInt32(_duration, true);
            }

            if (_flags[8])
            {
                writer.WriteTLString(_author);
            }

            if (_flags[9])
            {
                writer.Write(_document.TLBytes, false);
            }

            if (_flags[10])
            {
                writer.Write(_cachedPage.TLBytes, false);
            }

            if (_flags[12])
            {
                writer.Write(_attributes.TLBytes, false);
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

    private long _id;
    public long Id
    {
        get => _id;
        set
        {
            serialized = false;
            _id = value;
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

    private string _displayUrl;
    public string DisplayUrl
    {
        get => _displayUrl;
        set
        {
            serialized = false;
            _displayUrl = value;
        }
    }

    private int _hash;
    public int Hash
    {
        get => _hash;
        set
        {
            serialized = false;
            _hash = value;
        }
    }

    private string _type;
    public string Type
    {
        get => _type;
        set
        {
            serialized = false;
            _flags[0] = true;
            _type = value;
        }
    }

    private string _siteName;
    public string SiteName
    {
        get => _siteName;
        set
        {
            serialized = false;
            _flags[1] = true;
            _siteName = value;
        }
    }

    private string _title;
    public string Title
    {
        get => _title;
        set
        {
            serialized = false;
            _flags[2] = true;
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
            _flags[3] = true;
            _description = value;
        }
    }

    private Photo _photo;
    public Photo Photo
    {
        get => _photo;
        set
        {
            serialized = false;
            _flags[4] = true;
            _photo = value;
        }
    }

    private string _embedUrl;
    public string EmbedUrl
    {
        get => _embedUrl;
        set
        {
            serialized = false;
            _flags[5] = true;
            _embedUrl = value;
        }
    }

    private string _embedType;
    public string EmbedType
    {
        get => _embedType;
        set
        {
            serialized = false;
            _flags[5] = true;
            _embedType = value;
        }
    }

    private int _embedWidth;
    public int EmbedWidth
    {
        get => _embedWidth;
        set
        {
            serialized = false;
            _flags[6] = true;
            _embedWidth = value;
        }
    }

    private int _embedHeight;
    public int EmbedHeight
    {
        get => _embedHeight;
        set
        {
            serialized = false;
            _flags[6] = true;
            _embedHeight = value;
        }
    }

    private int _duration;
    public int Duration
    {
        get => _duration;
        set
        {
            serialized = false;
            _flags[7] = true;
            _duration = value;
        }
    }

    private string _author;
    public string Author
    {
        get => _author;
        set
        {
            serialized = false;
            _flags[8] = true;
            _author = value;
        }
    }

    private Document _document;
    public Document Document
    {
        get => _document;
        set
        {
            serialized = false;
            _flags[9] = true;
            _document = value;
        }
    }

    private Page _cachedPage;
    public Page CachedPage
    {
        get => _cachedPage;
        set
        {
            serialized = false;
            _flags[10] = true;
            _cachedPage = value;
        }
    }

    private Vector<WebPageAttribute> _attributes;
    public Vector<WebPageAttribute> Attributes
    {
        get => _attributes;
        set
        {
            serialized = false;
            _flags[12] = true;
            _attributes = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _id = buff.ReadInt64(true);
        _url = buff.ReadTLString();
        _displayUrl = buff.ReadTLString();
        _hash = buff.ReadInt32(true);
        if (_flags[0])
        {
            _type = buff.ReadTLString();
        }

        if (_flags[1])
        {
            _siteName = buff.ReadTLString();
        }

        if (_flags[2])
        {
            _title = buff.ReadTLString();
        }

        if (_flags[3])
        {
            _description = buff.ReadTLString();
        }

        if (_flags[4])
        {
            _photo = (Photo)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[5])
        {
            _embedUrl = buff.ReadTLString();
        }

        if (_flags[5])
        {
            _embedType = buff.ReadTLString();
        }

        if (_flags[6])
        {
            _embedWidth = buff.ReadInt32(true);
        }

        if (_flags[6])
        {
            _embedHeight = buff.ReadInt32(true);
        }

        if (_flags[7])
        {
            _duration = buff.ReadInt32(true);
        }

        if (_flags[8])
        {
            _author = buff.ReadTLString();
        }

        if (_flags[9])
        {
            _document = (Document)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[10])
        {
            _cachedPage = (Page)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[12])
        {
            buff.Skip(4);
            _attributes = factory.Read<Vector<WebPageAttribute>>(ref buff);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}