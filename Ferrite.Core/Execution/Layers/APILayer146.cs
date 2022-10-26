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

using System.Collections.Immutable;
using Autofac;
using Ferrite.Core.Execution.Functions;
using Ferrite.Core.Execution.Functions.Layer146;
using Ferrite.Crypto;
using Ferrite.Services;
using Ferrite.TL.slim;
using Ferrite.Utils;

namespace Ferrite.Core.Execution.Layers;

public class ApiLayer146 : IApiLayer
{
    private readonly ImmutableDictionary<int, object> _handlers;

    public ApiLayer146(IComponentContext context, IExecutionEngine engine)
    {
        _handlers = ImmutableDictionary<int, object>.Empty
            .Add(Constructors.req_pq_multi, context.Resolve<ReqPQ>())
            .Add(Constructors.req_DH_params, context.Resolve<ReqDhParams>())
            .Add(Constructors.set_client_DH_params, context.Resolve<SetClientDhParams>())
            .Add(Constructors.initConnection, 
                new InitConnection(context.Resolve<IRandomGenerator>(), 
                    context.Resolve<IAuthService>(), 
                    engine));//autofac does not support circular constructor dependencies
    }
    public ITLFunction? GetFunction(int constructor)
    {
        if(!_handlers.ContainsKey(constructor)) return null;
        return (ITLFunction)_handlers[constructor];
    }
}