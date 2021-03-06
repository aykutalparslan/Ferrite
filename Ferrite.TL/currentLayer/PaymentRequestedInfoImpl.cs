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
public class PaymentRequestedInfoImpl : PaymentRequestedInfo
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PaymentRequestedInfoImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1868808300;
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
                writer.WriteTLString(_name);
            }

            if (_flags[1])
            {
                writer.WriteTLString(_phone);
            }

            if (_flags[2])
            {
                writer.WriteTLString(_email);
            }

            if (_flags[3])
            {
                writer.Write(_shippingAddress.TLBytes, false);
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

    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            serialized = false;
            _flags[0] = true;
            _name = value;
        }
    }

    private string _phone;
    public string Phone
    {
        get => _phone;
        set
        {
            serialized = false;
            _flags[1] = true;
            _phone = value;
        }
    }

    private string _email;
    public string Email
    {
        get => _email;
        set
        {
            serialized = false;
            _flags[2] = true;
            _email = value;
        }
    }

    private PostAddress _shippingAddress;
    public PostAddress ShippingAddress
    {
        get => _shippingAddress;
        set
        {
            serialized = false;
            _flags[3] = true;
            _shippingAddress = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        if (_flags[0])
        {
            _name = buff.ReadTLString();
        }

        if (_flags[1])
        {
            _phone = buff.ReadTLString();
        }

        if (_flags[2])
        {
            _email = buff.ReadTLString();
        }

        if (_flags[3])
        {
            _shippingAddress = (PostAddress)factory.Read(buff.ReadInt32(true), ref buff);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}