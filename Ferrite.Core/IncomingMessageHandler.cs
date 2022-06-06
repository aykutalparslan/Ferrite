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
using Ferrite.Utils;

namespace Ferrite.Core;

public class IncomingMessageHandler: IProcessorManager
{
    private readonly ImmutableList<IProcessor> _processors;
    private readonly ILifetimeScope _scope;
    private readonly ILogger _log;
    public IncomingMessageHandler(ILifetimeScope scope, ILogger log)
    {
        _scope = scope;
        _log = log;
        _processors = ImmutableList<IProcessor>.Empty
            .Add(_scope.Resolve<AuthKeyProcessor>())
            .Add(_scope.Resolve<MsgContainerProcessor>())
            .Add(_scope.Resolve<ServiceMessagesProcessor>())
            .Add(_scope.Resolve<AuthorizationProcessor>())
            .Add(_scope.Resolve<MTProtoRequestProcessor>());
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
                try
                {
                    await processor.Process(sender, curr, tobeProcessed, ctx);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, ex.Message);
                }
            }
            if (++idx < _processors.Count)
            {
                processor = _processors[idx];
            }
        } while (tobeProcessed.Count > 0 && processor != null);
    }

    public async Task Process(object? sender, IMemoryOwner<byte> input, TLExecutionContext ctx)
    {
        if (_processors.Count == 0)
        {
            return;
        }
        Queue<IMemoryOwner<byte>> tobeProcessed = new();
        tobeProcessed.Enqueue(input);
        int idx = 0;
        var processor = _processors[idx];

        do {
            int limit = tobeProcessed.Count;
            for (int i = 0; i < limit; i++)
            {
                var curr = tobeProcessed.Dequeue();
                try
                {
                    await processor.Process(sender, curr, tobeProcessed, ctx);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, ex.Message);
                }
            }
            if (++idx < _processors.Count)
            {
                processor = _processors[idx];
            }
        } while (tobeProcessed.Count > 0 && processor != null) ;
    }
}

