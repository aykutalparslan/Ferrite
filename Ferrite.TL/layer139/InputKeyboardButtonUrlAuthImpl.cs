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
public class InputKeyboardButtonUrlAuthImpl : KeyboardButton
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public InputKeyboardButtonUrlAuthImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -802258988;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteTLString(_text);
            if (_flags[1])
            {
                writer.WriteTLString(_fwdText);
            }

            writer.WriteTLString(_url);
            writer.Write(_bot.TLBytes, false);
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

    public bool RequestWriteAccess
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    private string _text;
    public string Text
    {
        get => _text;
        set
        {
            serialized = false;
            _text = value;
        }
    }

    private string _fwdText;
    public string FwdText
    {
        get => _fwdText;
        set
        {
            serialized = false;
            _flags[1] = true;
            _fwdText = value;
        }
    }

    private string _url;
    public string Url
    {
        get => _url;
        set
        {
            serialized = false;
            _url = value;
        }
    }

    private InputUser _bot;
    public InputUser Bot
    {
        get => _bot;
        set
        {
            serialized = false;
            _bot = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _text = buff.ReadTLString();
        if (_flags[1])
        {
            _fwdText = buff.ReadTLString();
        }

        _url = buff.ReadTLString();
        _bot = (InputUser)factory.Read(buff.ReadInt32(true), ref buff);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}