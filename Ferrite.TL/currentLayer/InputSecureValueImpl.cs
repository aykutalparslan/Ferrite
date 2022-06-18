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
public class InputSecureValueImpl : InputSecureValue
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public InputSecureValueImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -618540889;
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

    private InputSecureFile _frontSide;
    public InputSecureFile FrontSide
    {
        get => _frontSide;
        set
        {
            serialized = false;
            _flags[1] = true;
            _frontSide = value;
        }
    }

    private InputSecureFile _reverseSide;
    public InputSecureFile ReverseSide
    {
        get => _reverseSide;
        set
        {
            serialized = false;
            _flags[2] = true;
            _reverseSide = value;
        }
    }

    private InputSecureFile _selfie;
    public InputSecureFile Selfie
    {
        get => _selfie;
        set
        {
            serialized = false;
            _flags[3] = true;
            _selfie = value;
        }
    }

    private Vector<InputSecureFile> _translation;
    public Vector<InputSecureFile> Translation
    {
        get => _translation;
        set
        {
            serialized = false;
            _flags[6] = true;
            _translation = value;
        }
    }

    private Vector<InputSecureFile> _files;
    public Vector<InputSecureFile> Files
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
            _frontSide = (InputSecureFile)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[2])
        {
            _reverseSide = (InputSecureFile)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[3])
        {
            _selfie = (InputSecureFile)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[6])
        {
            buff.Skip(4);
            _translation = factory.Read<Vector<InputSecureFile>>(ref buff);
        }

        if (_flags[4])
        {
            buff.Skip(4);
            _files = factory.Read<Vector<InputSecureFile>>(ref buff);
        }

        if (_flags[5])
        {
            _plainData = (SecurePlainData)factory.Read(buff.ReadInt32(true), ref buff);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}