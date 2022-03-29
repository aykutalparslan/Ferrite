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
public class DcOptionImpl : DcOption
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public DcOptionImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 414687501;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt32(_id, true);
            writer.WriteTLString(_ipAddress);
            writer.WriteInt32(_port, true);
            if (_flags[10])
            {
                writer.WriteTLBytes(_secret);
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

    public bool Ipv6
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool MediaOnly
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
        }
    }

    public bool TcpoOnly
    {
        get => _flags[2];
        set
        {
            serialized = false;
            _flags[2] = value;
        }
    }

    public bool Cdn
    {
        get => _flags[3];
        set
        {
            serialized = false;
            _flags[3] = value;
        }
    }

    public bool Static
    {
        get => _flags[4];
        set
        {
            serialized = false;
            _flags[4] = value;
        }
    }

    private int _id;
    public int Id
    {
        get => _id;
        set
        {
            serialized = false;
            _id = value;
        }
    }

    private string _ipAddress;
    public string IpAddress
    {
        get => _ipAddress;
        set
        {
            serialized = false;
            _ipAddress = value;
        }
    }

    private int _port;
    public int Port
    {
        get => _port;
        set
        {
            serialized = false;
            _port = value;
        }
    }

    private byte[] _secret;
    public byte[] Secret
    {
        get => _secret;
        set
        {
            serialized = false;
            _flags[10] = true;
            _secret = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _id = buff.ReadInt32(true);
        _ipAddress = buff.ReadTLString();
        _port = buff.ReadInt32(true);
        if (_flags[10])
        {
            _secret = buff.ReadTLBytes().ToArray();
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}