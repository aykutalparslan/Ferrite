using System.Net;
using System.Net.Sockets;
using DotNext.Buffers;

namespace Ferrite.Transport;

public class SocketListener
{
    private Socket? _listenSocket;
    private readonly IPEndPoint _localEndPoint;
    private readonly SocketSenderPool _socketSenderPool;
    private readonly IOQueue _pipeScheduler;
    public SocketListener(IPEndPoint localEndPoint)
    {
        _localEndPoint = localEndPoint;
        _pipeScheduler = new IOQueue();
        _socketSenderPool = new SocketSenderPool(_pipeScheduler);
    }
    public void Bind()
    {
        if(_listenSocket == null)
        {
            _listenSocket = new Socket(_localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(_localEndPoint);
            _listenSocket.Listen();
        }
    }
    public async ValueTask<SocketConnection?> AcceptAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            try
            {
                var acceptSocket = await _listenSocket.AcceptAsync(cancellationToken);
                acceptSocket.NoDelay = true;
                return new SocketConnection(acceptSocket, UnmanagedMemoryPool<byte>.Shared, _pipeScheduler, _socketSenderPool);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}

