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
public class ThemeImpl : Theme
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public ThemeImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1609668650;
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
            writer.WriteInt64(_accessHash, true);
            writer.WriteTLString(_slug);
            writer.WriteTLString(_title);
            if (_flags[2])
            {
                writer.Write(_document.TLBytes, false);
            }

            if (_flags[3])
            {
                writer.Write(_settings.TLBytes, false);
            }

            if (_flags[6])
            {
                writer.WriteTLString(_emoticon);
            }

            if (_flags[4])
            {
                writer.WriteInt32(_installsCount, true);
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

    public bool ForChat
    {
        get => _flags[5];
        set
        {
            serialized = false;
            _flags[5] = value;
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

    private string _title;
    public string Title
    {
        get => _title;
        set
        {
            serialized = false;
            _title = value;
        }
    }

    private Document _document;
    public Document Document
    {
        get => _document;
        set
        {
            serialized = false;
            _flags[2] = true;
            _document = value;
        }
    }

    private Vector<ThemeSettings> _settings;
    public Vector<ThemeSettings> Settings
    {
        get => _settings;
        set
        {
            serialized = false;
            _flags[3] = true;
            _settings = value;
        }
    }

    private string _emoticon;
    public string Emoticon
    {
        get => _emoticon;
        set
        {
            serialized = false;
            _flags[6] = true;
            _emoticon = value;
        }
    }

    private int _installsCount;
    public int InstallsCount
    {
        get => _installsCount;
        set
        {
            serialized = false;
            _flags[4] = true;
            _installsCount = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _id = buff.ReadInt64(true);
        _accessHash = buff.ReadInt64(true);
        _slug = buff.ReadTLString();
        _title = buff.ReadTLString();
        if (_flags[2])
        {
            buff.Skip(4);
            _document = factory.Read<Document>(ref buff);
        }

        if (_flags[3])
        {
            buff.Skip(4);
            _settings = factory.Read<Vector<ThemeSettings>>(ref buff);
        }

        if (_flags[6])
        {
            _emoticon = buff.ReadTLString();
        }

        if (_flags[4])
        {
            _installsCount = buff.ReadInt32(true);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}