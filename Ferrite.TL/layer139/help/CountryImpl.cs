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
public class CountryImpl : Country
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public CountryImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1014526429;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteTLString(_iso2);
            writer.WriteTLString(_defaultName);
            if (_flags[1])
            {
                writer.WriteTLString(_name);
            }

            writer.Write(_countryCodes.TLBytes, false);
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

    public bool Hidden
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    private string _iso2;
    public string Iso2
    {
        get => _iso2;
        set
        {
            serialized = false;
            _iso2 = value;
        }
    }

    private string _defaultName;
    public string DefaultName
    {
        get => _defaultName;
        set
        {
            serialized = false;
            _defaultName = value;
        }
    }

    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            serialized = false;
            _flags[1] = true;
            _name = value;
        }
    }

    private Vector<help.CountryCode> _countryCodes;
    public Vector<help.CountryCode> CountryCodes
    {
        get => _countryCodes;
        set
        {
            serialized = false;
            _countryCodes = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _iso2 = buff.ReadTLString();
        _defaultName = buff.ReadTLString();
        if (_flags[1])
        {
            _name = buff.ReadTLString();
        }

        buff.Skip(4); _countryCodes  =  factory . Read < Vector < help . CountryCode > > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}