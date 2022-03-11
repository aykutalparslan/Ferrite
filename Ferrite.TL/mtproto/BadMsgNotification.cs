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
public class BadMsgNotification : ITLObject
{
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private ITLObjectFactory factory;
    private bool serialized = false;
    public BadMsgNotification(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -1477445615;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(badMsgId, true);
            writer.WriteInt32(badMsgSeqno, true);
            writer.WriteInt32(errorCode, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private long badMsgId;
    public long BadMsgId
    {
        get => badMsgId;
        set
        {
            serialized = false;
            badMsgId = value;
        }
    }

    private int badMsgSeqno;
    public int BadMsgSeqno
    {
        get => badMsgSeqno;
        set
        {
            serialized = false;
            badMsgSeqno = value;
        }
    }

    private int errorCode;
    public int ErrorCode
    {
        get => errorCode;
        set
        {
            serialized = false;
            errorCode = value;
        }
    }

    public bool IsMethod => false;
    public ITLObject Execute(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        badMsgId = buff.ReadInt64(true);
        badMsgSeqno = buff.ReadInt32(true);
        errorCode = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}