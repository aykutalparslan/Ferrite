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
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.Transport;
using Ferrite.Utils;
using MessagePack;

namespace Ferrite.Core;

public class FerriteServer : IFerriteServer
{
    private readonly ILifetimeScope _scope;
    private readonly IConnectionListener _socketListener;
    private readonly IMessagePipe _pipe;
    private readonly Task? _pipeReceiveTask;
    private readonly ISessionService _sessionManager;
    private readonly ILogger _log;
    public FerriteServer(ILifetimeScope scope)
    {
        _scope = scope;
        _socketListener = _scope.Resolve<IConnectionListener>();
        _sessionManager = _scope.Resolve<ISessionService>();
        _pipe = _scope.Resolve<IMessagePipe>();
        _ = _pipe.SubscribeAsync(_sessionManager.NodeId.ToString());
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
        _log.Information(String.Format("Server is listening at {0}", socketListener.EndPoint));
        while (true)
        {
            if (await socketListener.AcceptAsync() is { } connection)
            {
                _log.Debug("New MTProto connection was created.");
                connection.Start();
                MTProtoConnection mtProtoConnection = _scope.Resolve<MTProtoConnection>(new NamedParameter("connection", connection));
                mtProtoConnection.Start();
                
            }
        }
    }

    private async Task DoReceive()
    {
        while (true)
        {
            var result = await _pipe.ReadMessageAsync();
            try
            {
                var message = MessagePackSerializer.Deserialize<MTProtoMessage>(result);
                if (message is { MessageType: MTProtoMessageType.Unencrypted } &&
                    message.Nonce != null)
                {
                    var sessionExists = _sessionManager.TryGetLocalAuthSession(message.Nonce, out var protoSession);
                    if (sessionExists &&
                        protoSession.TryGetConnection(out var connection) &&
                        !connection.IsEncrypted)
                    {
                        _ = connection.SendAsync(message);
                    }
                }
                else
                {
                    var sessionExists = _sessionManager.TryGetLocalSession(message.SessionId, out var protoSession);
                    if (sessionExists &&
                        protoSession.TryGetConnection(out var connection))
                    {
                        _log.Debug($"==> Session was found ==<");
                        await connection.SendAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
            }
        }
    }
}

