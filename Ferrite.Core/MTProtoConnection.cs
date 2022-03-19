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
using DotNext.IO;
using DotNext.Buffers;
using Ferrite.TL;
using System.Threading.Channels;
using Ferrite.Transport;
using System.Text;
using System.IO.Pipelines;

namespace Ferrite.Core;

public class MTProtoConnection
{
    public MTProtoTransport TransportType { get; private set; }
    private readonly ITransportDetector transportDetector;
    private IFrameDecoder decoder;
    private IFrameEncoder encoder;
    private long _authKeyId;
    private ISocketConnection socketConnection;
    private Task? receiveTask;
    private Channel<ITLObject> _outgoing = Channel.CreateUnbounded<ITLObject>();
    private Task? sendTask;
    private readonly ITLObjectFactory factory;
    private long _lastMessageId;
    private SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private TLExecutionContext _context = new TLExecutionContext();
    private const int WebSocketGet = 542393671;
    private byte[] CRLF = new byte[] { (byte)'\r', (byte)'\n' };
    private WebSocketHandler webSocketHandler;
    private Pipe webSocketPipe;

    public MTProtoConnection(ISocketConnection connection, ITLObjectFactory objectFactory, ITransportDetector detector)
    {
        socketConnection = connection;
        TransportType = MTProtoTransport.Unknown;
        factory = objectFactory;
        transportDetector = detector;
    }

    public void Start()
    {
        receiveTask = DoReceive();
        sendTask = DoSend();
    }

    private async Task DoReceive()
    {
        try
        {
            while (true)
            {
                var result = await socketConnection.Transport.Input.ReadAsync();
                if (result.IsCanceled)
                {
                    break;
                }
                if (result.Buffer.Length > 0)
                {
                    if (webSocketHandler != null)
                    {
                        if (webSocketPipe == null)
                        {
                            webSocketPipe = new Pipe();
                        }
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
        }
        catch (Exception ex)
        {

        }
    }

    public async Task SendAsync(ITLObject message)
    {
        if (message != null)
        {
            await _outgoing.Writer.WriteAsync(message);
        }
    }

    private async void SendAsync(byte[] data)
    {
        socketConnection.Transport.Output.Write(data);
        _ = await socketConnection.Transport.Output.FlushAsync();
    }

    private async Task DoSend()
    {
        try
        {
            while (true)
            {
                var msg = await _outgoing.Reader.ReadAsync();
                if (msg != null)
                {
                    var data = msg.TLBytes;
                    writer.Clear();
                    writer.WriteInt64(0, true);
                    writer.WriteInt64(GenerateMessageId(true), true);
                    writer.WriteInt32((int)data.Length, true);
                    writer.Write(data, false);
                    var message = writer.ToReadOnlySequence();
                    var encoded = encoder.Encode(message);
                    if (webSocketHandler != null)
                    {
                        webSocketHandler.WriteHeaderTo(socketConnection.Transport.Output, encoded.Length);
                    }
                    socketConnection.Transport.Output.Write(encoded);
                    var result = await socketConnection.Transport.Output.FlushAsync();
                    if(result.IsCompleted ||
                        result.IsCanceled)
                    {
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {

        }
    }
    

    public event EventHandler<MTProtoAsyncEventArgs> MessageReceived;

    protected virtual void OnMessageReceived(MTProtoAsyncEventArgs e)
    {
        EventHandler<MTProtoAsyncEventArgs> raiseEvent = MessageReceived;

        if (raiseEvent != null)
        {
            raiseEvent(this, e);
        }
    }

    private SequencePosition Process(in ReadOnlySequence<byte> buffer)
    {
        SequenceReader<byte> reader = new SequenceReader<byte>(buffer);
        if (TransportType == MTProtoTransport.Unknown)
        {
            if (reader.TryReadLittleEndian(out int firstInt))
            {
                reader.Rewind(4);
                if (firstInt == WebSocketGet)
                {
                    ProcessWebSocketHandshake(ref reader);
                    return reader.Position;
                }
            }

            TransportType = transportDetector.DetectTransport(ref reader,
            out decoder, out encoder);
        }

        bool hasMore = false;
        do
        {
            hasMore = decoder.Decode(ref reader, out var frame);
            if (frame.Length > 0)
            {
                ProcessFrame(frame);
            }
        } while (hasMore);
        
        return reader.Position;
    }

    private void ProcessWebSocketHandshake(ref SequenceReader<byte> reader)
    {
        if(webSocketHandler == null)
        {
            webSocketHandler = new();
        }
        HttpParser<WebSocketHandler> parser = new HttpParser<WebSocketHandler>();
        if (!webSocketHandler.RequestLineComplete)
        {
            parser.ParseRequestLine(webSocketHandler, ref reader);
        }
        parser.ParseHeaders(webSocketHandler, ref reader);
        if (webSocketHandler.HeadersComplete )
        {
            webSocketHandler.WriteHandshakeResponseTo(socketConnection.Transport.Output);
            socketConnection.Transport.Output.FlushAsync();
        }
    }

    private void ProcessEncryptedMessage(in ReadOnlySequence<byte> bytes)
    {

    }

    private void ProcessUnencryptedMessage(in ReadOnlySequence<byte> bytes)
    {
        SequenceReader reader = IAsyncBinaryReader.Create(bytes);
        long msgId  = reader.ReadInt64(true);
        int messageDataLength = reader.ReadInt32(true);
        int constructor = reader.ReadInt32(true);
        var msg = factory.Read(constructor, ref reader);
        OnMessageReceived(new MTProtoAsyncEventArgs(msg, _context, messageId:msgId));
    }

    private void ProcessFrame(in ReadOnlySequence<byte> bytes)
    {
        if (bytes.Length < 8)
        {
            return;
        }
        SequenceReader<byte> reader = new SequenceReader<byte>(bytes);
        reader.TryReadLittleEndian(out long authKey);
        _authKeyId = authKey;
        if (authKey == 0)
        {
            ProcessUnencryptedMessage(bytes.Slice(8));
        }
        else
        {
            ProcessEncryptedMessage(bytes.Slice(8));
        }
    }
    private long GenerateMessageId(bool response)
    {
        long id = (long)new TimeSpan(DateTime.Now.Ticks).TotalSeconds;
        id *= 4294967296L;
        if (id <= _lastMessageId)
        {
            id = ++_lastMessageId;
        }
        if (response)
        {
            while (id % 4 != 1)
            {
                id++;
            }
        }
        else
        {
            while (id % 4 != 3)
            {
                id++;
            }
        }
        _lastMessageId = id;
        return id;
    }
}


