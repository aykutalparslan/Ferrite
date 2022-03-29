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

namespace Ferrite.TL.layer139.stats;
public class GetMessagePublicForwards : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public GetMessagePublicForwards(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => 1445996571;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_channel.TLBytes, false);
            writer.WriteInt32(_msgId, true);
            writer.WriteInt32(_offsetRate, true);
            writer.Write(_offsetPeer.TLBytes, false);
            writer.WriteInt32(_offsetId, true);
            writer.WriteInt32(_limit, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private InputChannel _channel;
    public InputChannel Channel
    {
        get => _channel;
        set
        {
            serialized = false;
            _channel = value;
        }
    }

    private int _msgId;
    public int MsgId
    {
        get => _msgId;
        set
        {
            serialized = false;
            _msgId = value;
        }
    }

    private int _offsetRate;
    public int OffsetRate
    {
        get => _offsetRate;
        set
        {
            serialized = false;
            _offsetRate = value;
        }
    }

    private InputPeer _offsetPeer;
    public InputPeer OffsetPeer
    {
        get => _offsetPeer;
        set
        {
            serialized = false;
            _offsetPeer = value;
        }
    }

    private int _offsetId;
    public int OffsetId
    {
        get => _offsetId;
        set
        {
            serialized = false;
            _offsetId = value;
        }
    }

    private int _limit;
    public int Limit
    {
        get => _limit;
        set
        {
            serialized = false;
            _limit = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        buff.Skip(4); _channel  =  factory . Read < InputChannel > ( ref  buff ) ; 
        _msgId = buff.ReadInt32(true);
        _offsetRate = buff.ReadInt32(true);
        buff.Skip(4); _offsetPeer  =  factory . Read < InputPeer > ( ref  buff ) ; 
        _offsetId = buff.ReadInt32(true);
        _limit = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}