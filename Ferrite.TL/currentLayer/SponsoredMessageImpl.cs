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
public class SponsoredMessageImpl : SponsoredMessage
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public SponsoredMessageImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 981691896;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteTLBytes(_randomId);
            if (_flags[3])
            {
                writer.Write(_fromId.TLBytes, false);
            }

            if (_flags[4])
            {
                writer.Write(_chatInvite.TLBytes, false);
            }

            if (_flags[4])
            {
                writer.WriteTLString(_chatInviteHash);
            }

            if (_flags[2])
            {
                writer.WriteInt32(_channelPost, true);
            }

            if (_flags[0])
            {
                writer.WriteTLString(_startParam);
            }

            writer.WriteTLString(_message);
            if (_flags[1])
            {
                writer.Write(_entities.TLBytes, false);
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

    private byte[] _randomId;
    public byte[] RandomId
    {
        get => _randomId;
        set
        {
            serialized = false;
            _randomId = value;
        }
    }

    private Peer _fromId;
    public Peer FromId
    {
        get => _fromId;
        set
        {
            serialized = false;
            _flags[3] = true;
            _fromId = value;
        }
    }

    private ChatInvite _chatInvite;
    public ChatInvite ChatInvite
    {
        get => _chatInvite;
        set
        {
            serialized = false;
            _flags[4] = true;
            _chatInvite = value;
        }
    }

    private string _chatInviteHash;
    public string ChatInviteHash
    {
        get => _chatInviteHash;
        set
        {
            serialized = false;
            _flags[4] = true;
            _chatInviteHash = value;
        }
    }

    private int _channelPost;
    public int ChannelPost
    {
        get => _channelPost;
        set
        {
            serialized = false;
            _flags[2] = true;
            _channelPost = value;
        }
    }

    private string _startParam;
    public string StartParam
    {
        get => _startParam;
        set
        {
            serialized = false;
            _flags[0] = true;
            _startParam = value;
        }
    }

    private string _message;
    public string Message
    {
        get => _message;
        set
        {
            serialized = false;
            _message = value;
        }
    }

    private Vector<MessageEntity> _entities;
    public Vector<MessageEntity> Entities
    {
        get => _entities;
        set
        {
            serialized = false;
            _flags[1] = true;
            _entities = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _randomId = buff.ReadTLBytes().ToArray();
        if (_flags[3])
        {
            _fromId = (Peer)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[4])
        {
            _chatInvite = (ChatInvite)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[4])
        {
            _chatInviteHash = buff.ReadTLString();
        }

        if (_flags[2])
        {
            _channelPost = buff.ReadInt32(true);
        }

        if (_flags[0])
        {
            _startParam = buff.ReadTLString();
        }

        _message = buff.ReadTLString();
        if (_flags[1])
        {
            buff.Skip(4);
            _entities = factory.Read<Vector<MessageEntity>>(ref buff);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}