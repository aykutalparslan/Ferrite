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

using DotNext.Buffers;
using Ferrite.TL;
using Ferrite.TL.slim;

namespace Ferrite.Core.Execution.Functions.Layer148;

public class InvokeWithLayerFunc : ITLFunction
{
    public IExecutionEngine? ExecutionEngine { get; set; }
    public async ValueTask<TLBytes?> Process(TLBytes q, TLExecutionContext ctx)
    {
        var (query, layer) = GetQuery(q);
        if (ExecutionEngine != null) return await ExecutionEngine.Invoke(query, ctx, layer);
        return null;
    }
    private static ValueTuple<TLBytes, int> GetQuery(TLBytes q)
    {
        TL.slim.layer148.InvokeWithLayer request = new(q.AsSpan());
        var queryMemory = UnmanagedMemoryPool<byte>.Shared.Rent(request.Query.Length);
        request.Query.CopyTo(queryMemory.Memory.Span);
        TLBytes query = new(queryMemory, 0, request.Query.Length);
        return (query, request.Layer);
    }
}