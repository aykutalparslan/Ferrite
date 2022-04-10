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
using System.Collections.Concurrent;
using Autofac;
using Ferrite.TL;

namespace Ferrite.Core;

public class IncomingMessageHandler: IProcessorManager
{
    private readonly List<IProcessor> _processors;
    private readonly ILifetimeScope _scope;
    public IncomingMessageHandler(ILifetimeScope scope)
    {
        _scope = scope;
        _processors = new List<IProcessor>();
        _processors.Add(_scope.Resolve<AuthKeyProcessor>());
        _processors.Add(_scope.Resolve<MTProtoRequestProcessor>());
    }

    public async Task Process(object? sender, ITLObject input, TLExecutionContext ctx)
    {
        if (_processors.Count == 0)
        {
            return;
        }
        Queue<ITLObject> tobeProcessed = new();
        tobeProcessed.Enqueue(input);
        int idx = 0;
        var processor = _processors[idx];

        do {
            int limit = tobeProcessed.Count;
            for (int i = 0; i < limit; i++)
            {
                var curr = tobeProcessed.Dequeue();
                await processor.Process(sender, curr, tobeProcessed, ctx);
            }
            if (++idx < _processors.Count)
            {
                processor = _processors[idx];
            }
        } while (tobeProcessed.Count > 0 && processor != null) ;
    }
}

