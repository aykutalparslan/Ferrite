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
public class ChannelFullImpl : ChatFull
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public ChannelFullImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -516145888;
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
            if (_flags[0])
            {
                writer.WriteInt32(_participantsCount, true);
            }

            if (_flags[1])
            {
                writer.WriteInt32(_adminsCount, true);
            }

            if (_flags[2])
            {
                writer.WriteInt32(_kickedCount, true);
            }

            if (_flags[2])
            {
                writer.WriteInt32(_bannedCount, true);
            }

            if (_flags[13])
            {
                writer.WriteInt32(_onlineCount, true);
            }

            writer.WriteInt32(_readInboxMaxId, true);
            writer.WriteInt32(_readOutboxMaxId, true);
            writer.WriteInt32(_unreadCount, true);
            writer.Write(_chatPhoto.TLBytes, false);
            writer.Write(_notifySettings.TLBytes, false);
            if (_flags[23])
            {
                writer.Write(_exportedInvite.TLBytes, false);
            }

            writer.Write(_botInfo.TLBytes, false);
            if (_flags[4])
            {
                writer.WriteInt64(_migratedFromChatId, true);
            }

            if (_flags[4])
            {
                writer.WriteInt32(_migratedFromMaxId, true);
            }

            if (_flags[5])
            {
                writer.WriteInt32(_pinnedMsgId, true);
            }

            if (_flags[8])
            {
                writer.Write(_stickerset.TLBytes, false);
            }

            if (_flags[9])
            {
                writer.WriteInt32(_availableMinId, true);
            }

            if (_flags[11])
            {
                writer.WriteInt32(_folderId, true);
            }

            if (_flags[14])
            {
                writer.WriteInt64(_linkedChatId, true);
            }

            if (_flags[15])
            {
                writer.Write(_location.TLBytes, false);
            }

            if (_flags[17])
            {
                writer.WriteInt32(_slowmodeSeconds, true);
            }

            if (_flags[18])
            {
                writer.WriteInt32(_slowmodeNextSendDate, true);
            }

            if (_flags[12])
            {
                writer.WriteInt32(_statsDc, true);
            }

            writer.WriteInt32(_pts, true);
            if (_flags[21])
            {
                writer.Write(_call.TLBytes, false);
            }

            if (_flags[24])
            {
                writer.WriteInt32(_ttlPeriod, true);
            }

            if (_flags[25])
            {
                writer.Write(_pendingSuggestions.TLBytes, false);
            }

            if (_flags[26])
            {
                writer.Write(_groupcallDefaultJoinAs.TLBytes, false);
            }

            if (_flags[27])
            {
                writer.WriteTLString(_themeEmoticon);
            }

            if (_flags[28])
            {
                writer.WriteInt32(_requestsPending, true);
            }

            if (_flags[28])
            {
                writer.Write(_recentRequesters.TLBytes, false);
            }

            if (_flags[29])
            {
                writer.Write(_defaultSendAs.TLBytes, false);
            }

            if (_flags[30])
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

    public bool CanViewParticipants
    {
        get => _flags[3];
        set
        {
            serialized = false;
            _flags[3] = value;
        }
    }

    public bool CanSetUsername
    {
        get => _flags[6];
        set
        {
            serialized = false;
            _flags[6] = value;
        }
    }

    public bool CanSetStickers
    {
        get => _flags[7];
        set
        {
            serialized = false;
            _flags[7] = value;
        }
    }

    public bool HiddenPrehistory
    {
        get => _flags[10];
        set
        {
            serialized = false;
            _flags[10] = value;
        }
    }

    public bool CanSetLocation
    {
        get => _flags[16];
        set
        {
            serialized = false;
            _flags[16] = value;
        }
    }

    public bool HasScheduled
    {
        get => _flags[19];
        set
        {
            serialized = false;
            _flags[19] = value;
        }
    }

    public bool CanViewStats
    {
        get => _flags[20];
        set
        {
            serialized = false;
            _flags[20] = value;
        }
    }

    public bool Blocked
    {
        get => _flags[22];
        set
        {
            serialized = false;
            _flags[22] = value;
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

    private int _participantsCount;
    public int ParticipantsCount
    {
        get => _participantsCount;
        set
        {
            serialized = false;
            _flags[0] = true;
            _participantsCount = value;
        }
    }

    private int _adminsCount;
    public int AdminsCount
    {
        get => _adminsCount;
        set
        {
            serialized = false;
            _flags[1] = true;
            _adminsCount = value;
        }
    }

    private int _kickedCount;
    public int KickedCount
    {
        get => _kickedCount;
        set
        {
            serialized = false;
            _flags[2] = true;
            _kickedCount = value;
        }
    }

    private int _bannedCount;
    public int BannedCount
    {
        get => _bannedCount;
        set
        {
            serialized = false;
            _flags[2] = true;
            _bannedCount = value;
        }
    }

    private int _onlineCount;
    public int OnlineCount
    {
        get => _onlineCount;
        set
        {
            serialized = false;
            _flags[13] = true;
            _onlineCount = value;
        }
    }

    private int _readInboxMaxId;
    public int ReadInboxMaxId
    {
        get => _readInboxMaxId;
        set
        {
            serialized = false;
            _readInboxMaxId = value;
        }
    }

    private int _readOutboxMaxId;
    public int ReadOutboxMaxId
    {
        get => _readOutboxMaxId;
        set
        {
            serialized = false;
            _readOutboxMaxId = value;
        }
    }

    private int _unreadCount;
    public int UnreadCount
    {
        get => _unreadCount;
        set
        {
            serialized = false;
            _unreadCount = value;
        }
    }

    private Photo _chatPhoto;
    public Photo ChatPhoto
    {
        get => _chatPhoto;
        set
        {
            serialized = false;
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
            _flags[23] = true;
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
            _botInfo = value;
        }
    }

    private long _migratedFromChatId;
    public long MigratedFromChatId
    {
        get => _migratedFromChatId;
        set
        {
            serialized = false;
            _flags[4] = true;
            _migratedFromChatId = value;
        }
    }

    private int _migratedFromMaxId;
    public int MigratedFromMaxId
    {
        get => _migratedFromMaxId;
        set
        {
            serialized = false;
            _flags[4] = true;
            _migratedFromMaxId = value;
        }
    }

    private int _pinnedMsgId;
    public int PinnedMsgId
    {
        get => _pinnedMsgId;
        set
        {
            serialized = false;
            _flags[5] = true;
            _pinnedMsgId = value;
        }
    }

    private StickerSet _stickerset;
    public StickerSet Stickerset
    {
        get => _stickerset;
        set
        {
            serialized = false;
            _flags[8] = true;
            _stickerset = value;
        }
    }

    private int _availableMinId;
    public int AvailableMinId
    {
        get => _availableMinId;
        set
        {
            serialized = false;
            _flags[9] = true;
            _availableMinId = value;
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

    private long _linkedChatId;
    public long LinkedChatId
    {
        get => _linkedChatId;
        set
        {
            serialized = false;
            _flags[14] = true;
            _linkedChatId = value;
        }
    }

    private ChannelLocation _location;
    public ChannelLocation Location
    {
        get => _location;
        set
        {
            serialized = false;
            _flags[15] = true;
            _location = value;
        }
    }

    private int _slowmodeSeconds;
    public int SlowmodeSeconds
    {
        get => _slowmodeSeconds;
        set
        {
            serialized = false;
            _flags[17] = true;
            _slowmodeSeconds = value;
        }
    }

    private int _slowmodeNextSendDate;
    public int SlowmodeNextSendDate
    {
        get => _slowmodeNextSendDate;
        set
        {
            serialized = false;
            _flags[18] = true;
            _slowmodeNextSendDate = value;
        }
    }

    private int _statsDc;
    public int StatsDc
    {
        get => _statsDc;
        set
        {
            serialized = false;
            _flags[12] = true;
            _statsDc = value;
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

    private InputGroupCall _call;
    public InputGroupCall Call
    {
        get => _call;
        set
        {
            serialized = false;
            _flags[21] = true;
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
            _flags[24] = true;
            _ttlPeriod = value;
        }
    }

    private VectorOfString _pendingSuggestions;
    public VectorOfString PendingSuggestions
    {
        get => _pendingSuggestions;
        set
        {
            serialized = false;
            _flags[25] = true;
            _pendingSuggestions = value;
        }
    }

    private Peer _groupcallDefaultJoinAs;
    public Peer GroupcallDefaultJoinAs
    {
        get => _groupcallDefaultJoinAs;
        set
        {
            serialized = false;
            _flags[26] = true;
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
            _flags[27] = true;
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
            _flags[28] = true;
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
            _flags[28] = true;
            _recentRequesters = value;
        }
    }

    private Peer _defaultSendAs;
    public Peer DefaultSendAs
    {
        get => _defaultSendAs;
        set
        {
            serialized = false;
            _flags[29] = true;
            _defaultSendAs = value;
        }
    }

    private VectorOfString _availableReactions;
    public VectorOfString AvailableReactions
    {
        get => _availableReactions;
        set
        {
            serialized = false;
            _flags[30] = true;
            _availableReactions = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _id = buff.ReadInt64(true);
        _about = buff.ReadTLString();
        if (_flags[0])
        {
            _participantsCount = buff.ReadInt32(true);
        }

        if (_flags[1])
        {
            _adminsCount = buff.ReadInt32(true);
        }

        if (_flags[2])
        {
            _kickedCount = buff.ReadInt32(true);
        }

        if (_flags[2])
        {
            _bannedCount = buff.ReadInt32(true);
        }

        if (_flags[13])
        {
            _onlineCount = buff.ReadInt32(true);
        }

        _readInboxMaxId = buff.ReadInt32(true);
        _readOutboxMaxId = buff.ReadInt32(true);
        _unreadCount = buff.ReadInt32(true);
        buff.Skip(4); _chatPhoto  =  factory . Read < Photo > ( ref  buff ) ; 
        buff.Skip(4); _notifySettings  =  factory . Read < PeerNotifySettings > ( ref  buff ) ; 
        if (_flags[23])
        {
            buff.Skip(4);
            _exportedInvite = factory.Read<ExportedChatInvite>(ref buff);
        }

        buff.Skip(4); _botInfo  =  factory . Read < Vector < BotInfo > > ( ref  buff ) ; 
        if (_flags[4])
        {
            _migratedFromChatId = buff.ReadInt64(true);
        }

        if (_flags[4])
        {
            _migratedFromMaxId = buff.ReadInt32(true);
        }

        if (_flags[5])
        {
            _pinnedMsgId = buff.ReadInt32(true);
        }

        if (_flags[8])
        {
            buff.Skip(4);
            _stickerset = factory.Read<StickerSet>(ref buff);
        }

        if (_flags[9])
        {
            _availableMinId = buff.ReadInt32(true);
        }

        if (_flags[11])
        {
            _folderId = buff.ReadInt32(true);
        }

        if (_flags[14])
        {
            _linkedChatId = buff.ReadInt64(true);
        }

        if (_flags[15])
        {
            buff.Skip(4);
            _location = factory.Read<ChannelLocation>(ref buff);
        }

        if (_flags[17])
        {
            _slowmodeSeconds = buff.ReadInt32(true);
        }

        if (_flags[18])
        {
            _slowmodeNextSendDate = buff.ReadInt32(true);
        }

        if (_flags[12])
        {
            _statsDc = buff.ReadInt32(true);
        }

        _pts = buff.ReadInt32(true);
        if (_flags[21])
        {
            buff.Skip(4);
            _call = factory.Read<InputGroupCall>(ref buff);
        }

        if (_flags[24])
        {
            _ttlPeriod = buff.ReadInt32(true);
        }

        if (_flags[25])
        {
            buff.Skip(4);
            _pendingSuggestions = factory.Read<VectorOfString>(ref buff);
        }

        if (_flags[26])
        {
            buff.Skip(4);
            _groupcallDefaultJoinAs = factory.Read<Peer>(ref buff);
        }

        if (_flags[27])
        {
            _themeEmoticon = buff.ReadTLString();
        }

        if (_flags[28])
        {
            _requestsPending = buff.ReadInt32(true);
        }

        if (_flags[28])
        {
            buff.Skip(4);
            _recentRequesters = factory.Read<VectorOfLong>(ref buff);
        }

        if (_flags[29])
        {
            buff.Skip(4);
            _defaultSendAs = factory.Read<Peer>(ref buff);
        }

        if (_flags[30])
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