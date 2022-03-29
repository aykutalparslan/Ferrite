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
public class ChatAdminWithInvitesImpl : ChatAdminWithInvites
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public ChatAdminWithInvitesImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -219353309;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(_adminId, true);
            writer.WriteInt32(_invitesCount, true);
            writer.WriteInt32(_revokedInvitesCount, true);
            serialized = true;
            return writer.ToReadOnlySequence();
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

    private int _invitesCount;
    public int InvitesCount
    {
        get => _invitesCount;
        set
        {
            serialized = false;
            _invitesCount = value;
        }
    }

    private int _revokedInvitesCount;
    public int RevokedInvitesCount
    {
        get => _revokedInvitesCount;
        set
        {
            serialized = false;
            _revokedInvitesCount = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _adminId = buff.ReadInt64(true);
        _invitesCount = buff.ReadInt32(true);
        _revokedInvitesCount = buff.ReadInt32(true);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}