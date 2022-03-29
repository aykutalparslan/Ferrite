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

namespace Ferrite.TL.layer139.account;
public class AuthorizationsImpl : Authorizations
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public AuthorizationsImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 1275039392;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt32(_authorizationTtlDays, true);
            writer.Write(_authorizations.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private int _authorizationTtlDays;
    public int AuthorizationTtlDays
    {
        get => _authorizationTtlDays;
        set
        {
            serialized = false;
            _authorizationTtlDays = value;
        }
    }

    private Vector<Authorization> _authorizations;
    public Vector<Authorization> Authorizations
    {
        get => _authorizations;
        set
        {
            serialized = false;
            _authorizations = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _authorizationTtlDays = buff.ReadInt32(true);
        buff.Skip(4); _authorizations  =  factory . Read < Vector < Authorization > > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}