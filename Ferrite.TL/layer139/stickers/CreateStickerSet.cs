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

namespace Ferrite.TL.layer139.stickers;
public class CreateStickerSet : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public CreateStickerSet(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -1876841625;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.Write(_userId.TLBytes, false);
            writer.WriteTLString(_title);
            writer.WriteTLString(_shortName);
            if (_flags[2])
            {
                writer.Write(_thumb.TLBytes, false);
            }

            writer.Write(_stickers.TLBytes, false);
            if (_flags[3])
            {
                writer.WriteTLString(_software);
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

    public bool Masks
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool Animated
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
        }
    }

    public bool Videos
    {
        get => _flags[4];
        set
        {
            serialized = false;
            _flags[4] = value;
        }
    }

    private InputUser _userId;
    public InputUser UserId
    {
        get => _userId;
        set
        {
            serialized = false;
            _userId = value;
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

    private InputDocument _thumb;
    public InputDocument Thumb
    {
        get => _thumb;
        set
        {
            serialized = false;
            _flags[2] = true;
            _thumb = value;
        }
    }

    private Vector<InputStickerSetItem> _stickers;
    public Vector<InputStickerSetItem> Stickers
    {
        get => _stickers;
        set
        {
            serialized = false;
            _stickers = value;
        }
    }

    private string _software;
    public string Software
    {
        get => _software;
        set
        {
            serialized = false;
            _flags[3] = true;
            _software = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _userId = (InputUser)factory.Read(buff.ReadInt32(true), ref buff);
        _title = buff.ReadTLString();
        _shortName = buff.ReadTLString();
        if (_flags[2])
        {
            _thumb = (InputDocument)factory.Read(buff.ReadInt32(true), ref buff);
        }

        buff.Skip(4); _stickers  =  factory . Read < Vector < InputStickerSetItem > > ( ref  buff ) ; 
        if (_flags[3])
        {
            _software = buff.ReadTLString();
        }
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}