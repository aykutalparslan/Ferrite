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
public class UserImpl : User
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public UserImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 1073147056;
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
            if (_flags[0])
            {
                writer.WriteInt64(_accessHash, true);
            }

            if (_flags[1])
            {
                writer.WriteTLString(_firstName);
            }

            if (_flags[2])
            {
                writer.WriteTLString(_lastName);
            }

            if (_flags[3])
            {
                writer.WriteTLString(_username);
            }

            if (_flags[4])
            {
                writer.WriteTLString(_phone);
            }

            if (_flags[5])
            {
                writer.Write(_photo.TLBytes, false);
            }

            if (_flags[6])
            {
                writer.Write(_status.TLBytes, false);
            }

            if (_flags[14])
            {
                writer.WriteInt32(_botInfoVersion, true);
            }

            if (_flags[18])
            {
                writer.Write(_restrictionReason.TLBytes, false);
            }

            if (_flags[19])
            {
                writer.WriteTLString(_botInlinePlaceholder);
            }

            if (_flags[22])
            {
                writer.WriteTLString(_langCode);
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

    public bool Self
    {
        get => _flags[10];
        set
        {
            serialized = false;
            _flags[10] = value;
        }
    }

    public bool Contact
    {
        get => _flags[11];
        set
        {
            serialized = false;
            _flags[11] = value;
        }
    }

    public bool MutualContact
    {
        get => _flags[12];
        set
        {
            serialized = false;
            _flags[12] = value;
        }
    }

    public bool Deleted
    {
        get => _flags[13];
        set
        {
            serialized = false;
            _flags[13] = value;
        }
    }

    public bool Bot
    {
        get => _flags[14];
        set
        {
            serialized = false;
            _flags[14] = value;
        }
    }

    public bool BotChatHistory
    {
        get => _flags[15];
        set
        {
            serialized = false;
            _flags[15] = value;
        }
    }

    public bool BotNochats
    {
        get => _flags[16];
        set
        {
            serialized = false;
            _flags[16] = value;
        }
    }

    public bool Verified
    {
        get => _flags[17];
        set
        {
            serialized = false;
            _flags[17] = value;
        }
    }

    public bool Restricted
    {
        get => _flags[18];
        set
        {
            serialized = false;
            _flags[18] = value;
        }
    }

    public bool Min
    {
        get => _flags[20];
        set
        {
            serialized = false;
            _flags[20] = value;
        }
    }

    public bool BotInlineGeo
    {
        get => _flags[21];
        set
        {
            serialized = false;
            _flags[21] = value;
        }
    }

    public bool Support
    {
        get => _flags[23];
        set
        {
            serialized = false;
            _flags[23] = value;
        }
    }

    public bool Scam
    {
        get => _flags[24];
        set
        {
            serialized = false;
            _flags[24] = value;
        }
    }

    public bool ApplyMinPhoto
    {
        get => _flags[25];
        set
        {
            serialized = false;
            _flags[25] = value;
        }
    }

    public bool Fake
    {
        get => _flags[26];
        set
        {
            serialized = false;
            _flags[26] = value;
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
            _flags[0] = true;
            _accessHash = value;
        }
    }

    private string _firstName;
    public string FirstName
    {
        get => _firstName;
        set
        {
            serialized = false;
            _flags[1] = true;
            _firstName = value;
        }
    }

    private string _lastName;
    public string LastName
    {
        get => _lastName;
        set
        {
            serialized = false;
            _flags[2] = true;
            _lastName = value;
        }
    }

    private string _username;
    public string Username
    {
        get => _username;
        set
        {
            serialized = false;
            _flags[3] = true;
            _username = value;
        }
    }

    private string _phone;
    public string Phone
    {
        get => _phone;
        set
        {
            serialized = false;
            _flags[4] = true;
            _phone = value;
        }
    }

    private UserProfilePhoto _photo;
    public UserProfilePhoto Photo
    {
        get => _photo;
        set
        {
            serialized = false;
            _flags[5] = true;
            _photo = value;
        }
    }

    private UserStatus _status;
    public UserStatus Status
    {
        get => _status;
        set
        {
            serialized = false;
            _flags[6] = true;
            _status = value;
        }
    }

    private int _botInfoVersion;
    public int BotInfoVersion
    {
        get => _botInfoVersion;
        set
        {
            serialized = false;
            _flags[14] = true;
            _botInfoVersion = value;
        }
    }

    private Vector<RestrictionReason> _restrictionReason;
    public Vector<RestrictionReason> RestrictionReason
    {
        get => _restrictionReason;
        set
        {
            serialized = false;
            _flags[18] = true;
            _restrictionReason = value;
        }
    }

    private string _botInlinePlaceholder;
    public string BotInlinePlaceholder
    {
        get => _botInlinePlaceholder;
        set
        {
            serialized = false;
            _flags[19] = true;
            _botInlinePlaceholder = value;
        }
    }

    private string _langCode;
    public string LangCode
    {
        get => _langCode;
        set
        {
            serialized = false;
            _flags[22] = true;
            _langCode = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _id = buff.ReadInt64(true);
        if (_flags[0])
        {
            _accessHash = buff.ReadInt64(true);
        }

        if (_flags[1])
        {
            _firstName = buff.ReadTLString();
        }

        if (_flags[2])
        {
            _lastName = buff.ReadTLString();
        }

        if (_flags[3])
        {
            _username = buff.ReadTLString();
        }

        if (_flags[4])
        {
            _phone = buff.ReadTLString();
        }

        if (_flags[5])
        {
            _photo = (UserProfilePhoto)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[6])
        {
            _status = (UserStatus)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[14])
        {
            _botInfoVersion = buff.ReadInt32(true);
        }

        if (_flags[18])
        {
            buff.Skip(4);
            _restrictionReason = factory.Read<Vector<RestrictionReason>>(ref buff);
        }

        if (_flags[19])
        {
            _botInlinePlaceholder = buff.ReadTLString();
        }

        if (_flags[22])
        {
            _langCode = buff.ReadTLString();
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}