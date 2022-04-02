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
public class PageBlockPhotoImpl : PageBlock
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PageBlockPhotoImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 391759200;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt64(_photoId, true);
            writer.Write(_caption.TLBytes, false);
            if (_flags[0])
            {
                writer.WriteTLString(_url);
            }

            if (_flags[0])
            {
                writer.WriteInt64(_webpageId, true);
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

    private long _photoId;
    public long PhotoId
    {
        get => _photoId;
        set
        {
            serialized = false;
            _photoId = value;
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

    private string _url;
    public string Url
    {
        get => _url;
        set
        {
            serialized = false;
            _flags[0] = true;
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
            _flags[0] = true;
            _webpageId = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _photoId = buff.ReadInt64(true);
        _caption = (PageCaption)factory.Read(buff.ReadInt32(true), ref buff);
        if (_flags[0])
        {
            _url = buff.ReadTLString();
        }

        if (_flags[0])
        {
            _webpageId = buff.ReadInt64(true);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}