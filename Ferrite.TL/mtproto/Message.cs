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
public class Message : ITLObject
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public Message(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => 1538843921;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(msgId, true);
            writer.WriteInt32(seqno, true);
            writer.WriteInt32(bytes, true);
            writer.Write(body.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private long msgId;
    public long MsgId
    {
        get => msgId;
        set
        {
            serialized = false;
            msgId = value;
        }
    }

    private int seqno;
    public int Seqno
    {
        get => seqno;
        set
        {
            serialized = false;
            seqno = value;
        }
    }

    private int bytes;
    public int Bytes
    {
        get => bytes;
        set
        {
            serialized = false;
            bytes = value;
        }
    }

    private ITLObject body;
    public ITLObject Body
    {
        get => body;
        set
        {
            serialized = false;
            body = value;
        }
    }

    public bool IsMethod => false;
    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        msgId = buff.ReadInt64(true);
        seqno = buff.ReadInt32(true);
        bytes = buff.ReadInt32(true);
        body = factory.Read(buff.ReadInt32(true), ref buff);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}