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
public class AuthorizationImpl : Authorization
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public AuthorizationImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1392388579;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt64(_hash, true);
            writer.WriteTLString(_deviceModel);
            writer.WriteTLString(_platform);
            writer.WriteTLString(_systemVersion);
            writer.WriteInt32(_apiId, true);
            writer.WriteTLString(_appName);
            writer.WriteTLString(_appVersion);
            writer.WriteInt32(_dateCreated, true);
            writer.WriteInt32(_dateActive, true);
            writer.WriteTLString(_ip);
            writer.WriteTLString(_country);
            writer.WriteTLString(_region);
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

    public bool Current
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool OfficialApp
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
        }
    }

    public bool PasswordPending
    {
        get => _flags[2];
        set
        {
            serialized = false;
            _flags[2] = value;
        }
    }

    public bool EncryptedRequestsDisabled
    {
        get => _flags[3];
        set
        {
            serialized = false;
            _flags[3] = value;
        }
    }

    public bool CallRequestsDisabled
    {
        get => _flags[4];
        set
        {
            serialized = false;
            _flags[4] = value;
        }
    }

    private long _hash;
    public long Hash
    {
        get => _hash;
        set
        {
            serialized = false;
            _hash = value;
        }
    }

    private string _deviceModel;
    public string DeviceModel
    {
        get => _deviceModel;
        set
        {
            serialized = false;
            _deviceModel = value;
        }
    }

    private string _platform;
    public string Platform
    {
        get => _platform;
        set
        {
            serialized = false;
            _platform = value;
        }
    }

    private string _systemVersion;
    public string SystemVersion
    {
        get => _systemVersion;
        set
        {
            serialized = false;
            _systemVersion = value;
        }
    }

    private int _apiId;
    public int ApiId
    {
        get => _apiId;
        set
        {
            serialized = false;
            _apiId = value;
        }
    }

    private string _appName;
    public string AppName
    {
        get => _appName;
        set
        {
            serialized = false;
            _appName = value;
        }
    }

    private string _appVersion;
    public string AppVersion
    {
        get => _appVersion;
        set
        {
            serialized = false;
            _appVersion = value;
        }
    }

    private int _dateCreated;
    public int DateCreated
    {
        get => _dateCreated;
        set
        {
            serialized = false;
            _dateCreated = value;
        }
    }

    private int _dateActive;
    public int DateActive
    {
        get => _dateActive;
        set
        {
            serialized = false;
            _dateActive = value;
        }
    }

    private string _ip;
    public string Ip
    {
        get => _ip;
        set
        {
            serialized = false;
            _ip = value;
        }
    }

    private string _country;
    public string Country
    {
        get => _country;
        set
        {
            serialized = false;
            _country = value;
        }
    }

    private string _region;
    public string Region
    {
        get => _region;
        set
        {
            serialized = false;
            _region = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _hash = buff.ReadInt64(true);
        _deviceModel = buff.ReadTLString();
        _platform = buff.ReadTLString();
        _systemVersion = buff.ReadTLString();
        _apiId = buff.ReadInt32(true);
        _appName = buff.ReadTLString();
        _appVersion = buff.ReadTLString();
        _dateCreated = buff.ReadInt32(true);
        _dateActive = buff.ReadInt32(true);
        _ip = buff.ReadTLString();
        _country = buff.ReadTLString();
        _region = buff.ReadTLString();
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}