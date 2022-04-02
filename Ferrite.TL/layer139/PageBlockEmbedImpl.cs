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
public class PageBlockEmbedImpl : PageBlock
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PageBlockEmbedImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1468953147;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            if (_flags[1])
            {
                writer.WriteTLString(_url);
            }

            if (_flags[2])
            {
                writer.WriteTLString(_html);
            }

            if (_flags[4])
            {
                writer.WriteInt64(_posterPhotoId, true);
            }

            if (_flags[5])
            {
                writer.WriteInt32(_w, true);
            }

            if (_flags[5])
            {
                writer.WriteInt32(_h, true);
            }

            writer.Write(_caption.TLBytes, false);
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

    public bool FullWidth
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool AllowScrolling
    {
        get => _flags[3];
        set
        {
            serialized = false;
            _flags[3] = value;
        }
    }

    private string _url;
    public string Url
    {
        get => _url;
        set
        {
            serialized = false;
            _flags[1] = true;
            _url = value;
        }
    }

    private string _html;
    public string Html
    {
        get => _html;
        set
        {
            serialized = false;
            _flags[2] = true;
            _html = value;
        }
    }

    private long _posterPhotoId;
    public long PosterPhotoId
    {
        get => _posterPhotoId;
        set
        {
            serialized = false;
            _flags[4] = true;
            _posterPhotoId = value;
        }
    }

    private int _w;
    public int W
    {
        get => _w;
        set
        {
            serialized = false;
            _flags[5] = true;
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
            _flags[5] = true;
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
        _flags = buff.Read<Flags>();
        if (_flags[1])
        {
            _url = buff.ReadTLString();
        }

        if (_flags[2])
        {
            _html = buff.ReadTLString();
        }

        if (_flags[4])
        {
            _posterPhotoId = buff.ReadInt64(true);
        }

        if (_flags[5])
        {
            _w = buff.ReadInt32(true);
        }

        if (_flags[5])
        {
            _h = buff.ReadInt32(true);
        }

        _caption = (PageCaption)factory.Read(buff.ReadInt32(true), ref buff);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}