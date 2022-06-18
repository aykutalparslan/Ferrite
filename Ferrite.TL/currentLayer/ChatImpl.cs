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
public class ChatImpl : Chat
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public ChatImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 1103884886;
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
            writer.WriteTLString(_title);
            writer.Write(_photo.TLBytes, false);
            writer.WriteInt32(_participantsCount, true);
            writer.WriteInt32(_date, true);
            writer.WriteInt32(_version, true);
            if (_flags[6])
            {
                writer.Write(_migratedTo.TLBytes, false);
            }

            if (_flags[14])
            {
                writer.Write(_adminRights.TLBytes, false);
            }

            if (_flags[18])
            {
                writer.Write(_defaultBannedRights.TLBytes, false);
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

    public bool Kicked
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
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

    public bool Deactivated
    {
        get => _flags[5];
        set
        {
            serialized = false;
            _flags[5] = value;
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

    public bool Noforwards
    {
        get => _flags[25];
        set
        {
            serialized = false;
            _flags[25] = value;
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

    private int _participantsCount;
    public int ParticipantsCount
    {
        get => _participantsCount;
        set
        {
            serialized = false;
            _participantsCount = value;
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

    private int _version;
    public int Version
    {
        get => _version;
        set
        {
            serialized = false;
            _version = value;
        }
    }

    private InputChannel _migratedTo;
    public InputChannel MigratedTo
    {
        get => _migratedTo;
        set
        {
            serialized = false;
            _flags[6] = true;
            _migratedTo = value;
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

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _id = buff.ReadInt64(true);
        _title = buff.ReadTLString();
        _photo = (ChatPhoto)factory.Read(buff.ReadInt32(true), ref buff);
        _participantsCount = buff.ReadInt32(true);
        _date = buff.ReadInt32(true);
        _version = buff.ReadInt32(true);
        if (_flags[6])
        {
            _migratedTo = (InputChannel)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[14])
        {
            _adminRights = (ChatAdminRights)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[18])
        {
            _defaultBannedRights = (ChatBannedRights)factory.Read(buff.ReadInt32(true), ref buff);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}