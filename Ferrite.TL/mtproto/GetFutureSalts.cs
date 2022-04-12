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
using Ferrite.Services;
using Ferrite.Utils;

namespace Ferrite.TL.mtproto;
public class GetFutureSalts : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IMTProtoService protoService;
    private readonly IMTProtoTime _time;
    private bool serialized = false;
    public GetFutureSalts(ITLObjectFactory objectFactory, IMTProtoService mTProtoService, IMTProtoTime time)
    {
        factory = objectFactory;
        protoService = mTProtoService;
        _time = time;
    }

    public int Constructor => -1188971260;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt32(num, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private int num;
    public int Num
    {
        get => num;
        set
        {
            serialized = false;
            num = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        FutureSalts futureSalts = factory.Resolve<FutureSalts>();
        futureSalts.ReqMsgId = ctx.MessageId;
        futureSalts.Now = (int)_time.GetUnixTimeInSeconds();
        var salts = await protoService.GetServerSaltsAsync(ctx.AuthKeyId, num);
        futureSalts.Salts = factory.Resolve<VectorBare<FutureSalt>>();
        foreach (var salt in salts)
        {
            var futureSalt = factory.Resolve<FutureSalt>();
            futureSalt.Salt = salt.Salt;
            futureSalt.ValidSince = (int)salt.ValidSince;
            futureSalts.Salts.Add(futureSalt);
        }
        var rpcResult = factory.Resolve<RpcResult>();
        rpcResult.ReqMsgId = ctx.MessageId;
        rpcResult.Result = futureSalts;
        return rpcResult;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        num = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}