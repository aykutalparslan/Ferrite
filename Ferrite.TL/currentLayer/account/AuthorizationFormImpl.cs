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

namespace Ferrite.TL.currentLayer.account;
public class AuthorizationFormImpl : AuthorizationForm
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public AuthorizationFormImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1389486888;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.Write(_requiredTypes.TLBytes, false);
            writer.Write(_values.TLBytes, false);
            writer.Write(_errors.TLBytes, false);
            writer.Write(_users.TLBytes, false);
            if (_flags[0])
            {
                writer.WriteTLString(_privacyPolicyUrl);
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

    private Vector<SecureRequiredType> _requiredTypes;
    public Vector<SecureRequiredType> RequiredTypes
    {
        get => _requiredTypes;
        set
        {
            serialized = false;
            _requiredTypes = value;
        }
    }

    private Vector<SecureValue> _values;
    public Vector<SecureValue> Values
    {
        get => _values;
        set
        {
            serialized = false;
            _values = value;
        }
    }

    private Vector<SecureValueError> _errors;
    public Vector<SecureValueError> Errors
    {
        get => _errors;
        set
        {
            serialized = false;
            _errors = value;
        }
    }

    private Vector<User> _users;
    public Vector<User> Users
    {
        get => _users;
        set
        {
            serialized = false;
            _users = value;
        }
    }

    private string _privacyPolicyUrl;
    public string PrivacyPolicyUrl
    {
        get => _privacyPolicyUrl;
        set
        {
            serialized = false;
            _flags[0] = true;
            _privacyPolicyUrl = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        buff.Skip(4); _requiredTypes  =  factory . Read < Vector < SecureRequiredType > > ( ref  buff ) ; 
        buff.Skip(4); _values  =  factory . Read < Vector < SecureValue > > ( ref  buff ) ; 
        buff.Skip(4); _errors  =  factory . Read < Vector < SecureValueError > > ( ref  buff ) ; 
        buff.Skip(4); _users  =  factory . Read < Vector < User > > ( ref  buff ) ; 
        if (_flags[0])
        {
            _privacyPolicyUrl = buff.ReadTLString();
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}