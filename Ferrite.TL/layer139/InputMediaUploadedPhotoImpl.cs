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
public class InputMediaUploadedPhotoImpl : InputMedia
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public InputMediaUploadedPhotoImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 505969924;
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
        _file = (InputFile)factory.Read(buff.ReadInt32(true), ref buff);
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