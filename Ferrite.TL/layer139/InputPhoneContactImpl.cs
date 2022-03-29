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
public class InputPhoneContactImpl : InputContact
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public InputPhoneContactImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -208488460;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(_clientId, true);
            writer.WriteTLString(_phone);
            writer.WriteTLString(_firstName);
            writer.WriteTLString(_lastName);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private long _clientId;
    public long ClientId
    {
        get => _clientId;
        set
        {
            serialized = false;
            _clientId = value;
        }
    }

    private string _phone;
    public string Phone
    {
        get => _phone;
        set
        {
            serialized = false;
            _phone = value;
        }
    }

    private string _firstName;
    public string FirstName
    {
        get => _firstName;
        set
        {
            serialized = false;
            _firstName = value;
        }
    }

    private string _lastName;
    public string LastName
    {
        get => _lastName;
        set
        {
            serialized = false;
            _lastName = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _clientId = buff.ReadInt64(true);
        _phone = buff.ReadTLString();
        _firstName = buff.ReadTLString();
        _lastName = buff.ReadTLString();
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}