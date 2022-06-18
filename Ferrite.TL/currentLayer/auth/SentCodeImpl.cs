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

namespace Ferrite.TL.currentLayer.auth;
public class SentCodeImpl : SentCode
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public SentCodeImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 1577067778;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.Write(_type.TLBytes, false);
            writer.WriteTLString(_phoneCodeHash);
            if (_flags[1])
            {
                writer.Write(_nextType.TLBytes, false);
            }

            if (_flags[2])
            {
                writer.WriteInt32(_timeout, true);
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

    private auth.SentCodeType _type;
    public auth.SentCodeType Type
    {
        get => _type;
        set
        {
            serialized = false;
            _type = value;
        }
    }

    private string _phoneCodeHash;
    public string PhoneCodeHash
    {
        get => _phoneCodeHash;
        set
        {
            serialized = false;
            _phoneCodeHash = value;
        }
    }

    private auth.CodeType _nextType;
    public auth.CodeType NextType
    {
        get => _nextType;
        set
        {
            serialized = false;
            _flags[1] = true;
            _nextType = value;
        }
    }

    private int _timeout;
    public int Timeout
    {
        get => _timeout;
        set
        {
            serialized = false;
            _flags[2] = true;
            _timeout = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _type = (auth.SentCodeType)factory.Read(buff.ReadInt32(true), ref buff);
        _phoneCodeHash = buff.ReadTLString();
        if (_flags[1])
        {
            _nextType = (auth.CodeType)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[2])
        {
            _timeout = buff.ReadInt32(true);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}