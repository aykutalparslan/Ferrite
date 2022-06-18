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
public class PasswordImpl : Password
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PasswordImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 408623183;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            if (_flags[2])
            {
                writer.Write(_currentAlgo.TLBytes, false);
            }

            if (_flags[2])
            {
                writer.WriteTLBytes(_srpB);
            }

            if (_flags[2])
            {
                writer.WriteInt64(_srpId, true);
            }

            if (_flags[3])
            {
                writer.WriteTLString(_hint);
            }

            if (_flags[4])
            {
                writer.WriteTLString(_emailUnconfirmedPattern);
            }

            writer.Write(_newAlgo.TLBytes, false);
            writer.Write(_newSecureAlgo.TLBytes, false);
            writer.WriteTLBytes(_secureRandom);
            if (_flags[5])
            {
                writer.WriteInt32(_pendingResetDate, true);
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

    public bool HasRecovery
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool HasSecureValues
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
        }
    }

    public bool HasPassword
    {
        get => _flags[2];
        set
        {
            serialized = false;
            _flags[2] = value;
        }
    }

    private PasswordKdfAlgo _currentAlgo;
    public PasswordKdfAlgo CurrentAlgo
    {
        get => _currentAlgo;
        set
        {
            serialized = false;
            _flags[2] = true;
            _currentAlgo = value;
        }
    }

    private byte[] _srpB;
    public byte[] SrpB
    {
        get => _srpB;
        set
        {
            serialized = false;
            _flags[2] = true;
            _srpB = value;
        }
    }

    private long _srpId;
    public long SrpId
    {
        get => _srpId;
        set
        {
            serialized = false;
            _flags[2] = true;
            _srpId = value;
        }
    }

    private string _hint;
    public string Hint
    {
        get => _hint;
        set
        {
            serialized = false;
            _flags[3] = true;
            _hint = value;
        }
    }

    private string _emailUnconfirmedPattern;
    public string EmailUnconfirmedPattern
    {
        get => _emailUnconfirmedPattern;
        set
        {
            serialized = false;
            _flags[4] = true;
            _emailUnconfirmedPattern = value;
        }
    }

    private PasswordKdfAlgo _newAlgo;
    public PasswordKdfAlgo NewAlgo
    {
        get => _newAlgo;
        set
        {
            serialized = false;
            _newAlgo = value;
        }
    }

    private SecurePasswordKdfAlgo _newSecureAlgo;
    public SecurePasswordKdfAlgo NewSecureAlgo
    {
        get => _newSecureAlgo;
        set
        {
            serialized = false;
            _newSecureAlgo = value;
        }
    }

    private byte[] _secureRandom;
    public byte[] SecureRandom
    {
        get => _secureRandom;
        set
        {
            serialized = false;
            _secureRandom = value;
        }
    }

    private int _pendingResetDate;
    public int PendingResetDate
    {
        get => _pendingResetDate;
        set
        {
            serialized = false;
            _flags[5] = true;
            _pendingResetDate = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        if (_flags[2])
        {
            _currentAlgo = (PasswordKdfAlgo)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[2])
        {
            _srpB = buff.ReadTLBytes().ToArray();
        }

        if (_flags[2])
        {
            _srpId = buff.ReadInt64(true);
        }

        if (_flags[3])
        {
            _hint = buff.ReadTLString();
        }

        if (_flags[4])
        {
            _emailUnconfirmedPattern = buff.ReadTLString();
        }

        _newAlgo = (PasswordKdfAlgo)factory.Read(buff.ReadInt32(true), ref buff);
        _newSecureAlgo = (SecurePasswordKdfAlgo)factory.Read(buff.ReadInt32(true), ref buff);
        _secureRandom = buff.ReadTLBytes().ToArray();
        if (_flags[5])
        {
            _pendingResetDate = buff.ReadInt32(true);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}