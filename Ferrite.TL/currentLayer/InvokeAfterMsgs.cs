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
using Ferrite.TL.Exceptions;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer;
public class InvokeAfterMsgs : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly ILogger _log;
    private bool serialized = false;
    public InvokeAfterMsgs(ITLObjectFactory objectFactory, ILogger log)
    {
        factory = objectFactory;
        _log = log;
    }

    public int Constructor => 1036301552;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_msgIds.TLBytes, false);
            writer.Write(_query.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private VectorOfLong _msgIds;
    public VectorOfLong MsgIds
    {
        get => _msgIds;
        set
        {
            serialized = false;
            _msgIds = value;
        }
    }

    private ITLObject _query;
    public ITLObject Query
    {
        get => _query;
        set
        {
            serialized = false;
            _query = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        if (_query is ITLMethod medhod)
        {
            _log.Information($"Invoke {medhod.ToString()} after msg.");
            //TODO: Investigate if we actually can invoke after the msgs?
            return await medhod.ExecuteAsync(ctx);
        }
        var inner = new InvalidTLMethodException("'query' could not be cast to ITLMethod");
        var ex = new TLExecutionException("Invocation failed for invokeAfterMsgs", inner);
        _log.Error(ex, ex.Message);
        throw ex;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        buff.Skip(4); _msgIds  =  factory . Read < VectorOfLong > ( ref  buff ) ; 
        _query = factory.Read(buff.ReadInt32(true), ref buff);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}