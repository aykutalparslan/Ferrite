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

namespace Ferrite.TL.layer139.messages;
public class DhConfigImpl : DhConfig
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public DhConfigImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 740433629;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt32(_g, true);
            writer.WriteTLBytes(_p);
            writer.WriteInt32(_version, true);
            writer.WriteTLBytes(_random);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private int _g;
    public int G
    {
        get => _g;
        set
        {
            serialized = false;
            _g = value;
        }
    }

    private byte[] _p;
    public byte[] P
    {
        get => _p;
        set
        {
            serialized = false;
            _p = value;
        }
    }

    private int _version;
    public int Version
    {
        get => _version;
        set
        {
            serialized = false;
            _version = value;
        }
    }

    private byte[] _random;
    public byte[] Random
    {
        get => _random;
        set
        {
            serialized = false;
            _random = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _g = buff.ReadInt32(true);
        _p = buff.ReadTLBytes().ToArray();
        _version = buff.ReadInt32(true);
        _random = buff.ReadTLBytes().ToArray();
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}