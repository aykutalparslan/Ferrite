﻿/*
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
public class InvokeWithLayer : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly ILogger _log;
    private bool serialized = false;
    public InvokeWithLayer(ITLObjectFactory objectFactory, ILogger logger)
    {
        factory = objectFactory;
        _log = logger;
    }

    public int Constructor => -627372787;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt32(_layer, true);
            writer.Write(_query.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private int _layer;
    public int Layer
    {
        get => _layer;
        set
        {
            serialized = false;
            _layer = value;
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
        if(_query is ITLMethod medhod)
        {
            _log.Information($"Invoke {medhod} with Layer {_layer} MessageId: {ctx.MessageId} AuthKeyId:{ctx.AuthKeyId}");
            return await medhod.ExecuteAsync(ctx);
        }
        var inner = new InvalidTLMethodException("'query' could not be cast to ITLMethod");
        var ex = new TLExecutionException(String.Format("Invocation failed for Layer {0}", _layer), inner);
        _log.Error(ex, ex.Message);
        throw ex;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _layer = buff.ReadInt32(true);
        _query = factory.Read(buff.ReadInt32(true), ref buff);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}