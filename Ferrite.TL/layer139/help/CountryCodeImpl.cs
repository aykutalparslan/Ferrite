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

namespace Ferrite.TL.layer139.help;
public class CountryCodeImpl : CountryCode
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public CountryCodeImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 1107543535;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteTLString(_countryCode);
            if (_flags[0])
            {
                writer.Write(_prefixes.TLBytes, false);
            }

            if (_flags[1])
            {
                writer.Write(_patterns.TLBytes, false);
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

    private string _countryCode;
    public string CountryCode
    {
        get => _countryCode;
        set
        {
            serialized = false;
            _countryCode = value;
        }
    }

    private VectorOfString _prefixes;
    public VectorOfString Prefixes
    {
        get => _prefixes;
        set
        {
            serialized = false;
            _flags[0] = true;
            _prefixes = value;
        }
    }

    private VectorOfString _patterns;
    public VectorOfString Patterns
    {
        get => _patterns;
        set
        {
            serialized = false;
            _flags[1] = true;
            _patterns = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _countryCode = buff.ReadTLString();
        if (_flags[0])
        {
            buff.Skip(4);
            _prefixes = factory.Read<VectorOfString>(ref buff);
        }

        if (_flags[1])
        {
            buff.Skip(4);
            _patterns = factory.Read<VectorOfString>(ref buff);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}