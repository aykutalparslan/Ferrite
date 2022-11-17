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

using Ferrite.TL;
using Ferrite.TL.slim;

namespace Ferrite.Core.Execution;

public interface IExecutionEngine
{
    /// <summary>
    /// Invokes a Function with the specified layer.
    /// Function (functional combinator) is a combinator which may be computed (reduced)
    /// on condition that the requisite number of arguments of requisite types are provided.
    /// The result of the computation is an expression consisting of constructors
    /// and base type values only.
    /// </summary>
    /// <param name="rpc">Serialized functional combinator.</param>
    /// <param name="layer">Layer with which the function should be computed.</param>
    /// <returns>TL Serialized result of the computation.</returns>
    public ValueTask<TLBytes?> Invoke(TLBytes rpc, TLExecutionContext ctx, int layer = 148);
    public bool IsImplemented(int constructor, int layer = 148);
}