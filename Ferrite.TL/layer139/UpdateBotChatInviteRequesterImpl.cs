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
public class UpdateBotChatInviteRequesterImpl : Update
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public UpdateBotChatInviteRequesterImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 299870598;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_peer.TLBytes, false);
            writer.WriteInt32(_date, true);
            writer.WriteInt64(_userId, true);
            writer.WriteTLString(_about);
            writer.Write(_invite.TLBytes, false);
            writer.WriteInt32(_qts, true);
            serialized = true;
            return writer.ToReadOnlySequence();
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

    private long _userId;
    public long UserId
    {
        get => _userId;
        set
        {
            serialized = false;
            _userId = value;
        }
    }

    private string _about;
    public string About
    {
        get => _about;
        set
        {
            serialized = false;
            _about = value;
        }
    }

    private ExportedChatInvite _invite;
    public ExportedChatInvite Invite
    {
        get => _invite;
        set
        {
            serialized = false;
            _invite = value;
        }
    }

    private int _qts;
    public int Qts
    {
        get => _qts;
        set
        {
            serialized = false;
            _qts = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _peer = (Peer)factory.Read(buff.ReadInt32(true), ref buff);
        _date = buff.ReadInt32(true);
        _userId = buff.ReadInt64(true);
        _about = buff.ReadTLString();
        _invite = (ExportedChatInvite)factory.Read(buff.ReadInt32(true), ref buff);
        _qts = buff.ReadInt32(true);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}