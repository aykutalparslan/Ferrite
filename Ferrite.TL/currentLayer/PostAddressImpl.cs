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
public class PostAddressImpl : PostAddress
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PostAddressImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 512535275;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLString(_streetLine1);
            writer.WriteTLString(_streetLine2);
            writer.WriteTLString(_city);
            writer.WriteTLString(_state);
            writer.WriteTLString(_countryIso2);
            writer.WriteTLString(_postCode);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private string _streetLine1;
    public string StreetLine1
    {
        get => _streetLine1;
        set
        {
            serialized = false;
            _streetLine1 = value;
        }
    }

    private string _streetLine2;
    public string StreetLine2
    {
        get => _streetLine2;
        set
        {
            serialized = false;
            _streetLine2 = value;
        }
    }

    private string _city;
    public string City
    {
        get => _city;
        set
        {
            serialized = false;
            _city = value;
        }
    }

    private string _state;
    public string State
    {
        get => _state;
        set
        {
            serialized = false;
            _state = value;
        }
    }

    private string _countryIso2;
    public string CountryIso2
    {
        get => _countryIso2;
        set
        {
            serialized = false;
            _countryIso2 = value;
        }
    }

    private string _postCode;
    public string PostCode
    {
        get => _postCode;
        set
        {
            serialized = false;
            _postCode = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _streetLine1 = buff.ReadTLString();
        _streetLine2 = buff.ReadTLString();
        _city = buff.ReadTLString();
        _state = buff.ReadTLString();
        _countryIso2 = buff.ReadTLString();
        _postCode = buff.ReadTLString();
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}