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
public class MessageFwdHeaderImpl : MessageFwdHeader
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public MessageFwdHeaderImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 1601666510;
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
                writer.Write(_fromId.TLBytes, false);
            }

            if (_flags[5])
            {
                writer.WriteTLString(_fromName);
            }

            writer.WriteInt32(_date, true);
            if (_flags[2])
            {
                writer.WriteInt32(_channelPost, true);
            }

            if (_flags[3])
            {
                writer.WriteTLString(_postAuthor);
            }

            if (_flags[4])
            {
                writer.Write(_savedFromPeer.TLBytes, false);
            }

            if (_flags[4])
            {
                writer.WriteInt32(_savedFromMsgId, true);
            }

            if (_flags[6])
            {
                writer.WriteTLString(_psaType);
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

    public bool Imported
    {
        get => _flags[7];
        set
        {
            serialized = false;
            _flags[7] = value;
        }
    }

    private Peer _fromId;
    public Peer FromId
    {
        get => _fromId;
        set
        {
            serialized = false;
            _flags[0] = true;
            _fromId = value;
        }
    }

    private string _fromName;
    public string FromName
    {
        get => _fromName;
        set
        {
            serialized = false;
            _flags[5] = true;
            _fromName = value;
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

    private string _postAuthor;
    public string PostAuthor
    {
        get => _postAuthor;
        set
        {
            serialized = false;
            _flags[3] = true;
            _postAuthor = value;
        }
    }

    private Peer _savedFromPeer;
    public Peer SavedFromPeer
    {
        get => _savedFromPeer;
        set
        {
            serialized = false;
            _flags[4] = true;
            _savedFromPeer = value;
        }
    }

    private int _savedFromMsgId;
    public int SavedFromMsgId
    {
        get => _savedFromMsgId;
        set
        {
            serialized = false;
            _flags[4] = true;
            _savedFromMsgId = value;
        }
    }

    private string _psaType;
    public string PsaType
    {
        get => _psaType;
        set
        {
            serialized = false;
            _flags[6] = true;
            _psaType = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        if (_flags[0])
        {
            _fromId = (Peer)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[5])
        {
            _fromName = buff.ReadTLString();
        }

        _date = buff.ReadInt32(true);
        if (_flags[2])
        {
            _channelPost = buff.ReadInt32(true);
        }

        if (_flags[3])
        {
            _postAuthor = buff.ReadTLString();
        }

        if (_flags[4])
        {
            _savedFromPeer = (Peer)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[4])
        {
            _savedFromMsgId = buff.ReadInt32(true);
        }

        if (_flags[6])
        {
            _psaType = buff.ReadTLString();
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}