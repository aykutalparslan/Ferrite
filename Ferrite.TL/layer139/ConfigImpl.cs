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
public class ConfigImpl : Config
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public ConfigImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 856375399;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt32(_date, true);
            writer.WriteInt32(_expires, true);
            writer.WriteInt32(Bool.GetConstructor(_testMode), true);
            writer.WriteInt32(_thisDc, true);
            writer.Write(_dcOptions.TLBytes, false);
            writer.WriteTLString(_dcTxtDomainName);
            writer.WriteInt32(_chatSizeMax, true);
            writer.WriteInt32(_megagroupSizeMax, true);
            writer.WriteInt32(_forwardedCountMax, true);
            writer.WriteInt32(_onlineUpdatePeriodMs, true);
            writer.WriteInt32(_offlineBlurTimeoutMs, true);
            writer.WriteInt32(_offlineIdleTimeoutMs, true);
            writer.WriteInt32(_onlineCloudTimeoutMs, true);
            writer.WriteInt32(_notifyCloudDelayMs, true);
            writer.WriteInt32(_notifyDefaultDelayMs, true);
            writer.WriteInt32(_pushChatPeriodMs, true);
            writer.WriteInt32(_pushChatLimit, true);
            writer.WriteInt32(_savedGifsLimit, true);
            writer.WriteInt32(_editTimeLimit, true);
            writer.WriteInt32(_revokeTimeLimit, true);
            writer.WriteInt32(_revokePmTimeLimit, true);
            writer.WriteInt32(_ratingEDecay, true);
            writer.WriteInt32(_stickersRecentLimit, true);
            writer.WriteInt32(_stickersFavedLimit, true);
            writer.WriteInt32(_channelsReadMediaPeriod, true);
            if (_flags[0])
            {
                writer.WriteInt32(_tmpSessions, true);
            }

            writer.WriteInt32(_pinnedDialogsCountMax, true);
            writer.WriteInt32(_pinnedInfolderCountMax, true);
            writer.WriteInt32(_callReceiveTimeoutMs, true);
            writer.WriteInt32(_callRingTimeoutMs, true);
            writer.WriteInt32(_callConnectTimeoutMs, true);
            writer.WriteInt32(_callPacketTimeoutMs, true);
            writer.WriteTLString(_meUrlPrefix);
            if (_flags[7])
            {
                writer.WriteTLString(_autoupdateUrlPrefix);
            }

            if (_flags[9])
            {
                writer.WriteTLString(_gifSearchUsername);
            }

            if (_flags[10])
            {
                writer.WriteTLString(_venueSearchUsername);
            }

            if (_flags[11])
            {
                writer.WriteTLString(_imgSearchUsername);
            }

            if (_flags[12])
            {
                writer.WriteTLString(_staticMapsProvider);
            }

            writer.WriteInt32(_captionLengthMax, true);
            writer.WriteInt32(_messageLengthMax, true);
            writer.WriteInt32(_webfileDcId, true);
            if (_flags[2])
            {
                writer.WriteTLString(_suggestedLangCode);
            }

            if (_flags[2])
            {
                writer.WriteInt32(_langPackVersion, true);
            }

            if (_flags[2])
            {
                writer.WriteInt32(_baseLangPackVersion, true);
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

    public bool PhonecallsEnabled
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
        }
    }

    public bool DefaultP2pContacts
    {
        get => _flags[3];
        set
        {
            serialized = false;
            _flags[3] = value;
        }
    }

    public bool PreloadFeaturedStickers
    {
        get => _flags[4];
        set
        {
            serialized = false;
            _flags[4] = value;
        }
    }

    public bool IgnorePhoneEntities
    {
        get => _flags[5];
        set
        {
            serialized = false;
            _flags[5] = value;
        }
    }

    public bool RevokePmInbox
    {
        get => _flags[6];
        set
        {
            serialized = false;
            _flags[6] = value;
        }
    }

    public bool BlockedMode
    {
        get => _flags[8];
        set
        {
            serialized = false;
            _flags[8] = value;
        }
    }

    public bool PfsEnabled
    {
        get => _flags[13];
        set
        {
            serialized = false;
            _flags[13] = value;
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

    private int _expires;
    public int Expires
    {
        get => _expires;
        set
        {
            serialized = false;
            _expires = value;
        }
    }

    private bool _testMode;
    public bool TestMode
    {
        get => _testMode;
        set
        {
            serialized = false;
            _testMode = value;
        }
    }

    private int _thisDc;
    public int ThisDc
    {
        get => _thisDc;
        set
        {
            serialized = false;
            _thisDc = value;
        }
    }

    private Vector<DcOption> _dcOptions;
    public Vector<DcOption> DcOptions
    {
        get => _dcOptions;
        set
        {
            serialized = false;
            _dcOptions = value;
        }
    }

    private string _dcTxtDomainName;
    public string DcTxtDomainName
    {
        get => _dcTxtDomainName;
        set
        {
            serialized = false;
            _dcTxtDomainName = value;
        }
    }

    private int _chatSizeMax;
    public int ChatSizeMax
    {
        get => _chatSizeMax;
        set
        {
            serialized = false;
            _chatSizeMax = value;
        }
    }

    private int _megagroupSizeMax;
    public int MegagroupSizeMax
    {
        get => _megagroupSizeMax;
        set
        {
            serialized = false;
            _megagroupSizeMax = value;
        }
    }

    private int _forwardedCountMax;
    public int ForwardedCountMax
    {
        get => _forwardedCountMax;
        set
        {
            serialized = false;
            _forwardedCountMax = value;
        }
    }

    private int _onlineUpdatePeriodMs;
    public int OnlineUpdatePeriodMs
    {
        get => _onlineUpdatePeriodMs;
        set
        {
            serialized = false;
            _onlineUpdatePeriodMs = value;
        }
    }

    private int _offlineBlurTimeoutMs;
    public int OfflineBlurTimeoutMs
    {
        get => _offlineBlurTimeoutMs;
        set
        {
            serialized = false;
            _offlineBlurTimeoutMs = value;
        }
    }

    private int _offlineIdleTimeoutMs;
    public int OfflineIdleTimeoutMs
    {
        get => _offlineIdleTimeoutMs;
        set
        {
            serialized = false;
            _offlineIdleTimeoutMs = value;
        }
    }

    private int _onlineCloudTimeoutMs;
    public int OnlineCloudTimeoutMs
    {
        get => _onlineCloudTimeoutMs;
        set
        {
            serialized = false;
            _onlineCloudTimeoutMs = value;
        }
    }

    private int _notifyCloudDelayMs;
    public int NotifyCloudDelayMs
    {
        get => _notifyCloudDelayMs;
        set
        {
            serialized = false;
            _notifyCloudDelayMs = value;
        }
    }

    private int _notifyDefaultDelayMs;
    public int NotifyDefaultDelayMs
    {
        get => _notifyDefaultDelayMs;
        set
        {
            serialized = false;
            _notifyDefaultDelayMs = value;
        }
    }

    private int _pushChatPeriodMs;
    public int PushChatPeriodMs
    {
        get => _pushChatPeriodMs;
        set
        {
            serialized = false;
            _pushChatPeriodMs = value;
        }
    }

    private int _pushChatLimit;
    public int PushChatLimit
    {
        get => _pushChatLimit;
        set
        {
            serialized = false;
            _pushChatLimit = value;
        }
    }

    private int _savedGifsLimit;
    public int SavedGifsLimit
    {
        get => _savedGifsLimit;
        set
        {
            serialized = false;
            _savedGifsLimit = value;
        }
    }

    private int _editTimeLimit;
    public int EditTimeLimit
    {
        get => _editTimeLimit;
        set
        {
            serialized = false;
            _editTimeLimit = value;
        }
    }

    private int _revokeTimeLimit;
    public int RevokeTimeLimit
    {
        get => _revokeTimeLimit;
        set
        {
            serialized = false;
            _revokeTimeLimit = value;
        }
    }

    private int _revokePmTimeLimit;
    public int RevokePmTimeLimit
    {
        get => _revokePmTimeLimit;
        set
        {
            serialized = false;
            _revokePmTimeLimit = value;
        }
    }

    private int _ratingEDecay;
    public int RatingEDecay
    {
        get => _ratingEDecay;
        set
        {
            serialized = false;
            _ratingEDecay = value;
        }
    }

    private int _stickersRecentLimit;
    public int StickersRecentLimit
    {
        get => _stickersRecentLimit;
        set
        {
            serialized = false;
            _stickersRecentLimit = value;
        }
    }

    private int _stickersFavedLimit;
    public int StickersFavedLimit
    {
        get => _stickersFavedLimit;
        set
        {
            serialized = false;
            _stickersFavedLimit = value;
        }
    }

    private int _channelsReadMediaPeriod;
    public int ChannelsReadMediaPeriod
    {
        get => _channelsReadMediaPeriod;
        set
        {
            serialized = false;
            _channelsReadMediaPeriod = value;
        }
    }

    private int _tmpSessions;
    public int TmpSessions
    {
        get => _tmpSessions;
        set
        {
            serialized = false;
            _flags[0] = true;
            _tmpSessions = value;
        }
    }

    private int _pinnedDialogsCountMax;
    public int PinnedDialogsCountMax
    {
        get => _pinnedDialogsCountMax;
        set
        {
            serialized = false;
            _pinnedDialogsCountMax = value;
        }
    }

    private int _pinnedInfolderCountMax;
    public int PinnedInfolderCountMax
    {
        get => _pinnedInfolderCountMax;
        set
        {
            serialized = false;
            _pinnedInfolderCountMax = value;
        }
    }

    private int _callReceiveTimeoutMs;
    public int CallReceiveTimeoutMs
    {
        get => _callReceiveTimeoutMs;
        set
        {
            serialized = false;
            _callReceiveTimeoutMs = value;
        }
    }

    private int _callRingTimeoutMs;
    public int CallRingTimeoutMs
    {
        get => _callRingTimeoutMs;
        set
        {
            serialized = false;
            _callRingTimeoutMs = value;
        }
    }

    private int _callConnectTimeoutMs;
    public int CallConnectTimeoutMs
    {
        get => _callConnectTimeoutMs;
        set
        {
            serialized = false;
            _callConnectTimeoutMs = value;
        }
    }

    private int _callPacketTimeoutMs;
    public int CallPacketTimeoutMs
    {
        get => _callPacketTimeoutMs;
        set
        {
            serialized = false;
            _callPacketTimeoutMs = value;
        }
    }

    private string _meUrlPrefix;
    public string MeUrlPrefix
    {
        get => _meUrlPrefix;
        set
        {
            serialized = false;
            _meUrlPrefix = value;
        }
    }

    private string _autoupdateUrlPrefix;
    public string AutoupdateUrlPrefix
    {
        get => _autoupdateUrlPrefix;
        set
        {
            serialized = false;
            _flags[7] = true;
            _autoupdateUrlPrefix = value;
        }
    }

    private string _gifSearchUsername;
    public string GifSearchUsername
    {
        get => _gifSearchUsername;
        set
        {
            serialized = false;
            _flags[9] = true;
            _gifSearchUsername = value;
        }
    }

    private string _venueSearchUsername;
    public string VenueSearchUsername
    {
        get => _venueSearchUsername;
        set
        {
            serialized = false;
            _flags[10] = true;
            _venueSearchUsername = value;
        }
    }

    private string _imgSearchUsername;
    public string ImgSearchUsername
    {
        get => _imgSearchUsername;
        set
        {
            serialized = false;
            _flags[11] = true;
            _imgSearchUsername = value;
        }
    }

    private string _staticMapsProvider;
    public string StaticMapsProvider
    {
        get => _staticMapsProvider;
        set
        {
            serialized = false;
            _flags[12] = true;
            _staticMapsProvider = value;
        }
    }

    private int _captionLengthMax;
    public int CaptionLengthMax
    {
        get => _captionLengthMax;
        set
        {
            serialized = false;
            _captionLengthMax = value;
        }
    }

    private int _messageLengthMax;
    public int MessageLengthMax
    {
        get => _messageLengthMax;
        set
        {
            serialized = false;
            _messageLengthMax = value;
        }
    }

    private int _webfileDcId;
    public int WebfileDcId
    {
        get => _webfileDcId;
        set
        {
            serialized = false;
            _webfileDcId = value;
        }
    }

    private string _suggestedLangCode;
    public string SuggestedLangCode
    {
        get => _suggestedLangCode;
        set
        {
            serialized = false;
            _flags[2] = true;
            _suggestedLangCode = value;
        }
    }

    private int _langPackVersion;
    public int LangPackVersion
    {
        get => _langPackVersion;
        set
        {
            serialized = false;
            _flags[2] = true;
            _langPackVersion = value;
        }
    }

    private int _baseLangPackVersion;
    public int BaseLangPackVersion
    {
        get => _baseLangPackVersion;
        set
        {
            serialized = false;
            _flags[2] = true;
            _baseLangPackVersion = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _date = buff.ReadInt32(true);
        _expires = buff.ReadInt32(true);
        _testMode = Bool.Read(ref buff);
        _thisDc = buff.ReadInt32(true);
        buff.Skip(4); _dcOptions  =  factory . Read < Vector < DcOption > > ( ref  buff ) ; 
        _dcTxtDomainName = buff.ReadTLString();
        _chatSizeMax = buff.ReadInt32(true);
        _megagroupSizeMax = buff.ReadInt32(true);
        _forwardedCountMax = buff.ReadInt32(true);
        _onlineUpdatePeriodMs = buff.ReadInt32(true);
        _offlineBlurTimeoutMs = buff.ReadInt32(true);
        _offlineIdleTimeoutMs = buff.ReadInt32(true);
        _onlineCloudTimeoutMs = buff.ReadInt32(true);
        _notifyCloudDelayMs = buff.ReadInt32(true);
        _notifyDefaultDelayMs = buff.ReadInt32(true);
        _pushChatPeriodMs = buff.ReadInt32(true);
        _pushChatLimit = buff.ReadInt32(true);
        _savedGifsLimit = buff.ReadInt32(true);
        _editTimeLimit = buff.ReadInt32(true);
        _revokeTimeLimit = buff.ReadInt32(true);
        _revokePmTimeLimit = buff.ReadInt32(true);
        _ratingEDecay = buff.ReadInt32(true);
        _stickersRecentLimit = buff.ReadInt32(true);
        _stickersFavedLimit = buff.ReadInt32(true);
        _channelsReadMediaPeriod = buff.ReadInt32(true);
        if (_flags[0])
        {
            _tmpSessions = buff.ReadInt32(true);
        }

        _pinnedDialogsCountMax = buff.ReadInt32(true);
        _pinnedInfolderCountMax = buff.ReadInt32(true);
        _callReceiveTimeoutMs = buff.ReadInt32(true);
        _callRingTimeoutMs = buff.ReadInt32(true);
        _callConnectTimeoutMs = buff.ReadInt32(true);
        _callPacketTimeoutMs = buff.ReadInt32(true);
        _meUrlPrefix = buff.ReadTLString();
        if (_flags[7])
        {
            _autoupdateUrlPrefix = buff.ReadTLString();
        }

        if (_flags[9])
        {
            _gifSearchUsername = buff.ReadTLString();
        }

        if (_flags[10])
        {
            _venueSearchUsername = buff.ReadTLString();
        }

        if (_flags[11])
        {
            _imgSearchUsername = buff.ReadTLString();
        }

        if (_flags[12])
        {
            _staticMapsProvider = buff.ReadTLString();
        }

        _captionLengthMax = buff.ReadInt32(true);
        _messageLengthMax = buff.ReadInt32(true);
        _webfileDcId = buff.ReadInt32(true);
        if (_flags[2])
        {
            _suggestedLangCode = buff.ReadTLString();
        }

        if (_flags[2])
        {
            _langPackVersion = buff.ReadInt32(true);
        }

        if (_flags[2])
        {
            _baseLangPackVersion = buff.ReadInt32(true);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}