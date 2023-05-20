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

using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.slim;
using Ferrite.TL.slim.baseLayer.account;

namespace Ferrite.Core.Execution.Functions.BaseLayer.Account;

public class UpdateDeviceLockedFunc : ITLFunction
{
    private readonly IAccountService _account;

    public UpdateDeviceLockedFunc(IAccountService account)
    {
        _account = account;
    }
    public async ValueTask<TLBytes?> Process(TLBytes q, TLExecutionContext ctx)
    {
        using var unregister = await _account.UpdateDeviceLocked(ctx.CurrentAuthKeyId, ((UpdateDeviceLocked)q).Period);
        var rpcResult = RpcResultGenerator.Generate(unregister, ctx.MessageId);
        return rpcResult;
    }
}