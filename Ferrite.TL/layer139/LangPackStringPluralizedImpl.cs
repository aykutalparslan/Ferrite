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
public class LangPackStringPluralizedImpl : LangPackString
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public LangPackStringPluralizedImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 1816636575;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteTLString(_key);
            if (_flags[0])
            {
                writer.WriteTLString(_zeroValue);
            }

            if (_flags[1])
            {
                writer.WriteTLString(_oneValue);
            }

            if (_flags[2])
            {
                writer.WriteTLString(_twoValue);
            }

            if (_flags[3])
            {
                writer.WriteTLString(_fewValue);
            }

            if (_flags[4])
            {
                writer.WriteTLString(_manyValue);
            }

            writer.WriteTLString(_otherValue);
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

    private string _key;
    public string Key
    {
        get => _key;
        set
        {
            serialized = false;
            _key = value;
        }
    }

    private string _zeroValue;
    public string ZeroValue
    {
        get => _zeroValue;
        set
        {
            serialized = false;
            _flags[0] = true;
            _zeroValue = value;
        }
    }

    private string _oneValue;
    public string OneValue
    {
        get => _oneValue;
        set
        {
            serialized = false;
            _flags[1] = true;
            _oneValue = value;
        }
    }

    private string _twoValue;
    public string TwoValue
    {
        get => _twoValue;
        set
        {
            serialized = false;
            _flags[2] = true;
            _twoValue = value;
        }
    }

    private string _fewValue;
    public string FewValue
    {
        get => _fewValue;
        set
        {
            serialized = false;
            _flags[3] = true;
            _fewValue = value;
        }
    }

    private string _manyValue;
    public string ManyValue
    {
        get => _manyValue;
        set
        {
            serialized = false;
            _flags[4] = true;
            _manyValue = value;
        }
    }

    private string _otherValue;
    public string OtherValue
    {
        get => _otherValue;
        set
        {
            serialized = false;
            _otherValue = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _key = buff.ReadTLString();
        if (_flags[0])
        {
            _zeroValue = buff.ReadTLString();
        }

        if (_flags[1])
        {
            _oneValue = buff.ReadTLString();
        }

        if (_flags[2])
        {
            _twoValue = buff.ReadTLString();
        }

        if (_flags[3])
        {
            _fewValue = buff.ReadTLString();
        }

        if (_flags[4])
        {
            _manyValue = buff.ReadTLString();
        }

        _otherValue = buff.ReadTLString();
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}