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
public class SecureValueImpl : SecureValue
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public SecureValueImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 411017418;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.Write(_type.TLBytes, false);
            if (_flags[0])
            {
                writer.Write(_data.TLBytes, false);
            }

            if (_flags[1])
            {
                writer.Write(_frontSide.TLBytes, false);
            }

            if (_flags[2])
            {
                writer.Write(_reverseSide.TLBytes, false);
            }

            if (_flags[3])
            {
                writer.Write(_selfie.TLBytes, false);
            }

            if (_flags[6])
            {
                writer.Write(_translation.TLBytes, false);
            }

            if (_flags[4])
            {
                writer.Write(_files.TLBytes, false);
            }

            if (_flags[5])
            {
                writer.Write(_plainData.TLBytes, false);
            }

            writer.WriteTLBytes(_hash);
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

    private SecureData _data;
    public SecureData Data
    {
        get => _data;
        set
        {
            serialized = false;
            _flags[0] = true;
            _data = value;
        }
    }

    private SecureFile _frontSide;
    public SecureFile FrontSide
    {
        get => _frontSide;
        set
        {
            serialized = false;
            _flags[1] = true;
            _frontSide = value;
        }
    }

    private SecureFile _reverseSide;
    public SecureFile ReverseSide
    {
        get => _reverseSide;
        set
        {
            serialized = false;
            _flags[2] = true;
            _reverseSide = value;
        }
    }

    private SecureFile _selfie;
    public SecureFile Selfie
    {
        get => _selfie;
        set
        {
            serialized = false;
            _flags[3] = true;
            _selfie = value;
        }
    }

    private Vector<SecureFile> _translation;
    public Vector<SecureFile> Translation
    {
        get => _translation;
        set
        {
            serialized = false;
            _flags[6] = true;
            _translation = value;
        }
    }

    private Vector<SecureFile> _files;
    public Vector<SecureFile> Files
    {
        get => _files;
        set
        {
            serialized = false;
            _flags[4] = true;
            _files = value;
        }
    }

    private SecurePlainData _plainData;
    public SecurePlainData PlainData
    {
        get => _plainData;
        set
        {
            serialized = false;
            _flags[5] = true;
            _plainData = value;
        }
    }

    private byte[] _hash;
    public byte[] Hash
    {
        get => _hash;
        set
        {
            serialized = false;
            _hash = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _type = (SecureValueType)factory.Read(buff.ReadInt32(true), ref buff);
        if (_flags[0])
        {
            _data = (SecureData)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[1])
        {
            _frontSide = (SecureFile)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[2])
        {
            _reverseSide = (SecureFile)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[3])
        {
            _selfie = (SecureFile)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[6])
        {
            buff.Skip(4);
            _translation = factory.Read<Vector<SecureFile>>(ref buff);
        }

        if (_flags[4])
        {
            buff.Skip(4);
            _files = factory.Read<Vector<SecureFile>>(ref buff);
        }

        if (_flags[5])
        {
            _plainData = (SecurePlainData)factory.Read(buff.ReadInt32(true), ref buff);
        }

        _hash = buff.ReadTLBytes().ToArray();
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}