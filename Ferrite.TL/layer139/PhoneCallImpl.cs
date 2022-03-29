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
public class PhoneCallImpl : PhoneCall
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PhoneCallImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1770029977;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt64(_id, true);
            writer.WriteInt64(_accessHash, true);
            writer.WriteInt32(_date, true);
            writer.WriteInt64(_adminId, true);
            writer.WriteInt64(_participantId, true);
            writer.WriteTLBytes(_gAOrB);
            writer.WriteInt64(_keyFingerprint, true);
            writer.Write(_protocol.TLBytes, false);
            writer.Write(_connections.TLBytes, false);
            writer.WriteInt32(_startDate, true);
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

    public bool P2pAllowed
    {
        get => _flags[5];
        set
        {
            serialized = false;
            _flags[5] = value;
        }
    }

    public bool Video
    {
        get => _flags[6];
        set
        {
            serialized = false;
            _flags[6] = value;
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

    private long _accessHash;
    public long AccessHash
    {
        get => _accessHash;
        set
        {
            serialized = false;
            _accessHash = value;
        }
    }

    private int _date;
    public int Date
    {
        get => _date;
        set
        {
            serialized = false;
            _date = value;
        }
    }

    private long _adminId;
    public long AdminId
    {
        get => _adminId;
        set
        {
            serialized = false;
            _adminId = value;
        }
    }

    private long _participantId;
    public long ParticipantId
    {
        get => _participantId;
        set
        {
            serialized = false;
            _participantId = value;
        }
    }

    private byte[] _gAOrB;
    public byte[] GAOrB
    {
        get => _gAOrB;
        set
        {
            serialized = false;
            _gAOrB = value;
        }
    }

    private long _keyFingerprint;
    public long KeyFingerprint
    {
        get => _keyFingerprint;
        set
        {
            serialized = false;
            _keyFingerprint = value;
        }
    }

    private PhoneCallProtocol _protocol;
    public PhoneCallProtocol Protocol
    {
        get => _protocol;
        set
        {
            serialized = false;
            _protocol = value;
        }
    }

    private Vector<PhoneConnection> _connections;
    public Vector<PhoneConnection> Connections
    {
        get => _connections;
        set
        {
            serialized = false;
            _connections = value;
        }
    }

    private int _startDate;
    public int StartDate
    {
        get => _startDate;
        set
        {
            serialized = false;
            _startDate = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _id = buff.ReadInt64(true);
        _accessHash = buff.ReadInt64(true);
        _date = buff.ReadInt32(true);
        _adminId = buff.ReadInt64(true);
        _participantId = buff.ReadInt64(true);
        _gAOrB = buff.ReadTLBytes().ToArray();
        _keyFingerprint = buff.ReadInt64(true);
        buff.Skip(4); _protocol  =  factory . Read < PhoneCallProtocol > ( ref  buff ) ; 
        buff.Skip(4); _connections  =  factory . Read < Vector < PhoneConnection > > ( ref  buff ) ; 
        _startDate = buff.ReadInt32(true);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}