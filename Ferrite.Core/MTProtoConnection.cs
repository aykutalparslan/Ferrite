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
using System.Buffers.Binary;
using DotNext.IO;
using DotNext.Buffers;
using Ferrite.TL;
using System.Threading.Channels;
using Ferrite.Transport;
using System.IO.Pipelines;
using Ferrite.Data;
using Ferrite.Crypto;
using Ferrite.Utils;
using Ferrite.Core.Exceptions;
using Ferrite.TL.mtproto;
using System.Net;
using System.Security.Cryptography;
using DotNext;
using DotNext.Collections.Generic;
using DotNext.IO.Pipelines;
using Ferrite.Services;
using Ferrite.TL.currentLayer;
using Ferrite.TL.currentLayer.storage;
using Ferrite.TL.currentLayer.upload;
using Ferrite.TL.ObjectMapper;
using MessagePack;
using Org.BouncyCastle.Cms;
using TLConstructor = Ferrite.TL.currentLayer.TLConstructor;

namespace Ferrite.Core;

public class MTProtoConnection : IMTProtoConnection
{
    public MTProtoTransport TransportType { get; private set; }
    public bool IsEncrypted => _authKeyId != 0;
    private readonly ITransportDetector transportDetector;
    private readonly IDistributedCache _cache;
    private readonly IPersistentStore _db;
    private readonly ILogger _log;
    private readonly IRandomGenerator _random;
    private readonly ISessionService _sessionManager;
    private readonly IMTProtoTime _time;
    private readonly IMapperContext _mapper;
    private IFrameDecoder decoder;
    private IFrameEncoder encoder;
    private IProcessorManager _processorManager;
    private long _authKeyId;
    private long _permAuthKeyId;
    private byte[]? _authKey;
    private long _sessionId;
    private long _uniqueSessionId;
    private int _seq = 0;
    private ITransportConnection socketConnection;
    private Task? receiveTask;
    private Channel<MTProtoMessage> _outgoing = Channel.CreateUnbounded<MTProtoMessage>();
    private Channel<IDistributedFileOwner> _outgoingStreams = Channel.CreateUnbounded<IDistributedFileOwner>();
    private readonly SemaphoreSlim _sendSemaphore = new SemaphoreSlim(1, 1);
    private Task? sendTask;
    private Task? sendStreamTask;
    private Timer? disconnectTimer;
    private object disconnectTimerState = new object();
    private readonly ITLObjectFactory factory;
    private long _lastMessageId;
    private readonly CircularQueue<long> _lastMessageIds = new CircularQueue<long>(10);
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private WebSocketHandler? webSocketHandler;
    private Pipe webSocketPipe;
    private MTProtoPipe? _currentRequest = null;
    public Dictionary<string, object> SessionData { get; private set; } = new();

    private readonly object _abortLock = new object();
    private bool _connectionAborted = false;

    public MTProtoConnection(ITransportConnection connection,
        ITLObjectFactory objectFactory, ITransportDetector detector,
        IDistributedCache cache, IPersistentStore persistentStore,
        ILogger logger, IRandomGenerator random, ISessionService sessionManager,
        IMTProtoTime protoTime, IProcessorManager processorManager,
        IMapperContext mapper)
    {
        socketConnection = connection;
        TransportType = MTProtoTransport.Unknown;
        factory = objectFactory;
        transportDetector = detector;
        _cache = cache;
        _db = persistentStore;
        _log = logger;
        _random = random;
        _sessionManager = sessionManager;
        _time = protoTime;
        _processorManager = processorManager;
        _mapper = mapper;
    }

    public async ValueTask SendAsync(IDistributedFileOwner message)
    {
        if (message != null)
        {
            await _outgoingStreams.Writer.WriteAsync(message);
        }
    }

    public void Start()
    {
        receiveTask = DoReceive();
        sendTask = DoSend();
        sendStreamTask = DoSendStreams();
        DelayDisconnect();
    }

