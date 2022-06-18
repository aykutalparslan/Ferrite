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
public class AvailableReactionImpl : AvailableReaction
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public AvailableReactionImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1065882623;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteTLString(_reaction);
            writer.WriteTLString(_title);
            writer.Write(_staticIcon.TLBytes, false);
            writer.Write(_appearAnimation.TLBytes, false);
            writer.Write(_selectAnimation.TLBytes, false);
            writer.Write(_activateAnimation.TLBytes, false);
            writer.Write(_effectAnimation.TLBytes, false);
            if (_flags[1])
            {
                writer.Write(_aroundAnimation.TLBytes, false);
            }

            if (_flags[1])
            {
                writer.Write(_centerIcon.TLBytes, false);
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

    public bool Inactive
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    private string _reaction;
    public string Reaction
    {
        get => _reaction;
        set
        {
            serialized = false;
            _reaction = value;
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

    private Document _staticIcon;
    public Document StaticIcon
    {
        get => _staticIcon;
        set
        {
            serialized = false;
            _staticIcon = value;
        }
    }

    private Document _appearAnimation;
    public Document AppearAnimation
    {
        get => _appearAnimation;
        set
        {
            serialized = false;
            _appearAnimation = value;
        }
    }

    private Document _selectAnimation;
    public Document SelectAnimation
    {
        get => _selectAnimation;
        set
        {
            serialized = false;
            _selectAnimation = value;
        }
    }

    private Document _activateAnimation;
    public Document ActivateAnimation
    {
        get => _activateAnimation;
        set
        {
            serialized = false;
            _activateAnimation = value;
        }
    }

    private Document _effectAnimation;
    public Document EffectAnimation
    {
        get => _effectAnimation;
        set
        {
            serialized = false;
            _effectAnimation = value;
        }
    }

    private Document _aroundAnimation;
    public Document AroundAnimation
    {
        get => _aroundAnimation;
        set
        {
            serialized = false;
            _flags[1] = true;
            _aroundAnimation = value;
        }
    }

    private Document _centerIcon;
    public Document CenterIcon
    {
        get => _centerIcon;
        set
        {
            serialized = false;
            _flags[1] = true;
            _centerIcon = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _reaction = buff.ReadTLString();
        _title = buff.ReadTLString();
        _staticIcon = (Document)factory.Read(buff.ReadInt32(true), ref buff);
        _appearAnimation = (Document)factory.Read(buff.ReadInt32(true), ref buff);
        _selectAnimation = (Document)factory.Read(buff.ReadInt32(true), ref buff);
        _activateAnimation = (Document)factory.Read(buff.ReadInt32(true), ref buff);
        _effectAnimation = (Document)factory.Read(buff.ReadInt32(true), ref buff);
        if (_flags[1])
        {
            _aroundAnimation = (Document)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[1])
        {
            _centerIcon = (Document)factory.Read(buff.ReadInt32(true), ref buff);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}