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
public class WebAuthorizationImpl : WebAuthorization
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public WebAuthorizationImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1493633966;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(_hash, true);
            writer.WriteInt64(_botId, true);
            writer.WriteTLString(_domain);
            writer.WriteTLString(_browser);
            writer.WriteTLString(_platform);
            writer.WriteInt32(_dateCreated, true);
            writer.WriteInt32(_dateActive, true);
            writer.WriteTLString(_ip);
            writer.WriteTLString(_region);
            serialized = true;
            return writer.ToReadOnlySequence();
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

    private long _botId;
    public long BotId
    {
        get => _botId;
        set
        {
            serialized = false;
            _botId = value;
        }
    }

    private string _domain;
    public string Domain
    {
        get => _domain;
        set
        {
            serialized = false;
            _domain = value;
        }
    }

    private string _browser;
    public string Browser
    {
        get => _browser;
        set
        {
            serialized = false;
            _browser = value;
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
        _hash = buff.ReadInt64(true);
        _botId = buff.ReadInt64(true);
        _domain = buff.ReadTLString();
        _browser = buff.ReadTLString();
        _platform = buff.ReadTLString();
        _dateCreated = buff.ReadInt32(true);
        _dateActive = buff.ReadInt32(true);
        _ip = buff.ReadTLString();
        _region = buff.ReadTLString();
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}