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
public class SecureValueErrorTranslationFileImpl : SecureValueError
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public SecureValueErrorTranslationFileImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1592506512;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_type.TLBytes, false);
            writer.WriteTLBytes(_fileHash);
            writer.WriteTLString(_text);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private SecureValueType _type;
    public SecureValueType Type
    {
        get => _type;
        set
        {
            serialized = false;
            _type = value;
        }
    }

    private byte[] _fileHash;
    public byte[] FileHash
    {
        get => _fileHash;
        set
        {
            serialized = false;
            _fileHash = value;
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

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _type = (SecureValueType)factory.Read(buff.ReadInt32(true), ref buff);
        _fileHash = buff.ReadTLBytes().ToArray();
        _text = buff.ReadTLString();
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}