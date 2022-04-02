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
public class InputMediaDocumentImpl : InputMedia
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public InputMediaDocumentImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 860303448;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.Write(_id.TLBytes, false);
            if (_flags[0])
            {
                writer.WriteInt32(_ttlSeconds, true);
            }

            if (_flags[1])
            {
                writer.WriteTLString(_query);
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

    private InputDocument _id;
    public InputDocument Id
    {
        get => _id;
        set
        {
            serialized = false;
            _id = value;
        }
    }

    private int _ttlSeconds;
    public int TtlSeconds
    {
        get => _ttlSeconds;
        set
        {
            serialized = false;
            _flags[0] = true;
            _ttlSeconds = value;
        }
    }

    private string _query;
    public string Query
    {
        get => _query;
        set
        {
            serialized = false;
            _flags[1] = true;
            _query = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _id = (InputDocument)factory.Read(buff.ReadInt32(true), ref buff);
        if (_flags[0])
        {
            _ttlSeconds = buff.ReadInt32(true);
        }

        if (_flags[1])
        {
            _query = buff.ReadTLString();
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}