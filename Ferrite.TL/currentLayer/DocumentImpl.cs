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
public class DocumentImpl : Document
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public DocumentImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 512177195;
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
            writer.WriteTLBytes(_fileReference);
            writer.WriteInt32(_date, true);
            writer.WriteTLString(_mimeType);
            writer.WriteInt32(_size, true);
            if (_flags[0])
            {
                writer.Write(_thumbs.TLBytes, false);
            }

            if (_flags[1])
            {
                writer.Write(_videoThumbs.TLBytes, false);
            }

            writer.WriteInt32(_dcId, true);
            writer.Write(_attributes.TLBytes, false);
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

    private byte[] _fileReference;
    public byte[] FileReference
    {
        get => _fileReference;
        set
        {
            serialized = false;
            _fileReference = value;
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

    private int _size;
    public int Size
    {
        get => _size;
        set
        {
            serialized = false;
            _size = value;
        }
    }

    private Vector<PhotoSize> _thumbs;
    public Vector<PhotoSize> Thumbs
    {
        get => _thumbs;
        set
        {
            serialized = false;
            _flags[0] = true;
            _thumbs = value;
        }
    }

    private Vector<VideoSize> _videoThumbs;
    public Vector<VideoSize> VideoThumbs
    {
        get => _videoThumbs;
        set
        {
            serialized = false;
            _flags[1] = true;
            _videoThumbs = value;
        }
    }

    private int _dcId;
    public int DcId
    {
        get => _dcId;
        set
        {
            serialized = false;
            _dcId = value;
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

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _id = buff.ReadInt64(true);
        _accessHash = buff.ReadInt64(true);
        _fileReference = buff.ReadTLBytes().ToArray();
        _date = buff.ReadInt32(true);
        _mimeType = buff.ReadTLString();
        _size = buff.ReadInt32(true);
        if (_flags[0])
        {
            buff.Skip(4);
            _thumbs = factory.Read<Vector<PhotoSize>>(ref buff);
        }

        if (_flags[1])
        {
            buff.Skip(4);
            _videoThumbs = factory.Read<Vector<VideoSize>>(ref buff);
        }

        _dcId = buff.ReadInt32(true);
        buff.Skip(4); _attributes  =  factory . Read < Vector < DocumentAttribute > > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}