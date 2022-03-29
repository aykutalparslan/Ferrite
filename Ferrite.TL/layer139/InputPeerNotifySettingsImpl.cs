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
public class InputPeerNotifySettingsImpl : InputPeerNotifySettings
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public InputPeerNotifySettingsImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1673717362;
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
                writer.WriteTLString(_sound);
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

    private string _sound;
    public string Sound
    {
        get => _sound;
        set
        {
            serialized = false;
            _flags[3] = true;
            _sound = value;
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
            _sound = buff.ReadTLString();
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}