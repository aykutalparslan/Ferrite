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
using Autofac;
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.mtproto;
using MessagePack;

namespace Ferrite.Core;

public class ServiceMessagesProcessor : IProcessor
{
    private readonly ILifetimeScope _scope;
    private readonly ISessionService _sessionManager;
    private readonly IDistributedPipe _pipe;
    public ServiceMessagesProcessor(ILifetimeScope scope, ISessionService sessionManager, IDistributedPipe pipe)
    {
        _scope = scope;
        _sessionManager = sessionManager;
        _pipe = pipe;
    }

    public async Task Process(object? sender, ITLObject input, Queue<ITLObject> output, TLExecutionContext ctx)
    {
        if(sender is MTProtoConnection connection)
        {
            if (input.Constructor == TLConstructor.Ping &&
            input is Ping ping)
            {
                Console.WriteLine("Ping received.");
                await connection.Ping(ping.PingId);
            }
            else if (input.Constructor == TLConstructor.PingDelayDisconnect &&
               input is PingDelayDisconnect pingDelay)
            {
                Console.WriteLine($"Ping received with delay of {pingDelay.DisconnectDelay} seconds.");
                await connection.Ping(pingDelay.PingId, pingDelay.DisconnectDelay);
            }
            else if (input.Constructor == TLConstructor.Message && input is Message message &&
              message.Body.Constructor == TLConstructor.Ping && message.Body is Ping ping2)
            {
                Console.WriteLine("Ping received.");
                await connection.Ping(ping2.PingId);
            }
            else if (input.Constructor == TLConstructor.Message && input is Message message2 &&
               message2.Body.Constructor == TLConstructor.PingDelayDisconnect &&
               message2.Body is PingDelayDisconnect pingDelay2)
            {
                Console.WriteLine($"Ping received with delay of {pingDelay2.DisconnectDelay} seconds.");
                await connection.Ping(pingDelay2.PingId, pingDelay2.DisconnectDelay);
            }
            else
            {
                output.Enqueue(input);
            }
        }
        else
        {
            output.Enqueue(input);
        }
    }
}

