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
public class InputThemeSettingsImpl : InputThemeSettings
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public InputThemeSettingsImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1881255857;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.Write(_baseTheme.TLBytes, false);
            writer.WriteInt32(_accentColor, true);
            if (_flags[3])
            {
                writer.WriteInt32(_outboxAccentColor, true);
            }

            if (_flags[0])
            {
                writer.Write(_messageColors.TLBytes, false);
            }

            if (_flags[1])
            {
                writer.Write(_wallpaper.TLBytes, false);
            }

            if (_flags[1])
            {
                writer.Write(_wallpaperSettings.TLBytes, false);
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

    public bool MessageColorsAnimated
    {
        get => _flags[2];
        set
        {
            serialized = false;
            _flags[2] = value;
        }
    }

    private BaseTheme _baseTheme;
    public BaseTheme BaseTheme
    {
        get => _baseTheme;
        set
        {
            serialized = false;
            _baseTheme = value;
        }
    }

    private int _accentColor;
    public int AccentColor
    {
        get => _accentColor;
        set
        {
            serialized = false;
            _accentColor = value;
        }
    }

    private int _outboxAccentColor;
    public int OutboxAccentColor
    {
        get => _outboxAccentColor;
        set
        {
            serialized = false;
            _flags[3] = true;
            _outboxAccentColor = value;
        }
    }

    private VectorOfInt _messageColors;
    public VectorOfInt MessageColors
    {
        get => _messageColors;
        set
        {
            serialized = false;
            _flags[0] = true;
            _messageColors = value;
        }
    }

    private InputWallPaper _wallpaper;
    public InputWallPaper Wallpaper
    {
        get => _wallpaper;
        set
        {
            serialized = false;
            _flags[1] = true;
            _wallpaper = value;
        }
    }

    private WallPaperSettings _wallpaperSettings;
    public WallPaperSettings WallpaperSettings
    {
        get => _wallpaperSettings;
        set
        {
            serialized = false;
            _flags[1] = true;
            _wallpaperSettings = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _baseTheme = (BaseTheme)factory.Read(buff.ReadInt32(true), ref buff);
        _accentColor = buff.ReadInt32(true);
        if (_flags[3])
        {
            _outboxAccentColor = buff.ReadInt32(true);
        }

        if (_flags[0])
        {
            buff.Skip(4);
            _messageColors = factory.Read<VectorOfInt>(ref buff);
        }

        if (_flags[1])
        {
            _wallpaper = (InputWallPaper)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[1])
        {
            _wallpaperSettings = (WallPaperSettings)factory.Read(buff.ReadInt32(true), ref buff);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}