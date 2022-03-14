// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using DotNext.Buffers;

namespace Ferrite.Transport;

public sealed class SocketConnectionListener : IConnectionListener
{
    private Socket? _listenSocket;
    private readonly IOQueue _pipeScheduler;
    private readonly SocketSenderPool _socketSenderPool;
    public EndPoint EndPoint { get; private set; }

    public SocketConnectionListener(EndPoint endpoint)
    {
        EndPoint = endpoint;
        _pipeScheduler = new IOQueue();
        _socketSenderPool = new SocketSenderPool(_pipeScheduler);
        Bind();
    }

    private void Bind()
    {
        if (_listenSocket != null)
        {
            throw new InvalidOperationException("Transport is already bound.");
        }

        Socket listenSocket;
        try
        {
            listenSocket = new Socket(EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(EndPoint);
        }
        catch (SocketException e) when (e.SocketErrorCode == SocketError.AddressAlreadyInUse)
        {
            throw new Exception(e.Message, e);
        }

        Debug.Assert(listenSocket.LocalEndPoint != null);
        EndPoint = listenSocket.LocalEndPoint;

        listenSocket.Listen();

        _listenSocket = listenSocket;
    }

    public async ValueTask<ISocketConnection?> AcceptAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            try
            {
                Debug.Assert(_listenSocket != null, "Bind must be called first.");

                var acceptSocket = await _listenSocket.AcceptAsync(cancellationToken);

                // Only apply no delay to Tcp based endpoints
                if (acceptSocket.LocalEndPoint is IPEndPoint)
                {
                    acceptSocket.NoDelay = true;
                }
                return new SocketConnection(acceptSocket, UnmanagedMemoryPool<byte>.Shared, _pipeScheduler, _socketSenderPool);
            }
            catch (ObjectDisposedException)
            {
                // A call was made to UnbindAsync/DisposeAsync just return null which signals we're done
                return null;
            }
            catch (SocketException e) when (e.SocketErrorCode == SocketError.OperationAborted)
            {
                // A call was made to UnbindAsync/DisposeAsync just return null which signals we're done
                return null;
            }
            catch (SocketException)
            {
                // The connection got reset while it was in the backlog, so we try again.
            }
        }
    }

    public ValueTask UnbindAsync(CancellationToken cancellationToken = default)
    {
        _listenSocket?.Dispose();
        return default;
    }

    public ValueTask DisposeAsync()
    {
        _listenSocket?.Dispose();
        return default;
    }
}