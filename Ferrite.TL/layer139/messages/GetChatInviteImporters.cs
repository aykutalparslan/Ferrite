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

namespace Ferrite.TL.layer139.messages;
public class GetChatInviteImporters : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public GetChatInviteImporters(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -553329330;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.Write(_peer.TLBytes, false);
            if (_flags[1])
            {
                writer.WriteTLString(_link);
            }

            if (_flags[2])
            {
                writer.WriteTLString(_q);
            }

            writer.WriteInt32(_offsetDate, true);
            writer.Write(_offsetUser.TLBytes, false);
            writer.WriteInt32(_limit, true);
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

    public bool Requested
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    private InputPeer _peer;
    public InputPeer Peer
    {
        get => _peer;
        set
        {
            serialized = false;
            _peer = value;
        }
    }

    private string _link;
    public string Link
    {
        get => _link;
        set
        {
            serialized = false;
            _flags[1] = true;
            _link = value;
        }
    }

    private string _q;
    public string Q
    {
        get => _q;
        set
        {
            serialized = false;
            _flags[2] = true;
            _q = value;
        }
    }

    private int _offsetDate;
    public int OffsetDate
    {
        get => _offsetDate;
        set
        {
            serialized = false;
            _offsetDate = value;
        }
    }

    private InputUser _offsetUser;
    public InputUser OffsetUser
    {
        get => _offsetUser;
        set
        {
            serialized = false;
            _offsetUser = value;
        }
    }

    private int _limit;
    public int Limit
    {
        get => _limit;
        set
        {
            serialized = false;
            _limit = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _peer = (InputPeer)factory.Read(buff.ReadInt32(true), ref buff);
        if (_flags[1])
        {
            _link = buff.ReadTLString();
        }

        if (_flags[2])
        {
            _q = buff.ReadTLString();
        }

        _offsetDate = buff.ReadInt32(true);
        _offsetUser = (InputUser)factory.Read(buff.ReadInt32(true), ref buff);
        _limit = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}