    private void DelayDisconnect(int delayInMiliseconds = 750000)
    {
        lock (disconnectTimerState)
        {
            if (disconnectTimer == null)
            {
                disconnectTimer = new Timer((state) =>
                {
                    Abort(new Exception());
                }, disconnectTimerState, delayInMiliseconds, delayInMiliseconds);
            }
            else
            {
                disconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);
                disconnectTimer.Change(delayInMiliseconds, delayInMiliseconds);
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
                _currentRequest.Dispose();
                _sessionManager.RemoveSession(_authKeyId, _sessionId);
                _outgoing.Writer.Complete();
                socketConnection.Abort(abortReason);
                socketConnection.DisposeAsync();
                writer.Dispose();
                disconnectTimer?.Dispose();
            }
            catch (Exception ex)
            {

            }
        }
    }
    public async ValueTask Ping(long pingId, int delayDisconnectInSeconds = 75)
    {
        DelayDisconnect(delayDisconnectInSeconds * 1000);
        var pong = factory.Resolve<Pong>();
        pong.PingId = pingId;
        pong.MsgId = NextMessageId(true);
        MTProtoMessage message = new MTProtoMessage()
        {
            Data = pong.TLBytes.ToArray(),
            IsContentRelated = false,
            IsResponse = true,
            MessageType = MTProtoMessageType.Pong,
            SessionId = _sessionId,
            MessageId = pong.MsgId
        };
        await SendAsync(message);
    }

    private async Task DoReceive()
    {
        while (true)
        {
            var result = await socketConnection.Transport.Input.ReadAsync();
            try
            {
                if (result.IsCanceled)
                {
                    break;
                }

                if (result.Buffer.Length > 0)
                {
                    if (webSocketHandler != null)
                    {
                        webSocketPipe ??= new Pipe();

                        var position = webSocketHandler.DecodeTo(result.Buffer, webSocketPipe.Writer);
                        _ = await webSocketPipe.Writer.FlushAsync();
                        socketConnection.Transport.Input.AdvanceTo(position);

                        var wsResult = await webSocketPipe.Reader.ReadAsync();
                        var wsPosition = Process(wsResult.Buffer);
                        webSocketPipe.Reader.AdvanceTo(wsPosition);
                    }
                    else
                    {
                        var position = Process(result.Buffer);
                        socketConnection.Transport.Input.AdvanceTo(position);
                    }
                }

                if (result.IsCompleted)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
            }
        }
    }

    public async ValueTask SendAsync(MTProtoMessage message)
    {
        await _outgoing.Writer.WriteAsync(message);
    }

    private async Task DoSend()
    {
        try
        {
            while (true)
            {
                var msg = await _outgoing.Reader.ReadAsync();
                await _sendSemaphore.WaitAsync();
                //_log.Debug($"=>Sending {msg.MessageType} message for {msg.MessageId}.");
                if (msg.MessageType == MTProtoMessageType.Updates &&
                    await _sessionManager.GetSessionStateAsync(_sessionId)
                    is { } sess)
                {
                    var updates = MessagePackSerializer.Typeless.Deserialize(msg.Data) as UpdatesBase;
                    var tlObj = _mapper.MapToTLObject<Updates, UpdatesBase>(updates);
                    msg.Data = tlObj.TLBytes.ToArray();
                    SendEncrypted(msg, sess);
                }
                else if (msg.MessageType == MTProtoMessageType.QuickAck)
                {
                    SendQuickAck(msg.QuickAck);
                }
                else if (_authKeyId == 0)
                {
                    SendUnencrypted(msg.Data, NextMessageId(msg.IsResponse));
                }
                else if (await _sessionManager.GetSessionStateAsync(_sessionId)
                         is { } state)
                {
                    SendEncrypted(msg, state);
                }
                var result = await socketConnection.Transport.Output.FlushAsync();
                _sendSemaphore.Release();
                if (result.IsCompleted ||
                    result.IsCanceled)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, ex.Message);
        }
        finally
        {
            if (_sendSemaphore.CurrentCount != 0)
            {
                _sendSemaphore.Release();
            }
        }
    }
    private async Task DoSendStreams()
    {
        try
        {
            while (true)
            {
                var msg = await _outgoingStreams.Reader.ReadAsync();
                await _sendSemaphore.WaitAsync();
                _log.Debug($"=>Sending stream.");
                if (_authKeyId == 0)
                {
                    _sendSemaphore.Release();
                    continue;
                }
                else if (await _sessionManager.GetSessionStateAsync(_sessionId)
                         is { } state)
                {
                    await SendStream(msg, state);
                }

                var result = await socketConnection.Transport.Output.FlushAsync();
                _sendSemaphore.Release();
                if (result.IsCompleted ||
                    result.IsCanceled)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, ex.Message);
        }
        finally
        {
            if (_sendSemaphore.CurrentCount != 0)
            {
                _sendSemaphore.Release();
            }
        }
    }
    
    private async Task SendStream(IDistributedFileOwner message, SessionState state)
    {
        if (message == null) return;
        var rpcResult = factory.Resolve<RpcResult>();
        rpcResult.ReqMsgId = message.ReqMsgId;
        var data = await message.GetFileStream();
        if (data.Length < 0) return;
        
        var fileImpl = factory.Resolve<FileImpl>();
        fileImpl.Type = factory.Resolve<FileJpegImpl>();
        fileImpl.Mtime = (int)DateTimeOffset.Now.ToUnixTimeSeconds();
        fileImpl.Bytes = Array.Empty<byte>();
        rpcResult.Result = fileImpl;
        byte[] resultHeader = new byte[24 + (data.Length < 254 ? 1 : 4)];
        rpcResult.TLBytes.Slice(0, 24).CopyTo(resultHeader);
        int pad = 0;
        if (data.Length < 254)
        {
            resultHeader[24] = (byte)data.Length;
            pad = (int)((4 - ((data.Length + 1) % 4)) % 4);
        }
        else
        {
            resultHeader[24] = 254;
            resultHeader[25] = (byte)(data.Length & 0xff);
            resultHeader[26] = (byte)((data.Length & 0xff) >> 8);
            resultHeader[27] = (byte)((data.Length & 0xff) >> 16);
            pad = (int)((4 - ((data.Length + 4) % 4)) % 4);
        }
        writer.Clear();
        writer.WriteInt64(state.ServerSalt.Salt, true);
        writer.WriteInt64(state.SessionId, true);
        writer.WriteInt64(NextMessageId(true), true);
        writer.WriteInt32(GenerateSeqNo(true), true);
        writer.WriteInt32(resultHeader.Length + (int)data.Length + pad, true);
        var cryptographicHeader = writer.ToReadOnlySequence().ToArray();
        int paddingLength = _random.GetNext(12, 512);
        while ((resultHeader.Length + data.Length + pad + paddingLength) % 16 != 0)
        {
            paddingLength++;
        }
        var paddingBytes = _random.GetRandomBytes(paddingLength);
        Queue<Stream> streams = new Queue<Stream>();
        streams.Enqueue(new MemoryStream(cryptographicHeader));
        streams.Enqueue(new MemoryStream(resultHeader));
        streams.Enqueue(data);
        streams.Enqueue(new MemoryStream(new byte[pad]));
        streams.Enqueue(new MemoryStream(paddingBytes));
        var stream = new ConcatenatedStream(streams, 0, Int32.MaxValue);

        var messageKey = AesIge.GenerateMessageKey(_authKey, stream).ToArray();
        byte[] aesKey = new byte[32];
        byte[] aesIV = new byte[32];
        AesIge.GenerateAesKeyAndIV(_authKey, messageKey, false, aesKey, aesIV);
        //--
        writer.Clear();
        writer.WriteInt64(state.AuthKeyId, true);
        writer.Write(messageKey);
        var frameHead = encoder.EncodeHead(24 + (int)stream.Length);
        var header = writer.ToReadOnlySequence();
        if (webSocketHandler != null)
        {
            webSocketHandler.WriteHeaderTo(socketConnection.Transport.Output, 
                frameHead.Length + 24 + (int)stream.Length);
        }
        socketConnection.Transport.Output.Write(encoder.EncodeBlock(frameHead));
        socketConnection.Transport.Output.Write(encoder.EncodeBlock(header));
        //--
        MTProtoPipe pipe = new MTProtoPipe(aesKey, aesIV, true);
        await pipe.WriteAsync(cryptographicHeader);
        await pipe.WriteAsync(resultHeader);
        var dataStream = await message.GetFileStream();
        int remaining = (int)dataStream.Length;;
        var buffer = new byte[1024];
        while (remaining > 0)
        {
            var read = await dataStream.ReadAsync(buffer.AsMemory(0, Math.Min(remaining, 1024)));
            await pipe.WriteAsync(new ReadOnlySequence<byte>(buffer, 0, read));
            remaining -= read;
            var encrypted = await pipe.Input.ReadAsync();
            socketConnection.Transport.Output.Write(encoder.EncodeBlock(encrypted.Buffer));
            pipe.Input.AdvanceTo(encrypted.Buffer.End);
            await socketConnection.Transport.Output.FlushAsync();
        }
        if (pad > 0)
        {
            await pipe.WriteAsync(new byte[pad]);
        }
        if (paddingLength > 0)
        {
            await pipe.WriteAsync(paddingBytes);
        }

        await pipe.CompleteAsync();

        var readResult = await pipe.Input.ReadAsync();
        socketConnection.Transport.Output.Write(encoder.EncodeBlock(readResult.Buffer));
        pipe.Input.AdvanceTo(readResult.Buffer.End);
        while (!readResult.IsCompleted)
        {
            readResult = await pipe.Input.ReadAsync();
            socketConnection.Transport.Output.Write(encoder.EncodeBlock(readResult.Buffer));
            pipe.Input.AdvanceTo(readResult.Buffer.End);
        }

        var frameTail = encoder.EncodeTail();
        if (frameTail.Length > 0)
        {
            socketConnection.Transport.Output.Write(frameTail);
        }
    }
    private void SendUnencrypted(Span<byte> data, long messageId)
    {
        writer.Clear();
        writer.WriteInt64(0, true);
        writer.WriteInt64(messageId, true);
        writer.WriteInt32(data.Length, true);
        writer.Write(data);
        var message = writer.ToReadOnlySequence();
        var encoded = encoder.Encode(message);
        if (webSocketHandler != null)
        {
            webSocketHandler.WriteHeaderTo(socketConnection.Transport.Output, encoded.Length);
        }
        socketConnection.Transport.Output.Write(encoded);
    }

    private void SendEncrypted(MTProtoMessage message, SessionState state)
    {
        if (message.Data == null) { return; }
        writer.Clear();
        writer.WriteInt64(state.ServerSalt.Salt, true);
        writer.WriteInt64(message.SessionId, true);
        writer.WriteInt64(message.MessageType == MTProtoMessageType.Pong ?
            message.MessageId :
            NextMessageId(message.IsResponse), true);
        writer.WriteInt32(GenerateSeqNo(message.IsContentRelated), true);
        writer.WriteInt32(message.Data.Length, true);
        writer.Write(message.Data);
        int paddingLength = _random.GetNext(12, 512);
        while ((message.Data.Length + paddingLength) % 16 != 0)
        {
            paddingLength++;
        }
        writer.Write(_random.GetRandomBytes(paddingLength), false);

        using var messageData = UnmanagedMemoryPool<byte>.Shared.Rent((int)writer.WrittenCount);
        var messageSpan = messageData.Memory.Slice(0, (int)writer.WrittenCount).Span;
        writer.ToReadOnlySequence().CopyTo(messageSpan);
        Span<byte> messageKey = AesIge.GenerateMessageKey(_authKey, messageSpan);
        AesIge aesIge = new AesIge(_authKey, messageKey, false);
        aesIge.Encrypt(messageSpan);
        writer.Clear();
        writer.WriteInt64(state.AuthKeyId, true);
        writer.Write(messageKey);
        writer.Write(messageSpan);
        var msg = writer.ToReadOnlySequence();
        var encoded = encoder.Encode(msg);
        if (webSocketHandler != null)
        {
            webSocketHandler.WriteHeaderTo(socketConnection.Transport.Output, encoded.Length);
        }
        socketConnection.Transport.Output.Write(encoded);
    }

    private void SendQuickAck(int ack)
    {
        writer.Clear();
        if (encoder is AbridgedFrameEncoder)
        {
            ack = BinaryPrimitives.ReverseEndianness(ack);
        }
        ack |= 1 << 31;
        writer.WriteInt32(ack, true);
        var msg = writer.ToReadOnlySequence();
        var encoded = encoder.EncodeBlock(msg);
        if (webSocketHandler != null)
        {
            webSocketHandler.WriteHeaderTo(socketConnection.Transport.Output, 4);
        }

        socketConnection.Transport.Output.Write(encoded);
    }
    
    private void SendTransportError(int errorCode)
    {
        writer.Clear();
        writer.WriteInt32(-errorCode, true);
        var msg = writer.ToReadOnlySequence();
        var encoded = encoder.EncodeBlock(msg);
        if (webSocketHandler != null)
        {
            webSocketHandler.WriteHeaderTo(socketConnection.Transport.Output, 4);
        }

        socketConnection.Transport.Output.Write(encoded);
    }

    public delegate Task AsyncEventHandler<in MTProtoAsyncEventArgs>(object? sender, MTProtoAsyncEventArgs e);
    public event AsyncEventHandler<MTProtoAsyncEventArgs>? MessageReceived;

    protected virtual void OnMessageReceived(MTProtoAsyncEventArgs e)
    {
        MessageReceived?.Invoke(this, e);
    }

    private SequencePosition Process(in ReadOnlySequence<byte> buffer)
    {
        SequenceReader<byte> reader = new SequenceReader<byte>(buffer);
        if (TransportType == MTProtoTransport.Unknown)
        {
            if (reader.TryReadLittleEndian(out int firstInt))
            {
                reader.Rewind(4);
                if (firstInt == WebSocketHandler.Get)
                {
                    ProcessWebSocketHandshake(ref reader);
                    return reader.Position;
                }
            }

            TransportType = transportDetector.DetectTransport(ref reader,
            out decoder, out encoder);
        }

        bool hasMore;
        do
        {
            hasMore = decoder.Decode(ref reader, out var frame, 
                out var isStream, out var requiresQuickAck);
            if (isStream)
            {
                ProcessStream(frame, hasMore).Wait();
                return reader.Position;
            }
            if (frame.Length > 0)
            {
                ProcessFrame(frame, requiresQuickAck);
            }
        } while (hasMore);

        return reader.Position;
    }

    private async Task ProcessStream(ReadOnlySequence<byte> frame, bool hasMore)
    {
        SequenceReader reader = IAsyncBinaryReader.Create(frame);
        if (_currentRequest == null)
        {
            if (frame.Length < 24)
            {
                return;
            }
            long authKeyId = reader.ReadInt64(true);
            if (authKeyId != 0 && _authKeyId != authKeyId)
            {
                _authKeyId = authKeyId;
                _authKey = null;
                GetAuthKey();
            }

            if (_authKeyId != 0 && _permAuthKeyId == 0)
            {
                long? permAuthKey = await _cache.GetBoundAuthKeyAsync(authKeyId);
                _permAuthKeyId = permAuthKey ?? 0;
            }

            var incomingMessageKey = new byte[16];
            reader.Read(incomingMessageKey);
            var aesKey = new byte[32];
            var aesIV = new byte[32];
            AesIge.GenerateAesKeyAndIV(_authKey,incomingMessageKey,true,aesKey,aesIV);
            _currentRequest = new MTProtoPipe(aesKey, aesIV, false);
            await _currentRequest.WriteAsync(reader);
            await ProcessPipe(_currentRequest);
        }
        else
        { 
            await _currentRequest.WriteAsync(reader);
        }

        if (!hasMore)
        {
            _currentRequest.Complete();
            _currentRequest = null;
        }
    }

    private async Task ProcessPipe(MTProtoPipe pipe)
    {
        TLExecutionContext context = new TLExecutionContext(SessionData);
        context.Salt = await pipe.Input.ReadInt64Async(true);
        context.SessionId = await pipe.Input.ReadInt64Async(true);
        context.AuthKeyId = _authKeyId;
        context.PermAuthKeyId = _permAuthKeyId;
        context.MessageId = await pipe.Input.ReadInt64Async(true);
        context.SequenceNo = await pipe.Input.ReadInt32Async(true);
        if (socketConnection.RemoteEndPoint is IPEndPoint endpoint)
        {
            context.IP = endpoint.Address.ToString();
        }

        int messageDataLength = await pipe.Input.ReadInt32Async(true);
        int constructor = await pipe.Input.ReadInt32Async(true);
        if (_sessionId == 0)
        {
            _sessionId = context.SessionId;
            SessionState state = new SessionState();
            var salt = new ServerSaltDTO();
            state.SessionId = _sessionId;
            state.ServerSalt = salt;
            state.AuthKeyId = _authKeyId;
            state.AuthKey = _authKey;
            state.NodeId = _sessionManager.NodeId;
            await _sessionManager.AddSessionAsync(state,
                new MTProtoSession(this));

            _uniqueSessionId = _random.NextLong();
            var newSessionCreated = factory.Resolve<NewSessionCreated>();
            newSessionCreated.FirstMsgId = context.MessageId;
            newSessionCreated.ServerSalt = salt.Salt;
            newSessionCreated.UniqueId = _uniqueSessionId;
            MTProtoMessage newSessionMessage = new MTProtoMessage();
            newSessionMessage.Data = newSessionCreated.TLBytes.ToArray();
            newSessionMessage.IsContentRelated = false;
            newSessionMessage.IsResponse = false;
            newSessionMessage.SessionId = _sessionId;
            newSessionMessage.MessageType = MTProtoMessageType.NewSession;
            _ = SendAsync(newSessionMessage);
        }

        if (context.MessageId < _time.ThirtySecondsLater &&
            //msg_id values that belong over 30 seconds in the future
            context.MessageId > _time.FiveMinutesAgo &&
            //or over 300 seconds in the past are to be ignored
            context.MessageId % 2 == 0 && //must have even parity
            (_lastMessageIds.Count == 0 || (!_lastMessageIds.Contains(context.MessageId) && //must not be equal to any
                                            context.MessageId > _lastMessageIds.Min()))) //must not be lower than all
        {
            _lastMessageIds.Enqueue(context.MessageId);

            try
            {
                if (constructor == TLConstructor.Upload_SaveFilePart)
                {
                    var msg = factory.Resolve<SaveFilePart>();
                    await msg.SetPipe(pipe);
                    _ = _processorManager.Process(this, msg, context);
                    OnMessageReceived(new MTProtoAsyncEventArgs(msg, context));
                }
                else if (constructor == TLConstructor.Upload_SaveBigFilePart)
                {
                    var msg = factory.Resolve<SaveBigFilePart>();
                    await msg.SetPipe(pipe);
                    _ = _processorManager.Process(this, msg, context);
                    OnMessageReceived(new MTProtoAsyncEventArgs(msg, context));
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
            }
        }
    }

    private void ProcessWebSocketHandshake(ref SequenceReader<byte> reader)
    {
        if (webSocketHandler == null)
        {
            webSocketHandler = new();
        }
        HttpParser<WebSocketHandler> parser = new HttpParser<WebSocketHandler>();
        if (!webSocketHandler.RequestLineComplete)
        {
            parser.ParseRequestLine(webSocketHandler, ref reader);
        }
        parser.ParseHeaders(webSocketHandler, ref reader);
        if (webSocketHandler.HeadersComplete)
        {
            webSocketHandler.WriteHandshakeResponseTo(socketConnection.Transport.Output);
            socketConnection.Transport.Output.FlushAsync();
        }
    }

    private void ProcessEncryptedMessageAsync(ReadOnlySequence<byte> bytes, bool requiresQuickAck)
    {
        if (bytes.Length < 16)
        {
            return;
        }

        try
        {
            DecryptAndRaiseEvent(in bytes, requiresQuickAck);
        }
        catch (Exception ex)
        {
            _log.Error(ex, ex.Message);
        }
    }

    private void GetAuthKey()
    {
        if (_authKey == null)
        {
            _log.Information("Trying to get the authKey from cache");
            var authKey = _cache.GetAuthKey(_authKeyId);
            if (authKey != null)
            {
                Interlocked.CompareExchange(ref _authKey, authKey, null);
                _permAuthKeyId = _authKeyId;
            }
        }
        if (_authKey == null)
        {
            _log.Information("Trying to get tempAuthKey");
            var authKey = _cache.GetTempAuthKey(_authKeyId);
            if (authKey != null)
            {
                Interlocked.CompareExchange(ref _authKey, authKey, null);
            }
        }
        if (_authKey == null)
        {
            _log.Information("Trying to get the authKey from db");
            var authKey = _db.GetAuthKey(_authKeyId);
            if (authKey != null)
            {
                Interlocked.CompareExchange(ref _authKey, authKey, null);
                _permAuthKeyId = _authKeyId;
                _ = _cache.PutAuthKeyAsync(_authKeyId, _authKey);
            }
        }

        if (_authKey == null)
        {
            _sendSemaphore.Wait();
            SendTransportError(404);
            Abort(new Exception("Auth key not found"));
        }
    }

    private void DecryptAndRaiseEvent(in ReadOnlySequence<byte> bytes, bool requiresQuickAck)
    {
        SequenceReader reader = IAsyncBinaryReader.Create(bytes);
        Span<byte> messageKey = stackalloc byte[16];
        reader.Read(messageKey);
        AesIge aesIge = new AesIge(_authKey, messageKey);
        using var messageData = UnmanagedMemoryPool<byte>.Shared.Rent((int)reader.RemainingSequence.Length);
        var messageSpan = messageData.Memory.Span.Slice(0, (int)reader.RemainingSequence.Length);
        reader.Read(messageSpan);
        aesIge.Decrypt(messageSpan);

        var messageKeyActual = AesIge.GenerateMessageKey(_authKey, messageSpan, true);
        if (!messageKey.SequenceEqual(messageKeyActual))
        {
            var ex = new MTProtoSecurityException("The security check for the 'msg_key' failed.");
            _log.Fatal(ex, ex.Message);
            throw ex;
        }
        SequenceReader rd = IAsyncBinaryReader.Create(messageData.Memory);
        TLExecutionContext _context = new TLExecutionContext(SessionData);
        _context.Salt = rd.ReadInt64(true);
        _context.SessionId = rd.ReadInt64(true);
        _context.AuthKeyId = _authKeyId;
        _context.PermAuthKeyId = _permAuthKeyId;
        _context.MessageId = rd.ReadInt64(true);
        _context.SequenceNo = rd.ReadInt32(true);
        if (socketConnection.RemoteEndPoint is IPEndPoint endpoint)
        {
            _context.IP = endpoint.Address.ToString();
        }
        if (requiresQuickAck)
        {
            _context.QuickAck = GenerateQuickAck(messageSpan);
        }
        int messageDataLength = rd.ReadInt32(true);
        int constructor = rd.ReadInt32(true);
        if (_sessionId == 0)
        {
            _sessionId = _context.SessionId;
            SessionState state = new SessionState();
            var salt = new ServerSaltDTO();
            state.SessionId = _sessionId;
            state.ServerSalt = salt;
            state.AuthKeyId = _authKeyId;
            state.AuthKey = _authKey;
            state.NodeId = _sessionManager.NodeId;
            _sessionManager.AddSession(state,
                new MTProtoSession(this));

            _uniqueSessionId = _random.NextLong();
            var newSessionCreated = factory.Resolve<NewSessionCreated>();
            newSessionCreated.FirstMsgId = _context.MessageId;
            newSessionCreated.ServerSalt = salt.Salt;
            newSessionCreated.UniqueId = _uniqueSessionId;
            MTProtoMessage newSessionMessage = new MTProtoMessage();
            newSessionMessage.Data = newSessionCreated.TLBytes.ToArray();
            newSessionMessage.IsContentRelated = false;
            newSessionMessage.IsResponse = false;
            newSessionMessage.SessionId = _sessionId;
            newSessionMessage.MessageType = MTProtoMessageType.NewSession;
            _ = SendAsync(newSessionMessage);
        }

        if (_context.MessageId < _time.ThirtySecondsLater &&
            //msg_id values that belong over 30 seconds in the future
            _context.MessageId > _time.FiveMinutesAgo &&
            //or over 300 seconds in the past are to be ignored
            _context.MessageId % 2 == 0 && //must have even parity
            (_lastMessageIds.Count == 0 || (!_lastMessageIds.Contains(_context.MessageId) && //must not be equal to any
                                            _context.MessageId > _lastMessageIds.Min()))) //must not be lower than all
        {
            _lastMessageIds.Enqueue(_context.MessageId);

            try
            {
                var msg = factory.Read(constructor, ref rd);
                _processorManager.Process(this, msg, _context);
                OnMessageReceived(new MTProtoAsyncEventArgs(msg, _context));
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
            }
        }
    }

    private int GenerateQuickAck(Span<byte> messageSpan)
    {
        var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        sha256.AppendData(_authKey.AsSpan().Slice(88, 32));
        sha256.AppendData(messageSpan);
        var ack = sha256.GetCurrentHash();
        return BitConverter.ToInt32(ack, 0);
    }

    private void ProcessUnencryptedMessage(in ReadOnlySequence<byte> bytes)
    {
        if (bytes.Length < 16)
        {
            return;
        }
        SequenceReader reader = IAsyncBinaryReader.Create(bytes);
        long msgId = reader.ReadInt64(true);
        int messageDataLength = reader.ReadInt32(true);
        if (messageDataLength > reader.RemainingSequence.Length)
        {
            return;
        }

        var messageData = UnmanagedMemoryAllocator.Allocate<byte>(messageDataLength, false);
        reader.Read(messageData.Span);
        //int constructor = reader.ReadInt32(true);
        //var msg = factory.Read(constructor, ref reader);
        //TODO: We should probably use a pool for the MTProtoAsyncEventArgs
        TLExecutionContext _context = new TLExecutionContext(SessionData);
        if (socketConnection.RemoteEndPoint is IPEndPoint endpoint)
        {
            _context.IP = endpoint.Address.ToString();
        }
        _context.MessageId = msgId;
        _context.AuthKeyId = _authKeyId;
        _context.PermAuthKeyId = _permAuthKeyId;
        _processorManager.Process(this, messageData, _context);
        //_processorManager.Process(this, msg, _context);
        //OnMessageReceived(new MTProtoAsyncEventArgs(msg, _context));
    }

    private void ProcessFrame(ReadOnlySequence<byte> bytes, bool requiresQuickAck)
    {
        if (bytes.Length < 8)
        {
            return;
        }
        SequenceReader reader = IAsyncBinaryReader.Create(bytes);
        long keyId = reader.ReadInt64(true);
        if (keyId != 0 && keyId != _authKeyId)
        {
            _authKey = null;
            _authKeyId = keyId;
            GetAuthKey();
        }
        
        if (keyId == 0)
        {
            ProcessUnencryptedMessage(bytes.Slice(8));
        }
        else
        {
            ProcessEncryptedMessageAsync(bytes.Slice(8), requiresQuickAck);
        }
    }
    private int GenerateSeqNo(bool isContentRelated)
    {
        return isContentRelated ? (2 * _seq++) + 1 : 2 * _seq;
    }
    /// <summary>
    /// Gets the next Message Identifier (msg_id) for this session.
    /// </summary>
    /// <param name="response">If the message is a response to a client message.</param>
    /// <returns></returns>
    private long NextMessageId(bool response)
    {
        long id = _time.GetUnixTimeInSeconds();
        id *= 4294967296L;
        long r1 = (4 - id % 4) % 4;
        id += (response ? r1 + 1 : r1 + 3);
        long last = _lastMessageId;
        long r2 = 4 - (last + 1) % 4;
        if (id <= last)
        {
            id = Interlocked.Add(ref _lastMessageId,
                response ? r2 + 2 : r2 + 4);
            if ((response && id % 4 == 1) || (!response && id % 4 == 3))
            {
                return id;
            }
        }
        else if (Interlocked.CompareExchange(ref _lastMessageId, id, last) == last)
        {
            return id;
        }
        do
        {
            r2 = 4 - (_lastMessageId + 1) % 4;
            id = Interlocked.Add(ref _lastMessageId, response ? r2 + 2 : r2 + 4);
        } while (!((response && id % 4 != 1) || (!response && id % 4 != 3)));
        return id;
    }
}


