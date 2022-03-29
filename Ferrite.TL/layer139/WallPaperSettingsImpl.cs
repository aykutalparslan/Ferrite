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
public class WallPaperSettingsImpl : WallPaperSettings
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public WallPaperSettingsImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 499236004;
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
                writer.WriteInt32(_backgroundColor, true);
            }

            if (_flags[4])
            {
                writer.WriteInt32(_secondBackgroundColor, true);
            }

            if (_flags[5])
            {
                writer.WriteInt32(_thirdBackgroundColor, true);
            }

            if (_flags[6])
            {
                writer.WriteInt32(_fourthBackgroundColor, true);
            }

            if (_flags[3])
            {
                writer.WriteInt32(_intensity, true);
            }

            if (_flags[4])
            {
                writer.WriteInt32(_rotation, true);
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

    public bool Blur
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
        }
    }

    public bool Motion
    {
        get => _flags[2];
        set
        {
            serialized = false;
            _flags[2] = value;
        }
    }

    private int _backgroundColor;
    public int BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            serialized = false;
            _flags[0] = true;
            _backgroundColor = value;
        }
    }

    private int _secondBackgroundColor;
    public int SecondBackgroundColor
    {
        get => _secondBackgroundColor;
        set
        {
            serialized = false;
            _flags[4] = true;
            _secondBackgroundColor = value;
        }
    }

    private int _thirdBackgroundColor;
    public int ThirdBackgroundColor
    {
        get => _thirdBackgroundColor;
        set
        {
            serialized = false;
            _flags[5] = true;
            _thirdBackgroundColor = value;
        }
    }

    private int _fourthBackgroundColor;
    public int FourthBackgroundColor
    {
        get => _fourthBackgroundColor;
        set
        {
            serialized = false;
            _flags[6] = true;
            _fourthBackgroundColor = value;
        }
    }

    private int _intensity;
    public int Intensity
    {
        get => _intensity;
        set
        {
            serialized = false;
            _flags[3] = true;
            _intensity = value;
        }
    }

    private int _rotation;
    public int Rotation
    {
        get => _rotation;
        set
        {
            serialized = false;
            _flags[4] = true;
            _rotation = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        if (_flags[0])
        {
            _backgroundColor = buff.ReadInt32(true);
        }

        if (_flags[4])
        {
            _secondBackgroundColor = buff.ReadInt32(true);
        }

        if (_flags[5])
        {
            _thirdBackgroundColor = buff.ReadInt32(true);
        }

        if (_flags[6])
        {
            _fourthBackgroundColor = buff.ReadInt32(true);
        }

        if (_flags[3])
        {
            _intensity = buff.ReadInt32(true);
        }

        if (_flags[4])
        {
            _rotation = buff.ReadInt32(true);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}