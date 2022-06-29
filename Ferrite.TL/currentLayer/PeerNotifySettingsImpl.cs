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
public class PeerNotifySettingsImpl : PeerNotifySettings
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PeerNotifySettingsImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => unchecked((int)0xa83b0426);
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
                writer.WriteInt32(Bool.GetConstructor(_showPreviews), true);
            }

            if (_flags[1])
            {
                writer.WriteInt32(Bool.GetConstructor(_silent), true);
            }

            if (_flags[2])
            {
                writer.WriteInt32(_muteUntil, true);
            }

            if (_flags[3])
            {
                writer.Write(_iOSSound.TLBytes, false);
            }
            if(_flags[4])
            {
                writer.Write(_androidSound.TLBytes, false);
            }

            if (_flags[5])
            {
                writer.Write(_otherSound.TLBytes, false);
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

    private bool _showPreviews;
    public bool ShowPreviews
    {
        get => _showPreviews;
        set
        {
            serialized = false;
            _flags[0] = true;
            _showPreviews = value;
        }
    }

    private bool _silent;
    public bool Silent
    {
        get => _silent;
        set
        {
            serialized = false;
            _flags[1] = true;
            _silent = value;
        }
    }

    private int _muteUntil;
    public int MuteUntil
    {
        get => _muteUntil;
        set
        {
            serialized = false;
            _flags[2] = true;
            _muteUntil = value;
        }
    }

    private NotificationSound _iOSSound;
    public NotificationSound iOSSound
    {
        get => _iOSSound;
        set
        {
            serialized = false;
            _flags[3] = true;
            _iOSSound = value;
        }
    }
    private NotificationSound _androidSound;
    public NotificationSound AndroidSound
    {
        get => _androidSound;
        set
        {
            serialized = false;
            _flags[4] = true;
            _androidSound = value;
        }
    }
    private NotificationSound _otherSound;
    public NotificationSound OtherSound
    {
        get => _otherSound;
        set
        {
            serialized = false;
            _flags[5] = true;
            _otherSound = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        if (_flags[0])
        {
            _showPreviews = Bool.Read(ref buff);
        }

        if (_flags[1])
        {
            _silent = Bool.Read(ref buff);
        }

        if (_flags[2])
        {
            _muteUntil = buff.ReadInt32(true);
        }

        if (_flags[3])
        {
            _iOSSound = (NotificationSound)factory.Read(buff.ReadInt32(true), ref buff);
        }
        if(_flags[4])
        {
            _androidSound = (NotificationSound)factory.Read(buff.ReadInt32(true), ref buff);
        }
        if(_flags[5])
        {
            _otherSound = (NotificationSound)factory.Read(buff.ReadInt32(true), ref buff);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}