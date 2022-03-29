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
public class ChannelAdminLogEventActionParticipantToggleBanImpl : ChannelAdminLogEventAction
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public ChannelAdminLogEventActionParticipantToggleBanImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -422036098;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_prevParticipant.TLBytes, false);
            writer.Write(_newParticipant.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private ChannelParticipant _prevParticipant;
    public ChannelParticipant PrevParticipant
    {
        get => _prevParticipant;
        set
        {
            serialized = false;
            _prevParticipant = value;
        }
    }

    private ChannelParticipant _newParticipant;
    public ChannelParticipant NewParticipant
    {
        get => _newParticipant;
        set
        {
            serialized = false;
            _newParticipant = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        buff.Skip(4); _prevParticipant  =  factory . Read < ChannelParticipant > ( ref  buff ) ; 
        buff.Skip(4); _newParticipant  =  factory . Read < ChannelParticipant > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}