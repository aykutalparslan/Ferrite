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
using Ferrite.TL.slim.layer146;

namespace Ferrite.Core.Execution.Functions.Layer146;

public class InitConnection : ITLFunction
{
    public IExecutionEngine ExecutionEngine { get; set; }
    private readonly IRandomGenerator _random;
    private readonly IAuthService _auth;

    public InitConnection(IRandomGenerator random, IAuthService auth)
    {
        _random = random;
        _auth = auth;
    }

    public async ValueTask<TLBytes?> Process(TLBytes q, TLExecutionContext ctx)
    {
        var info = CreateAppInfo(q, ctx);
        await _auth.SaveAppInfo(info);
        return await ExecutionEngine.Invoke(GetQuery(q), ctx);
    }

    private static TLBytes GetQuery(TLBytes q)
    {
        initConnection request = new(q.AsSpan());
        var queryMemory = UnmanagedMemoryPool<byte>.Shared.Rent(request.query.Length);
        request.query.CopyTo(queryMemory.Memory.Span);
        TLBytes query = new(queryMemory, 0, request.query.Length);
        return query;
    }

    private AppInfoDTO CreateAppInfo(TLBytes q, TLExecutionContext ctx)
    {
        initConnection request = new(q.AsSpan());
        return new AppInfoDTO()
        {
            Hash = _random.NextLong(),
            ApiId = request.api_id,
            AppVersion = Encoding.UTF8.GetString(request.app_version),
            AuthKeyId = ctx.CurrentAuthKeyId,
            DeviceModel = Encoding.UTF8.GetString(request.device_model),
            IP = ctx.IP,
            LangCode = Encoding.UTF8.GetString(request.lang_code),
            LangPack = Encoding.UTF8.GetString(request.lang_pack),
            SystemLangCode = Encoding.UTF8.GetString(request.lang_code),
            SystemVersion = Encoding.UTF8.GetString(request.system_version)
        };
    }
}