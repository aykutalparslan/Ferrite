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
public class StickerSetImpl : StickerSet
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public StickerSetImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -673242758;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            if (_flags[0])
            {
                writer.WriteInt32(_installedDate, true);
            }

            writer.WriteInt64(_id, true);
            writer.WriteInt64(_accessHash, true);
            writer.WriteTLString(_title);
            writer.WriteTLString(_shortName);
            if (_flags[4])
            {
                writer.Write(_thumbs.TLBytes, false);
            }

            if (_flags[4])
            {
                writer.WriteInt32(_thumbDcId, true);
            }

            if (_flags[4])
            {
                writer.WriteInt32(_thumbVersion, true);
            }

            writer.WriteInt32(_count, true);
            writer.WriteInt32(_hash, true);
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

    public bool Archived
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
        }
    }

    public bool Official
    {
        get => _flags[2];
        set
        {
            serialized = false;
            _flags[2] = value;
        }
    }

    public bool Masks
    {
        get => _flags[3];
        set
        {
            serialized = false;
            _flags[3] = value;
        }
    }

    public bool Animated
    {
        get => _flags[5];
        set
        {
            serialized = false;
            _flags[5] = value;
        }
    }

    public bool Videos
    {
        get => _flags[6];
        set
        {
            serialized = false;
            _flags[6] = value;
        }
    }

    private int _installedDate;
    public int InstalledDate
    {
        get => _installedDate;
        set
        {
            serialized = false;
            _flags[0] = true;
            _installedDate = value;
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

    private string _shortName;
    public string ShortName
    {
        get => _shortName;
        set
        {
            serialized = false;
            _shortName = value;
        }
    }

    private Vector<PhotoSize> _thumbs;
    public Vector<PhotoSize> Thumbs
    {
        get => _thumbs;
        set
        {
            serialized = false;
            _flags[4] = true;
            _thumbs = value;
        }
    }

    private int _thumbDcId;
    public int ThumbDcId
    {
        get => _thumbDcId;
        set
        {
            serialized = false;
            _flags[4] = true;
            _thumbDcId = value;
        }
    }

    private int _thumbVersion;
    public int ThumbVersion
    {
        get => _thumbVersion;
        set
        {
            serialized = false;
            _flags[4] = true;
            _thumbVersion = value;
        }
    }

    private int _count;
    public int Count
    {
        get => _count;
        set
        {
            serialized = false;
            _count = value;
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

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        if (_flags[0])
        {
            _installedDate = buff.ReadInt32(true);
        }

        _id = buff.ReadInt64(true);
        _accessHash = buff.ReadInt64(true);
        _title = buff.ReadTLString();
        _shortName = buff.ReadTLString();
        if (_flags[4])
        {
            buff.Skip(4);
            _thumbs = factory.Read<Vector<PhotoSize>>(ref buff);
        }

        if (_flags[4])
        {
            _thumbDcId = buff.ReadInt32(true);
        }

        if (_flags[4])
        {
            _thumbVersion = buff.ReadInt32(true);
        }

        _count = buff.ReadInt32(true);
        _hash = buff.ReadInt32(true);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}