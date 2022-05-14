/*
 *   Project Ferrite is an Implementation of the Telegram Server API
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

namespace Ferrite.TL.mtproto;
public class PQInnerDataTemp : ITLObject
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PQInnerDataTemp(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => 1013613780;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLBytes(_pq);
            writer.WriteTLBytes(_p);
            writer.WriteTLBytes(_q);
            writer.Write(_nonce.TLBytes, false);
            writer.Write(_serverNonce.TLBytes, false);
            writer.Write(_newNonce.TLBytes, false);
            writer.WriteInt32(_expiresIn, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private byte[] _pq;
    public byte[] Pq
    {
        get => _pq;
        set
        {
            serialized = false;
            _pq = value;
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

    private byte[] _q;
    public byte[] Q
    {
        get => _q;
        set
        {
            serialized = false;
            _q = value;
        }
    }

    private Int128 _nonce;
    public Int128 Nonce
    {
        get => _nonce;
        set
        {
            serialized = false;
            _nonce = value;
        }
    }

    private Int128 _serverNonce;
    public Int128 ServerNonce
    {
        get => _serverNonce;
        set
        {
            serialized = false;
            _serverNonce = value;
        }
    }

    private Int256 _newNonce;
    public Int256 NewNonce
    {
        get => _newNonce;
        set
        {
            serialized = false;
            _newNonce = value;
        }
    }

    private int _expiresIn;
    public int ExpiresIn
    {
        get => _expiresIn;
        set
        {
            serialized = false;
            _expiresIn = value;
        }
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _pq = buff.ReadTLBytes().ToArray();
        _p = buff.ReadTLBytes().ToArray();
        _q = buff.ReadTLBytes().ToArray();
        _nonce = factory.Read<Int128>(ref buff);
        _serverNonce = factory.Read<Int128>(ref buff);
        _newNonce = factory.Read<Int256>(ref buff);
        _expiresIn = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}