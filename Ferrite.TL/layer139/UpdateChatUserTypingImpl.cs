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
public class UpdateChatUserTypingImpl : Update
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public UpdateChatUserTypingImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -2092401936;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(_chatId, true);
            writer.Write(_fromId.TLBytes, false);
            writer.Write(_action.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private long _chatId;
    public long ChatId
    {
        get => _chatId;
        set
        {
            serialized = false;
            _chatId = value;
        }
    }

    private Peer _fromId;
    public Peer FromId
    {
        get => _fromId;
        set
        {
            serialized = false;
            _fromId = value;
        }
    }

    private SendMessageAction _action;
    public SendMessageAction Action
    {
        get => _action;
        set
        {
            serialized = false;
            _action = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _chatId = buff.ReadInt64(true);
        buff.Skip(4); _fromId  =  factory . Read < Peer > ( ref  buff ) ; 
        buff.Skip(4); _action  =  factory . Read < SendMessageAction > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}