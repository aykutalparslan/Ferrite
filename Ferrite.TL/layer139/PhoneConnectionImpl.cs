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
public class PhoneConnectionImpl : PhoneConnection
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PhoneConnectionImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1655957568;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(_id, true);
            writer.WriteTLString(_ip);
            writer.WriteTLString(_ipv6);
            writer.WriteInt32(_port, true);
            writer.WriteTLBytes(_peerTag);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private long _id;
    public long Id
    {
        get => _id;
        set
        {
            serialized = false;
            _id = value;
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

    private string _ipv6;
    public string Ipv6
    {
        get => _ipv6;
        set
        {
            serialized = false;
            _ipv6 = value;
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

    private byte[] _peerTag;
    public byte[] PeerTag
    {
        get => _peerTag;
        set
        {
            serialized = false;
            _peerTag = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _id = buff.ReadInt64(true);
        _ip = buff.ReadTLString();
        _ipv6 = buff.ReadTLString();
        _port = buff.ReadInt32(true);
        _peerTag = buff.ReadTLBytes().ToArray();
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}