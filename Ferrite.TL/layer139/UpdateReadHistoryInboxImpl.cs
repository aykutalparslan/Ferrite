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
public class UpdateReadHistoryInboxImpl : Update
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public UpdateReadHistoryInboxImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1667805217;
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
                writer.WriteInt32(_folderId, true);
            }

            writer.Write(_peer.TLBytes, false);
            writer.WriteInt32(_maxId, true);
            writer.WriteInt32(_stillUnreadCount, true);
            writer.WriteInt32(_pts, true);
            writer.WriteInt32(_ptsCount, true);
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

    private int _folderId;
    public int FolderId
    {
        get => _folderId;
        set
        {
            serialized = false;
            _flags[0] = true;
            _folderId = value;
        }
    }

    private Peer _peer;
    public Peer Peer
    {
        get => _peer;
        set
        {
            serialized = false;
            _peer = value;
        }
    }

    private int _maxId;
    public int MaxId
    {
        get => _maxId;
        set
        {
            serialized = false;
            _maxId = value;
        }
    }

    private int _stillUnreadCount;
    public int StillUnreadCount
    {
        get => _stillUnreadCount;
        set
        {
            serialized = false;
            _stillUnreadCount = value;
        }
    }

    private int _pts;
    public int Pts
    {
        get => _pts;
        set
        {
            serialized = false;
            _pts = value;
        }
    }

    private int _ptsCount;
    public int PtsCount
    {
        get => _ptsCount;
        set
        {
            serialized = false;
            _ptsCount = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        if (_flags[0])
        {
            _folderId = buff.ReadInt32(true);
        }

        buff.Skip(4); _peer  =  factory . Read < Peer > ( ref  buff ) ; 
        _maxId = buff.ReadInt32(true);
        _stillUnreadCount = buff.ReadInt32(true);
        _pts = buff.ReadInt32(true);
        _ptsCount = buff.ReadInt32(true);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}