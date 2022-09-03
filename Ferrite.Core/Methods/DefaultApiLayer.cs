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

using Autofac;
using Ferrite.TL.slim;
using Ferrite.TL.slim.mtproto;

namespace Ferrite.Core.Methods;

public class DefaultApiLayer : IApiLayer
{
    private Dictionary<int, object> _handlers;

    public DefaultApiLayer(IComponentContext context)
    {
        _handlers = new Dictionary<int, object>
        {
            { unchecked((int)0xbe7e8ef1), context.Resolve<ReqPQHandler>() },
            { unchecked((int)0xd712e4be), context.Resolve<ReqDhParamsHandler>() },
            { unchecked((int)0xf5045f1f), context.Resolve<SetClientDhParamsHandler>() }
        };
    }
    public IQueryHandler? GetHandler(int constructor)
    {
        return (IQueryHandler?)_handlers[constructor];
    }
}