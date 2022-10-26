//
//  Project Ferrite is an Implementation Telegram Server API
//  Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Affero General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Affero General Public License for more details.
//
//  You should have received a copy of the GNU Affero General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Autofac;
using Ferrite.TL;
using Ferrite.TL.slim;
using Ferrite.Utils;

namespace Ferrite.Core.RequestChain;

public class DefaultChain: ITLHandler
{
    private readonly ILinkedHandler _first;
    private readonly ILogger _log;
    public DefaultChain(ILifetimeScope scope, ILogger log)
    {
        _log = log;
        _first = scope.Resolve<AuthKeyProcessor>();
        _first.SetNext(scope.Resolve<MsgContainerProcessor>())
            .SetNext(scope.Resolve<ServiceMessagesProcessor>())
            .SetNext(scope.Resolve<GZipProcessor>())
            .SetNext(scope.Resolve<AuthorizationProcessor>())
            .SetNext(scope.Resolve<MTProtoRequestProcessor>());
    }

    public async ValueTask Process(object? sender, ITLObject input, TLExecutionContext ctx)
    {
        await _first.Process(sender, input, ctx);
    }

    public async ValueTask Process(object? sender, TLBytes input, TLExecutionContext ctx)
    {
        await _first.Process(sender, input, ctx);
    }
}

