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
public class LangPackDifferenceImpl : LangPackDifference
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public LangPackDifferenceImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -209337866;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLString(_langCode);
            writer.WriteInt32(_fromVersion, true);
            writer.WriteInt32(_version, true);
            writer.Write(_strings.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private string _langCode;
    public string LangCode
    {
        get => _langCode;
        set
        {
            serialized = false;
            _langCode = value;
        }
    }

    private int _fromVersion;
    public int FromVersion
    {
        get => _fromVersion;
        set
        {
            serialized = false;
            _fromVersion = value;
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

    private Vector<LangPackString> _strings;
    public Vector<LangPackString> Strings
    {
        get => _strings;
        set
        {
            serialized = false;
            _strings = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _langCode = buff.ReadTLString();
        _fromVersion = buff.ReadInt32(true);
        _version = buff.ReadInt32(true);
        buff.Skip(4); _strings  =  factory . Read < Vector < LangPackString > > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}