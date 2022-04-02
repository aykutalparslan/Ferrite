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

namespace Ferrite.TL.layer139.messages;
public class TranslateText : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public TranslateText(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => 617508334;
    public ReadOnlySequence<byte> TLBytes
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
                writer.Write(_peer.TLBytes, false);
            }

            if (_flags[0])
            {
                writer.WriteInt32(_msgId, true);
            }

            if (_flags[1])
            {
                writer.WriteTLString(_text);
            }

            if (_flags[2])
            {
                writer.WriteTLString(_fromLang);
            }

            writer.WriteTLString(_toLang);
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

    private InputPeer _peer;
    public InputPeer Peer
    {
        get => _peer;
        set
        {
            serialized = false;
            _flags[0] = true;
            _peer = value;
        }
    }

    private int _msgId;
    public int MsgId
    {
        get => _msgId;
        set
        {
            serialized = false;
            _flags[0] = true;
            _msgId = value;
        }
    }

    private string _text;
    public string Text
    {
        get => _text;
        set
        {
            serialized = false;
            _flags[1] = true;
            _text = value;
        }
    }

    private string _fromLang;
    public string FromLang
    {
        get => _fromLang;
        set
        {
            serialized = false;
            _flags[2] = true;
            _fromLang = value;
        }
    }

    private string _toLang;
    public string ToLang
    {
        get => _toLang;
        set
        {
            serialized = false;
            _toLang = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        if (_flags[0])
        {
            _peer = (InputPeer)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[0])
        {
            _msgId = buff.ReadInt32(true);
        }

        if (_flags[1])
        {
            _text = buff.ReadTLString();
        }

        if (_flags[2])
        {
            _fromLang = buff.ReadTLString();
        }

        _toLang = buff.ReadTLString();
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}