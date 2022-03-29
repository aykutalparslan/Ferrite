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
public class InputMediaUploadedDocumentImpl : InputMedia
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public InputMediaUploadedDocumentImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 1530447553;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.Write(_file.TLBytes, false);
            if (_flags[2])
            {
                writer.Write(_thumb.TLBytes, false);
            }

            writer.WriteTLString(_mimeType);
            writer.Write(_attributes.TLBytes, false);
            if (_flags[0])
            {
                writer.Write(_stickers.TLBytes, false);
            }

            if (_flags[1])
            {
                writer.WriteInt32(_ttlSeconds, true);
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

    public bool NosoundVideo
    {
        get => _flags[3];
        set
        {
            serialized = false;
            _flags[3] = value;
        }
    }

    public bool ForceFile
    {
        get => _flags[4];
        set
        {
            serialized = false;
            _flags[4] = value;
        }
    }

    private InputFile _file;
    public InputFile File
    {
        get => _file;
        set
        {
            serialized = false;
            _file = value;
        }
    }

    private InputFile _thumb;
    public InputFile Thumb
    {
        get => _thumb;
        set
        {
            serialized = false;
            _flags[2] = true;
            _thumb = value;
        }
    }

    private string _mimeType;
    public string MimeType
    {
        get => _mimeType;
        set
        {
            serialized = false;
            _mimeType = value;
        }
    }

    private Vector<DocumentAttribute> _attributes;
    public Vector<DocumentAttribute> Attributes
    {
        get => _attributes;
        set
        {
            serialized = false;
            _attributes = value;
        }
    }

    private Vector<InputDocument> _stickers;
    public Vector<InputDocument> Stickers
    {
        get => _stickers;
        set
        {
            serialized = false;
            _flags[0] = true;
            _stickers = value;
        }
    }

    private int _ttlSeconds;
    public int TtlSeconds
    {
        get => _ttlSeconds;
        set
        {
            serialized = false;
            _flags[1] = true;
            _ttlSeconds = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        buff.Skip(4); _file  =  factory . Read < InputFile > ( ref  buff ) ; 
        if (_flags[2])
        {
            buff.Skip(4);
            _thumb = factory.Read<InputFile>(ref buff);
        }

        _mimeType = buff.ReadTLString();
        buff.Skip(4); _attributes  =  factory . Read < Vector < DocumentAttribute > > ( ref  buff ) ; 
        if (_flags[0])
        {
            buff.Skip(4);
            _stickers = factory.Read<Vector<InputDocument>>(ref buff);
        }

        if (_flags[1])
        {
            _ttlSeconds = buff.ReadInt32(true);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}