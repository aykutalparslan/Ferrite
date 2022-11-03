/*
 *   Project Ferrite is an Implementation Telegram Server API
 *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Affero General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Affero General Public License for more details.
 *
 *   You should have received a copy of the GNU Affero General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Buffers;
using System.IO.Pipelines;
using DotNext.IO;
using DotNext.Buffers;
using Ferrite.TL;
using System.Threading.Channels;
using Ferrite.Transport;
using Ferrite.Core.Framing;
using Ferrite.Data;
using Ferrite.Utils;
using Ferrite.TL.mtproto;
using Ferrite.Services;
using Ferrite.TL.currentLayer;
using Ferrite.TL.ObjectMapper;
using MessagePack;

namespace Ferrite.Core;

public sealed class MTProtoConnection : IMTProtoConnection
{
    public MTProtoTransport TransportType { get; private set; }
    public bool IsEncrypted => _session.AuthKeyId != 0;
    private readonly ITransportDetector _transportDetector;
    private readonly ILogger _log;
    private readonly ISessionService _sessionManager;
    private readonly IMapperContext _mapper;
    private readonly MTProtoSession _session;
    private IFrameDecoder _decoder;
    private IFrameEncoder _encoder;
    private readonly IUnencryptedMessageHandler _unencryptedMessageHandler;
    private readonly IMessageHandler _messageHandler;
    private readonly IStreamHandler _streamHandler;
    private readonly ITransportConnection _socketConnection;
    private Task? _receiveTask;
    private readonly Channel<MTProtoMessage> _outgoing = Channel.CreateUnbounded<MTProtoMessage>();
    private readonly Channel<IFileOwner> _outgoingStreams = Channel.CreateUnbounded<IFileOwner>();
    private readonly SemaphoreSlim _sendSemaphore = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _incomingSemaphore = new SemaphoreSlim(1, 1);
    private Task? _sendTask;
    private Task? _sendStreamTask;
    private Timer? _disconnectTimer;
    private readonly object _disconnectTimerState = new object();
    private readonly ITLObjectFactory _factory;
    private readonly SparseBufferWriter<byte> _writer = new(UnmanagedMemoryPool<byte>.Shared);
    private readonly ProtoTransport _protoTransport;
    internal ITransportConnection TransportConnection => _socketConnection;

    private readonly object _abortLock = new object();
    private bool _connectionAborted = false;

    public MTProtoConnection(ITransportConnection connection,
        ITLObjectFactory objectFactory, ITransportDetector detector,
        ILogger logger, ISessionService sessionManager, IMapperContext mapper, 
        IUnencryptedMessageHandler unencryptedMessageHandler,
        IMessageHandler messageHandler, MTProtoSession session,
        IStreamHandler streamHandler, ProtoTransport protoTransport)
    {
        _socketConnection = connection;
        TransportType = MTProtoTransport.Unknown;
        _factory = objectFactory;
        _transportDetector = detector;
        _log = logger;
        _sessionManager = sessionManager;
        _unencryptedMessageHandler = unencryptedMessageHandler;
        _streamHandler = streamHandler;
        _messageHandler = messageHandler;
        _mapper = mapper;
        _session = session;
        _protoTransport = protoTransport;
    }
    public void Start()
    {
        _receiveTask = DoReceive();
        _sendTask = DoSend();
        _sendStreamTask = DoSendStreams();
        DelayDisconnect();
    }
    public async ValueTask SendAsync(IFileOwner? message)
    {
        if (message != null)
        {
            await _outgoingStreams.Writer.WriteAsync(message);
        }
    }
    public async ValueTask SendAsync(MTProtoMessage message)
    {
        await _outgoing.Writer.WriteAsync(message);
    }
    private async Task DoReceive()
    {
        while (true)
        {
            await _incomingSemaphore.WaitAsync();
            var result = await _socketConnection.Transport.Input.ReadAsync();
            try
            {
                if (result.Buffer.Length > 0)
                {
                    if (_protoTransport.WebSocketHandshakeCompleted)
                    {
                        var position = await _protoTransport.DecodeWebSocketData(result.Buffer);
                        _socketConnection.Transport.Input.AdvanceTo(position);
                        
                        var wsResult = await _protoTransport.WebSocketReader.ReadAsync();
                        var wsPosition = await Process(wsResult.Buffer);
                        _protoTransport.WebSocketReader.AdvanceTo(wsPosition);
                    }
                    else
                    {
                        var position = await Process(result.Buffer);
                        _socketConnection.Transport.Input.AdvanceTo(position);
                    }
                }
                else
                {
                    _socketConnection.Transport.Input.AdvanceTo(result.Buffer.Start,
                        result.Buffer.End);
                }

                if (result.IsCompleted ||
                    result.IsCanceled)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
            }

            _incomingSemaphore.Release();
        }
    }
    private async Task DoSend()
    {
        while (await _outgoing.Reader.WaitToReadAsync())
        {
            try
            {
                var msg = await _outgoing.Reader.ReadAsync();
                await _sendSemaphore.WaitAsync();
                if (msg.MessageType == MTProtoMessageType.Updates)
                {
                    var updates = MessagePackSerializer.Typeless.Deserialize(msg.Data) as UpdatesBase;
                    if (updates != null)
                    {
                        var tlObj = _mapper.MapToTLObject<Updates, UpdatesBase>(updates);
                        msg.Data = tlObj.TLBytes.ToArray();
                        if (tlObj is UpdatesImpl update)
                        {
                            _log.Debug($"==> Sending Updates with Seq: {update.Seq} ==<");
                        }
                        var outgoingMessage = _messageHandler.GenerateOutgoingMessage(msg, _session);
                        WriteFrame(outgoingMessage);
                    }
                }
                else if (msg.MessageType == MTProtoMessageType.QuickAck)
                {
                    var quickAck = _protoTransport.GenerateQuickAck(msg.QuickAck, TransportType);
                    WriteFrame(quickAck);
                }
                else if (_session.AuthKeyId == 0)
                {
                    var outgoingMessage = _unencryptedMessageHandler.GenerateOutgoingMessage(msg, _session);
                    WriteFrame(outgoingMessage);
                }
                else if (_session.AuthKey != null &&
                         _session.AuthKey.Length == 192)
                {
                    var outgoingMessage = _messageHandler.GenerateOutgoingMessage(msg, _session);
                    WriteFrame(outgoingMessage);
                }

                var result = await FlushSocketAsync();
                if (result.IsCompleted ||
                    result.IsCanceled)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
            }
            finally
            {
                _sendSemaphore.Release();
            }
        }
    }
    private async Task DoSendStreams()
    {
        while (await _outgoingStreams.Reader.WaitToReadAsync())
        {
            try
            {
                var msg = await _outgoingStreams.Reader.ReadAsync();
                await _sendSemaphore.WaitAsync();
                _log.Debug($"=>Sending stream.");

                await _streamHandler.HandleOutgoingStream(msg, this, _session);

                var result = await _socketConnection.Transport.Output.FlushAsync();
                if (result.IsCompleted ||
                    result.IsCanceled)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
            }
            finally
            {
                _sendSemaphore.Release();
            }
        }
    }
    private async Task<SequencePosition> Process(ReadOnlySequence<byte> buffer)
    {
        if (buffer.Length < 4) return buffer.Start;
        SequencePosition position = buffer.Start;
        if (TransportType == MTProtoTransport.Unknown)
        {
            var rd = IAsyncBinaryReader.Create(buffer);
            int firstInt = rd.ReadInt32(true);
            if (firstInt == WebSocketHandler.Get)
            {
                var handshake = _protoTransport.ProcessWebSocketHandshake(buffer);
                if (handshake.Completed)
                {
                    WriteFrame(handshake.Response, false);
                    var result = await FlushSocketAsync();
                }
                return handshake.Position;
            }

            TransportType = _transportDetector.DetectTransport(buffer,
            out _decoder, out _encoder, out position);
        }

        bool hasMore;
        do
        {
            hasMore = _decoder.Decode(buffer.Slice(position), out var frame, 
                out var isStream, out var requiresQuickAck, out position);
            if (isStream)
            {
                await _streamHandler.HandleIncomingStreamAsync(frame, this, 
                    _socketConnection.RemoteEndPoint, _session,
                    hasMore);
            }
            else if (frame.Length > 0)
            {
                ProcessFrame(frame, requiresQuickAck);
            }
        } while (hasMore);

        return position;
    }
    private void ProcessFrame(ReadOnlySequence<byte> bytes, bool requiresQuickAck)
    {
        if (bytes.Length < 8)
        {
            return;
        }
        var reader = new SequenceReader(bytes);
        long authKeyId = reader.ReadInt64(true);
        if (authKeyId != 0)
        {
            if (!_session.TryFetchAuthKey(authKeyId) &&
                _session.AuthKeyId == 0)
            {
                SendTransportError(404);
            }
        }
        if (_session.PermAuthKeyId != 0)
        {
            _session.SaveCurrentSession(_session.PermAuthKeyId, this);
        }
        if (authKeyId == 0)
        {
            _unencryptedMessageHandler.HandleIncomingMessage(bytes.Slice(8), this, _session);
        }
        else if(_session.AuthKey != null)
        {
            _messageHandler.HandleIncomingMessage(bytes.Slice(8), this,
                _socketConnection.RemoteEndPoint, _session, requiresQuickAck);
        }
    }
    public void SendNewSessionCreatedMessage(long firstMessageId, long serverSalt)
    {
        var sessionCreated = _session.GenerateSessionCreated(firstMessageId, serverSalt);
        SendAsync(sessionCreated);
    }
    internal void SendTransportError(int errorCode)
    {
        var transportError = _protoTransport.GenerateTransportError(errorCode);
        WriteFrame(transportError);
        FlushSocketAsync();
    }
    public async ValueTask Ping(long pingId, int delayDisconnectInSeconds = 75)
    {
        DelayDisconnect(delayDisconnectInSeconds * 1000);
        await _sessionManager.OnPing(_session.AuthKeyId != 0 ? 
                _session.PermAuthKeyId : _session.AuthKeyId,
            _session.SessionId);
        var pong = _factory.Resolve<Pong>();
        pong.PingId = pingId;
        pong.MsgId = _session.NextMessageId(true);
        MTProtoMessage message = new MTProtoMessage()
        {
            Data = pong.TLBytes.ToArray(),
            IsContentRelated = false,
            IsResponse = true,
            MessageType = MTProtoMessageType.Pong,
            SessionId = _session.SessionId,
            MessageId = pong.MsgId
        };
        await SendAsync(message);
    }
    private void WriteFrame(ReadOnlySequence<byte> buffer, bool webSocketFeatureEnabled = true)
    {
        if(buffer.Length == 0) return;
        var encoded = _encoder?.Encode(buffer) ?? buffer;
        if (webSocketFeatureEnabled &&
            _protoTransport.WebSocketHandshakeCompleted)
        {
            var webSocketHeader = _protoTransport.GenerateWebSocketHeader((int)encoded.Length);
            _socketConnection.Write(webSocketHeader);
        }
        _socketConnection.Write(encoded);
    }
    internal void WriteFrameHeader(int length)
    {
        if(length == 0) return;
        var header = _encoder.GenerateHead(length);
        if (_protoTransport.WebSocketHandshakeCompleted)
        {
            var webSocketHeader = _protoTransport.GenerateWebSocketHeader((int)header.Length + length);
            _socketConnection.Write(webSocketHeader);
        }

        WriteFrameBlock(header);
    }
    internal void WriteFrameBlock(ReadOnlySequence<byte> buffer)
    {
        if(buffer.Length == 0) return;
        var encoded = _encoder.EncodeBlock(buffer);
        _socketConnection.Write(encoded);
    }
    internal void WriteFrameTail()
    {
        var frameTail = _encoder.EncodeTail();
        if (frameTail.Length > 0)
        {
            _socketConnection.Transport.Output.Write(frameTail);
        }
    }
    internal ValueTask<FlushResult> FlushSocketAsync()
    {
        return _socketConnection.FlushAsync();
    }
    private void DelayDisconnect(int delayInMilliseconds = 750000)
    {
        lock (_disconnectTimerState)
        {
            if (_disconnectTimer == null)
            {
                _disconnectTimer = new Timer((state) =>
                {
                    Abort(new Exception());
                }, _disconnectTimerState, delayInMilliseconds, delayInMilliseconds);
            }
            else
            {
                _disconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _disconnectTimer.Change(delayInMilliseconds, delayInMilliseconds);
            }
        }
    }
    public void Abort(Exception abortReason)
    {
        lock (_abortLock)
        {
            if (_connectionAborted)
            {
                return;
            }

            _connectionAborted = true;
            try
            {
                _ = _streamHandler.DisposeAsync();
                _sessionManager.RemoveSession(_session.AuthKeyId, _session.SessionId);
                _outgoing.Writer.Complete();
                _socketConnection.Abort(abortReason);
                _socketConnection.DisposeAsync();
                _writer.Dispose();
                _disconnectTimer?.Dispose();
            }
            catch (Exception ex)
            {
                _log.Verbose(ex, $"Connection closed for authKeyId{_session.AuthKeyId}");
            }
        }
    }
}


