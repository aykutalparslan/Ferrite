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
public class DocumentAttributeAudioImpl : DocumentAttribute
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public DocumentAttributeAudioImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1739392570;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt32(_duration, true);
            if (_flags[0])
            {
                writer.WriteTLString(_title);
            }

            if (_flags[1])
            {
                writer.WriteTLString(_performer);
            }

            if (_flags[2])
            {
                writer.WriteTLBytes(_waveform);
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

    public bool Voice
    {
        get => _flags[10];
        set
        {
            serialized = false;
            _flags[10] = value;
        }
    }

    private int _duration;
    public int Duration
    {
        get => _duration;
        set
        {
            serialized = false;
            _duration = value;
        }
    }

    private string _title;
    public string Title
    {
        get => _title;
        set
        {
            serialized = false;
            _flags[0] = true;
            _title = value;
        }
    }

    private string _performer;
    public string Performer
    {
        get => _performer;
        set
        {
            serialized = false;
            _flags[1] = true;
            _performer = value;
        }
    }

    private byte[] _waveform;
    public byte[] Waveform
    {
        get => _waveform;
        set
        {
            serialized = false;
            _flags[2] = true;
            _waveform = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _duration = buff.ReadInt32(true);
        if (_flags[0])
        {
            _title = buff.ReadTLString();
        }

        if (_flags[1])
        {
            _performer = buff.ReadTLString();
        }

        if (_flags[2])
        {
            _waveform = buff.ReadTLBytes().ToArray();
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}