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
using System.Net;
using Autofac;
using Ferrite.Data;
using Ferrite.TL;
using Ferrite.Transport;
using Ferrite.Utils;
using MessagePack;

namespace Ferrite.Core;

public class FerriteServer : IFerriteServer
{
    private readonly ILifetimeScope _scope;
    private readonly IConnectionListener _socketListener;
    private readonly ITLObjectFactory _factory;
    private readonly IDistributedStore _store;
    private readonly IDistributedPipe _pipe;
    private readonly Task? _pipeReceiveTask;
    private readonly ISessionManager _sessionManager;
    private readonly IProcessor _requestProcessor;
    private readonly ILogger _log;
    public FerriteServer(ILifetimeScope scope)
    {
        _scope = scope;
        _socketListener = _scope.Resolve<IConnectionListener>();
        _factory = _scope.Resolve<ITLObjectFactory>();
        _store = _scope.Resolve<IDistributedStore>();
        _sessionManager = _scope.Resolve<ISessionManager>();
        _requestProcessor = _scope.Resolve<IProcessor>();
        _pipe = _scope.Resolve<IDistributedPipe>();
        _pipe.Subscribe(_sessionManager.NodeId.ToString());
        _pipeReceiveTask = DoReceive();
        _log = _scope.Resolve<ILogger>();
    }

    public async Task StartAsync(IPEndPoint endPoint, CancellationToken token)
    {
        _socketListener.Bind(endPoint);
        await StartAccept(_socketListener);
    }

    public async Task StopAsync(CancellationToken token)
    {
        await _scope.DisposeAsync();

    }

    private async Task StartAccept(IConnectionListener socketListener)
    {
        Console.WriteLine("Server is listening at {0}", socketListener.EndPoint);
        _log.Information(String.Format("Server is listening at {0}", socketListener.EndPoint));
        while (true)
        {
            if (await socketListener.AcceptAsync() is
                ITransportConnection connection)
            {
                connection.Start();
                MTProtoConnection mtProtoConnection = _scope.Resolve<MTProtoConnection>(new NamedParameter("connection", connection));
                mtProtoConnection.Start();
            }
        }
    }

    private async Task DoReceive()
    {
        try
        {
            while (true)
            {
                var result = await _pipe.ReadAsync();
                var message = MessagePackSerializer.Deserialize<MTProtoMessage>(result);
                var sessionExists = _sessionManager.TryGetLocalSession(message.SessionId, out var protoSession);
                if (sessionExists &&
                    protoSession.TryGetConnection(out var connection))
                {
                    _ = connection.SendAsync(message);
                }
            }
        }
        catch (Exception ex)
        {
            _log.Debug(ex, ex.Message);
        }
    }
}

