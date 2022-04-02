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
public class ChannelImpl : Chat
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public ChannelImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -2107528095;
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
            if (_flags[13])
            {
                writer.WriteInt64(_accessHash, true);
            }

            writer.WriteTLString(_title);
            if (_flags[6])
            {
                writer.WriteTLString(_username);
            }

            writer.Write(_photo.TLBytes, false);
            writer.WriteInt32(_date, true);
            if (_flags[9])
            {
                writer.Write(_restrictionReason.TLBytes, false);
            }

            if (_flags[14])
            {
                writer.Write(_adminRights.TLBytes, false);
            }

            if (_flags[15])
            {
                writer.Write(_bannedRights.TLBytes, false);
            }

            if (_flags[18])
            {
                writer.Write(_defaultBannedRights.TLBytes, false);
            }

            if (_flags[17])
            {
                writer.WriteInt32(_participantsCount, true);
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

    public bool Creator
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool Left
    {
        get => _flags[2];
        set
        {
            serialized = false;
            _flags[2] = value;
        }
    }

    public bool Broadcast
    {
        get => _flags[5];
        set
        {
            serialized = false;
            _flags[5] = value;
        }
    }

    public bool Verified
    {
        get => _flags[7];
        set
        {
            serialized = false;
            _flags[7] = value;
        }
    }

    public bool Megagroup
    {
        get => _flags[8];
        set
        {
            serialized = false;
            _flags[8] = value;
        }
    }

    public bool Restricted
    {
        get => _flags[9];
        set
        {
            serialized = false;
            _flags[9] = value;
        }
    }

    public bool Signatures
    {
        get => _flags[11];
        set
        {
            serialized = false;
            _flags[11] = value;
        }
    }

    public bool Min
    {
        get => _flags[12];
        set
        {
            serialized = false;
            _flags[12] = value;
        }
    }

    public bool Scam
    {
        get => _flags[19];
        set
        {
            serialized = false;
            _flags[19] = value;
        }
    }

    public bool HasLink
    {
        get => _flags[20];
        set
        {
            serialized = false;
            _flags[20] = value;
        }
    }

    public bool HasGeo
    {
        get => _flags[21];
        set
        {
            serialized = false;
            _flags[21] = value;
        }
    }

    public bool SlowmodeEnabled
    {
        get => _flags[22];
        set
        {
            serialized = false;
            _flags[22] = value;
        }
    }

    public bool CallActive
    {
        get => _flags[23];
        set
        {
            serialized = false;
            _flags[23] = value;
        }
    }

    public bool CallNotEmpty
    {
        get => _flags[24];
        set
        {
            serialized = false;
            _flags[24] = value;
        }
    }

    public bool Fake
    {
        get => _flags[25];
        set
        {
            serialized = false;
            _flags[25] = value;
        }
    }

    public bool Gigagroup
    {
        get => _flags[26];
        set
        {
            serialized = false;
            _flags[26] = value;
        }
    }

    public bool Noforwards
    {
        get => _flags[27];
        set
        {
            serialized = false;
            _flags[27] = value;
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

    private long _accessHash;
    public long AccessHash
    {
        get => _accessHash;
        set
        {
            serialized = false;
            _flags[13] = true;
            _accessHash = value;
        }
    }

    private string _title;
    public string Title
    {
        get => _title;
        set
        {
            serialized = false;
            _title = value;
        }
    }

    private string _username;
    public string Username
    {
        get => _username;
        set
        {
            serialized = false;
            _flags[6] = true;
            _username = value;
        }
    }

    private ChatPhoto _photo;
    public ChatPhoto Photo
    {
        get => _photo;
        set
        {
            serialized = false;
            _photo = value;
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

    private Vector<RestrictionReason> _restrictionReason;
    public Vector<RestrictionReason> RestrictionReason
    {
        get => _restrictionReason;
        set
        {
            serialized = false;
            _flags[9] = true;
            _restrictionReason = value;
        }
    }

    private ChatAdminRights _adminRights;
    public ChatAdminRights AdminRights
    {
        get => _adminRights;
        set
        {
            serialized = false;
            _flags[14] = true;
            _adminRights = value;
        }
    }

    private ChatBannedRights _bannedRights;
    public ChatBannedRights BannedRights
    {
        get => _bannedRights;
        set
        {
            serialized = false;
            _flags[15] = true;
            _bannedRights = value;
        }
    }

    private ChatBannedRights _defaultBannedRights;
    public ChatBannedRights DefaultBannedRights
    {
        get => _defaultBannedRights;
        set
        {
            serialized = false;
            _flags[18] = true;
            _defaultBannedRights = value;
        }
    }

    private int _participantsCount;
    public int ParticipantsCount
    {
        get => _participantsCount;
        set
        {
            serialized = false;
            _flags[17] = true;
            _participantsCount = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _id = buff.ReadInt64(true);
        if (_flags[13])
        {
            _accessHash = buff.ReadInt64(true);
        }

        _title = buff.ReadTLString();
        if (_flags[6])
        {
            _username = buff.ReadTLString();
        }

        _photo = (ChatPhoto)factory.Read(buff.ReadInt32(true), ref buff);
        _date = buff.ReadInt32(true);
        if (_flags[9])
        {
            buff.Skip(4);
            _restrictionReason = factory.Read<Vector<RestrictionReason>>(ref buff);
        }

        if (_flags[14])
        {
            _adminRights = (ChatAdminRights)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[15])
        {
            _bannedRights = (ChatBannedRights)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[18])
        {
            _defaultBannedRights = (ChatBannedRights)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[17])
        {
            _participantsCount = buff.ReadInt32(true);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}