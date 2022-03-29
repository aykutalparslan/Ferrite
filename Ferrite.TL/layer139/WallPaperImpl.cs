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
public class WallPaperImpl : WallPaper
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public WallPaperImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1539849235;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(_id, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt64(_accessHash, true);
            writer.WriteTLString(_slug);
            writer.Write(_document.TLBytes, false);
            if (_flags[2])
            {
                writer.Write(_settings.TLBytes, false);
            }

            serialized = true;
            return writer.ToReadOnlySequence();
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

    public bool Creator
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool Default
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
        }
    }

    public bool Pattern
    {
        get => _flags[3];
        set
        {
            serialized = false;
            _flags[3] = value;
        }
    }

    public bool Dark
    {
        get => _flags[4];
        set
        {
            serialized = false;
            _flags[4] = value;
        }
    }

    private long _accessHash;
    public long AccessHash
    {
        get => _accessHash;
        set
        {
            serialized = false;
            _accessHash = value;
        }
    }

    private string _slug;
    public string Slug
    {
        get => _slug;
        set
        {
            serialized = false;
            _slug = value;
        }
    }

    private Document _document;
    public Document Document
    {
        get => _document;
        set
        {
            serialized = false;
            _document = value;
        }
    }

    private WallPaperSettings _settings;
    public WallPaperSettings Settings
    {
        get => _settings;
        set
        {
            serialized = false;
            _flags[2] = true;
            _settings = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _id = buff.ReadInt64(true);
        _flags = buff.Read<Flags>();
        _accessHash = buff.ReadInt64(true);
        _slug = buff.ReadTLString();
        buff.Skip(4); _document  =  factory . Read < Document > ( ref  buff ) ; 
        if (_flags[2])
        {
            buff.Skip(4);
            _settings = factory.Read<WallPaperSettings>(ref buff);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}