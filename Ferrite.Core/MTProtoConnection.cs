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

    public MTProtoConnection(ISocketConnection connection, ITLObjectFactory objectFactory, ITransportDetector detector)
    {
        socketConnection = connection;
        TransportType = MTProtoTransport.Unknown;
        factory = objectFactory;
        transportDetector = detector;
    }

    public void Start()
    {
        receiveTask = Receive();
        sendTask = Send();
    }

    private async Task Receive()
    {
        try
        {
            while (true)
            {
                var result = await socketConnection.Transport.Input.ReadAsync();
                if (result.Buffer.Length > 0)
                {
                    var position = Process(result.Buffer);
                    socketConnection.Transport.Input.AdvanceTo(position);
                }
            }
        }
        catch (Exception ex)
        {

        }
    }

    public async Task Send(ITLObject message)
    {
        if (message != null)
        {
            await _outgoing.Writer.WriteAsync(message);
        }
    }
    
    private async Task Send()
    {
        try
        {
            while (true)
            {
                try
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
                        socketConnection.Transport.Output.Write(encoder.Encode(writer.ToReadOnlySequence()));
                        _ = await socketConnection.Transport.Output.FlushAsync();
                    }
                }
                catch (Exception ex)
                {

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
            if (reader.TryReadLittleEndian(out int firstInt) &&
                firstInt == WebSocketGet)
            {
                reader.Rewind(4);
                ProcessWebSocketHandshake(ref reader);
                return reader.Position;
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
            var resp = webSocketHandler.GetHandshakeResponse();
            var data = Encoding.ASCII.GetBytes(resp);
            Send(data);
        }
    }

    private async void Send(byte[] data)
    {
        socketConnection.Transport.Output.Write(data);
        _ = await socketConnection.Transport.Output.FlushAsync();
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


