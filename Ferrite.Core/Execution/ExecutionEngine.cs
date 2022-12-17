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
using Autofac.Features.Indexed;
using Ferrite.Core.Execution.Functions;
using Ferrite.TL;
using Ferrite.TL.slim;
using Ferrite.Utils;

namespace Ferrite.Core.Execution;

public class ExecutionEngine : IExecutionEngine
{
    private readonly IIndex<FunctionKey, ITLFunction> _functions;
    private readonly ILogger _log;

    public ExecutionEngine(IIndex<FunctionKey, ITLFunction> functions, ILogger log)
    {
        _functions = functions;
        _log = log;
    }

    public async ValueTask<TLBytes?> Invoke(TLBytes rpc, TLExecutionContext ctx, int layer = IExecutionEngine.DefaultLayer)
    {
        try
        {
            var func = _functions[new FunctionKey(layer, rpc.Constructor)];
            return await func.Process(rpc, ctx);
        }
        catch (Exception e)
        {
            _log.Error(e, $"#{rpc.Constructor.ToString("x")} is not registered for layer {layer}");
        }

        return null;
    }

    public bool IsImplemented(int constructor, int layer = IExecutionEngine.DefaultLayer)
    {
        try
        {
            var func = _functions[new FunctionKey(layer, constructor)];
            return true;
        }
        catch (Exception e)
        {
            _log.Error(e, $"#{constructor.ToString("x")} is not registered for layer {layer}");
        }

        return false;
    }
}