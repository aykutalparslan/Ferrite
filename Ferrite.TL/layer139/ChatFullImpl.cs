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
public class ChatFullImpl : ChatFull
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public ChatFullImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -779165146;
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
            writer.WriteTLString(_about);
            writer.Write(_participants.TLBytes, false);
            if (_flags[2])
            {
                writer.Write(_chatPhoto.TLBytes, false);
            }

            writer.Write(_notifySettings.TLBytes, false);
            if (_flags[13])
            {
                writer.Write(_exportedInvite.TLBytes, false);
            }

            if (_flags[3])
            {
                writer.Write(_botInfo.TLBytes, false);
            }

            if (_flags[6])
            {
                writer.WriteInt32(_pinnedMsgId, true);
            }

            if (_flags[11])
            {
                writer.WriteInt32(_folderId, true);
            }

            if (_flags[12])
            {
                writer.Write(_call.TLBytes, false);
            }

            if (_flags[14])
            {
                writer.WriteInt32(_ttlPeriod, true);
            }

            if (_flags[15])
            {
                writer.Write(_groupcallDefaultJoinAs.TLBytes, false);
            }

            if (_flags[16])
            {
                writer.WriteTLString(_themeEmoticon);
            }

            if (_flags[17])
            {
                writer.WriteInt32(_requestsPending, true);
            }

            if (_flags[17])
            {
                writer.Write(_recentRequesters.TLBytes, false);
            }

            if (_flags[18])
            {
                writer.Write(_availableReactions.TLBytes, false);
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

    public bool CanSetUsername
    {
        get => _flags[7];
        set
        {
            serialized = false;
            _flags[7] = value;
        }
    }

    public bool HasScheduled
    {
        get => _flags[8];
        set
        {
            serialized = false;
            _flags[8] = value;
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

    private ChatParticipants _participants;
    public ChatParticipants Participants
    {
        get => _participants;
        set
        {
            serialized = false;
            _participants = value;
        }
    }

    private Photo _chatPhoto;
    public Photo ChatPhoto
    {
        get => _chatPhoto;
        set
        {
            serialized = false;
            _flags[2] = true;
            _chatPhoto = value;
        }
    }

    private PeerNotifySettings _notifySettings;
    public PeerNotifySettings NotifySettings
    {
        get => _notifySettings;
        set
        {
            serialized = false;
            _notifySettings = value;
        }
    }

    private ExportedChatInvite _exportedInvite;
    public ExportedChatInvite ExportedInvite
    {
        get => _exportedInvite;
        set
        {
            serialized = false;
            _flags[13] = true;
            _exportedInvite = value;
        }
    }

    private Vector<BotInfo> _botInfo;
    public Vector<BotInfo> BotInfo
    {
        get => _botInfo;
        set
        {
            serialized = false;
            _flags[3] = true;
            _botInfo = value;
        }
    }

    private int _pinnedMsgId;
    public int PinnedMsgId
    {
        get => _pinnedMsgId;
        set
        {
            serialized = false;
            _flags[6] = true;
            _pinnedMsgId = value;
        }
    }

    private int _folderId;
    public int FolderId
    {
        get => _folderId;
        set
        {
            serialized = false;
            _flags[11] = true;
            _folderId = value;
        }
    }

    private InputGroupCall _call;
    public InputGroupCall Call
    {
        get => _call;
        set
        {
            serialized = false;
            _flags[12] = true;
            _call = value;
        }
    }

    private int _ttlPeriod;
    public int TtlPeriod
    {
        get => _ttlPeriod;
        set
        {
            serialized = false;
            _flags[14] = true;
            _ttlPeriod = value;
        }
    }

    private Peer _groupcallDefaultJoinAs;
    public Peer GroupcallDefaultJoinAs
    {
        get => _groupcallDefaultJoinAs;
        set
        {
            serialized = false;
            _flags[15] = true;
            _groupcallDefaultJoinAs = value;
        }
    }

    private string _themeEmoticon;
    public string ThemeEmoticon
    {
        get => _themeEmoticon;
        set
        {
            serialized = false;
            _flags[16] = true;
            _themeEmoticon = value;
        }
    }

    private int _requestsPending;
    public int RequestsPending
    {
        get => _requestsPending;
        set
        {
            serialized = false;
            _flags[17] = true;
            _requestsPending = value;
        }
    }

    private VectorOfLong _recentRequesters;
    public VectorOfLong RecentRequesters
    {
        get => _recentRequesters;
        set
        {
            serialized = false;
            _flags[17] = true;
            _recentRequesters = value;
        }
    }

    private VectorOfString _availableReactions;
    public VectorOfString AvailableReactions
    {
        get => _availableReactions;
        set
        {
            serialized = false;
            _flags[18] = true;
            _availableReactions = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _id = buff.ReadInt64(true);
        _about = buff.ReadTLString();
        _participants = (ChatParticipants)factory.Read(buff.ReadInt32(true), ref buff);
        if (_flags[2])
        {
            _chatPhoto = (Photo)factory.Read(buff.ReadInt32(true), ref buff);
        }

        _notifySettings = (PeerNotifySettings)factory.Read(buff.ReadInt32(true), ref buff);
        if (_flags[13])
        {
            _exportedInvite = (ExportedChatInvite)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[3])
        {
            buff.Skip(4);
            _botInfo = factory.Read<Vector<BotInfo>>(ref buff);
        }

        if (_flags[6])
        {
            _pinnedMsgId = buff.ReadInt32(true);
        }

        if (_flags[11])
        {
            _folderId = buff.ReadInt32(true);
        }

        if (_flags[12])
        {
            _call = (InputGroupCall)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[14])
        {
            _ttlPeriod = buff.ReadInt32(true);
        }

        if (_flags[15])
        {
            _groupcallDefaultJoinAs = (Peer)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[16])
        {
            _themeEmoticon = buff.ReadTLString();
        }

        if (_flags[17])
        {
            _requestsPending = buff.ReadInt32(true);
        }

        if (_flags[17])
        {
            buff.Skip(4);
            _recentRequesters = factory.Read<VectorOfLong>(ref buff);
        }

        if (_flags[18])
        {
            buff.Skip(4);
            _availableReactions = factory.Read<VectorOfString>(ref buff);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}