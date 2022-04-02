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
public class PasswordInputSettingsImpl : PasswordInputSettings
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PasswordInputSettingsImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1036572727;
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
                writer.Write(_newAlgo.TLBytes, false);
            }

            if (_flags[0])
            {
                writer.WriteTLBytes(_newPasswordHash);
            }

            if (_flags[0])
            {
                writer.WriteTLString(_hint);
            }

            if (_flags[1])
            {
                writer.WriteTLString(_email);
            }

            if (_flags[2])
            {
                writer.Write(_newSecureSettings.TLBytes, false);
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

    private PasswordKdfAlgo _newAlgo;
    public PasswordKdfAlgo NewAlgo
    {
        get => _newAlgo;
        set
        {
            serialized = false;
            _flags[0] = true;
            _newAlgo = value;
        }
    }

    private byte[] _newPasswordHash;
    public byte[] NewPasswordHash
    {
        get => _newPasswordHash;
        set
        {
            serialized = false;
            _flags[0] = true;
            _newPasswordHash = value;
        }
    }

    private string _hint;
    public string Hint
    {
        get => _hint;
        set
        {
            serialized = false;
            _flags[0] = true;
            _hint = value;
        }
    }

    private string _email;
    public string Email
    {
        get => _email;
        set
        {
            serialized = false;
            _flags[1] = true;
            _email = value;
        }
    }

    private SecureSecretSettings _newSecureSettings;
    public SecureSecretSettings NewSecureSettings
    {
        get => _newSecureSettings;
        set
        {
            serialized = false;
            _flags[2] = true;
            _newSecureSettings = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        if (_flags[0])
        {
            _newAlgo = (PasswordKdfAlgo)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[0])
        {
            _newPasswordHash = buff.ReadTLBytes().ToArray();
        }

        if (_flags[0])
        {
            _hint = buff.ReadTLString();
        }

        if (_flags[1])
        {
            _email = buff.ReadTLString();
        }

        if (_flags[2])
        {
            _newSecureSettings = (SecureSecretSettings)factory.Read(buff.ReadInt32(true), ref buff);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}