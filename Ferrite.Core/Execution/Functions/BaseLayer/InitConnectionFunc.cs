// 
// Project Ferrite is an Implementation of the Telegram Server API
// Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 

using System.Text;
using DotNext.Buffers;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.slim;
using Ferrite.TL.slim.dto;

namespace Ferrite.Core.Execution.Functions.BaseLayer;

public class InitConnectionFunc : ITLFunction
{
    public IExecutionEngine? ExecutionEngine { get; set; }
    private readonly IRandomGenerator _random;
    private readonly IAuthService _auth;

    public InitConnectionFunc(IRandomGenerator random, IAuthService auth)
    {
        _random = random;
        _auth = auth;
    }

    public async ValueTask<TLBytes?> Process(TLBytes q, TLExecutionContext ctx)
    {
        var info = CreateAppInfo(q, ctx);
        await _auth.SaveAppInfo(info);
        if (ExecutionEngine != null) return await ExecutionEngine.Invoke(GetQuery(q), ctx);
        return null;
    }

    private static TLBytes GetQuery(TLBytes q)
    {
        TL.slim.layer150.InitConnection request = new(q.AsSpan());
        var queryMemory = UnmanagedMemoryPool<byte>.Shared.Rent(request.Query.Length);
        request.Query.CopyTo(queryMemory.Memory.Span);
        TLBytes query = new(queryMemory, 0, request.Query.Length);
        return query;
    }

    private TLAppInfo CreateAppInfo(TLBytes q, TLExecutionContext ctx)
    {
        var request = (TL.slim.layer150.InitConnection)q;
        return AppInfo.Builder()
            .Hash(_random.NextLong())
            .ApiId(request.ApiId)
            .AppVersion(request.AppVersion)
            .AuthKeyId(ctx.CurrentAuthKeyId)
            .DeviceModel(request.DeviceModel)
            .Ip(Encoding.UTF8.GetBytes(ctx.IP))
            .LangCode(request.LangCode)
            .LangPack(request.LangPack)
            .SystemLangCode(request.LangCode)
            .SystemVersion(request.SystemVersion)
            .Build();
    }
}