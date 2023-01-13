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
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.slim;
using Ferrite.TL.slim.layer150.account;

namespace Ferrite.Core.Execution.Functions.BaseLayer.Account;

public class UpdateUsernameFunc : ITLFunction
{
    private readonly IAccountService _account;

    public UpdateUsernameFunc(IAccountService account)
    {
        _account = account;
    }
    public async ValueTask<TLBytes?> Process(TLBytes q, TLExecutionContext ctx)
    {
        using var result = await _account.UpdateUsername(ctx.CurrentAuthKeyId,
            Encoding.UTF8.GetString(((AccountCheckUsername)q).Username));
        var rpcResult = RpcResultGenerator.Generate(result, ctx.MessageId);
        return rpcResult;
    }
}