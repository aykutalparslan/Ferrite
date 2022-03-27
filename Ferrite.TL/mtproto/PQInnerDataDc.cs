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

namespace Ferrite.TL.mtproto;
public class PQInnerDataDc : ITLObject
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public int Constructor => -1443537003;
    public PQInnerDataDc(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLBytes(pq);
            writer.WriteTLBytes(p);
            writer.WriteTLBytes(q);
            writer.Write(nonce.TLBytes, false);
            writer.Write(serverNonce.TLBytes, false);
            writer.Write(newNonce.TLBytes, false);
            writer.WriteInt32(dc, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private byte[] pq;
    public byte[] Pq
    {
        get => pq;
        set
        {
            serialized = false;
            pq = value;
        }
    }

    private byte[] p;
    public byte[] P
    {
        get => p;
        set
        {
            serialized = false;
            p = value;
        }
    }

    private byte[] q;
    public byte[] Q
    {
        get => q;
        set
        {
            serialized = false;
            q = value;
        }
    }

    private Int128 nonce;
    public Int128 Nonce
    {
        get => nonce;
        set
        {
            serialized = false;
            nonce = value;
        }
    }

    private Int128 serverNonce;
    public Int128 ServerNonce
    {
        get => serverNonce;
        set
        {
            serialized = false;
            serverNonce = value;
        }
    }

    private Int256 newNonce;
    public Int256 NewNonce
    {
        get => newNonce;
        set
        {
            serialized = false;
            newNonce = value;
        }
    }

    private int dc;
    public int Dc
    {
        get => dc;
        set
        {
            serialized = false;
            dc = value;
        }
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        pq = buff.ReadTLBytes().ToArray();
        p = buff.ReadTLBytes().ToArray();
        q = buff.ReadTLBytes().ToArray();
        nonce = factory.Read<Int128>(ref buff);
        serverNonce = factory.Read<Int128>(ref buff);
        newNonce = factory.Read<Int256>(ref buff);
        dc = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}