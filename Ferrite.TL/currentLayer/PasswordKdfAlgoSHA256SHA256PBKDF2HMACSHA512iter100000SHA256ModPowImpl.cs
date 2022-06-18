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
public class PasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPowImpl : PasswordKdfAlgo
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPowImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 982592842;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLBytes(_salt1);
            writer.WriteTLBytes(_salt2);
            writer.WriteInt32(_g, true);
            writer.WriteTLBytes(_p);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private byte[] _salt1;
    public byte[] Salt1
    {
        get => _salt1;
        set
        {
            serialized = false;
            _salt1 = value;
        }
    }

    private byte[] _salt2;
    public byte[] Salt2
    {
        get => _salt2;
        set
        {
            serialized = false;
            _salt2 = value;
        }
    }

    private int _g;
    public int G
    {
        get => _g;
        set
        {
            serialized = false;
            _g = value;
        }
    }

    private byte[] _p;
    public byte[] P
    {
        get => _p;
        set
        {
            serialized = false;
            _p = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _salt1 = buff.ReadTLBytes().ToArray();
        _salt2 = buff.ReadTLBytes().ToArray();
        _g = buff.ReadInt32(true);
        _p = buff.ReadTLBytes().ToArray();
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}