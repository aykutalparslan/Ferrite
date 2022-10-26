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
using Ferrite.Core.Execution.Layers;
using Ferrite.TL;
using Ferrite.TL.slim;

namespace Ferrite.Core.Execution;

public class ExecutionEngine : IExecutionEngine
{
    private readonly ImmutableDictionary<int, object> _apiLayers;

    public ExecutionEngine(IComponentContext context)
    {
        _apiLayers = ImmutableDictionary<int, object>.Empty
            .Add(146, new ApiLayer146(context, this));
    }
    public async ValueTask<TLBytes?> Invoke(TLBytes rpc, TLExecutionContext ctx, int layer = 146)
    {
        if (_apiLayers.ContainsKey(layer))
        {
            var func = ((IApiLayer)_apiLayers[layer]).GetFunction(rpc.Constructor);
            if (func != null) return await func.Process(rpc, ctx);
        }

        return null;
    }
}