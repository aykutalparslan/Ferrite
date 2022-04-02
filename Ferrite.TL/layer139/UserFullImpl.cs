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
public class UserFullImpl : UserFull
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public UserFullImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -818518751;
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
            if (_flags[1])
            {
                writer.WriteTLString(_about);
            }

            writer.Write(_settings.TLBytes, false);
            if (_flags[2])
            {
                writer.Write(_profilePhoto.TLBytes, false);
            }

            writer.Write(_notifySettings.TLBytes, false);
            if (_flags[3])
            {
                writer.Write(_botInfo.TLBytes, false);
            }

            if (_flags[6])
            {
                writer.WriteInt32(_pinnedMsgId, true);
            }

            writer.WriteInt32(_commonChatsCount, true);
            if (_flags[11])
            {
                writer.WriteInt32(_folderId, true);
            }

            if (_flags[14])
            {
                writer.WriteInt32(_ttlPeriod, true);
            }

            if (_flags[15])
            {
                writer.WriteTLString(_themeEmoticon);
            }

            if (_flags[16])
            {
                writer.WriteTLString(_privateForwardName);
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

    public bool Blocked
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool PhoneCallsAvailable
    {
        get => _flags[4];
        set
        {
            serialized = false;
            _flags[4] = value;
        }
    }

    public bool PhoneCallsPrivate
    {
        get => _flags[5];
        set
        {
            serialized = false;
            _flags[5] = value;
        }
    }

    public bool CanPinMessage
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
        get => _flags[12];
        set
        {
            serialized = false;
            _flags[12] = value;
        }
    }

    public bool VideoCallsAvailable
    {
        get => _flags[13];
        set
        {
            serialized = false;
            _flags[13] = value;
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
            _flags[1] = true;
            _about = value;
        }
    }

    private PeerSettings _settings;
    public PeerSettings Settings
    {
        get => _settings;
        set
        {
            serialized = false;
            _settings = value;
        }
    }

    private Photo _profilePhoto;
    public Photo ProfilePhoto
    {
        get => _profilePhoto;
        set
        {
            serialized = false;
            _flags[2] = true;
            _profilePhoto = value;
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

    private BotInfo _botInfo;
    public BotInfo BotInfo
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

    private int _commonChatsCount;
    public int CommonChatsCount
    {
        get => _commonChatsCount;
        set
        {
            serialized = false;
            _commonChatsCount = value;
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

    private string _themeEmoticon;
    public string ThemeEmoticon
    {
        get => _themeEmoticon;
        set
        {
            serialized = false;
            _flags[15] = true;
            _themeEmoticon = value;
        }
    }

    private string _privateForwardName;
    public string PrivateForwardName
    {
        get => _privateForwardName;
        set
        {
            serialized = false;
            _flags[16] = true;
            _privateForwardName = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _id = buff.ReadInt64(true);
        if (_flags[1])
        {
            _about = buff.ReadTLString();
        }

        _settings = (PeerSettings)factory.Read(buff.ReadInt32(true), ref buff);
        if (_flags[2])
        {
            _profilePhoto = (Photo)factory.Read(buff.ReadInt32(true), ref buff);
        }

        _notifySettings = (PeerNotifySettings)factory.Read(buff.ReadInt32(true), ref buff);
        if (_flags[3])
        {
            _botInfo = (BotInfo)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[6])
        {
            _pinnedMsgId = buff.ReadInt32(true);
        }

        _commonChatsCount = buff.ReadInt32(true);
        if (_flags[11])
        {
            _folderId = buff.ReadInt32(true);
        }

        if (_flags[14])
        {
            _ttlPeriod = buff.ReadInt32(true);
        }

        if (_flags[15])
        {
            _themeEmoticon = buff.ReadTLString();
        }

        if (_flags[16])
        {
            _privateForwardName = buff.ReadTLString();
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}