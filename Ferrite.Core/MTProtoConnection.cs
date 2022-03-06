/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using System.Buffers;
using System.Text;
using DotNext.IO;
using DotNext.Buffers;
using Ferrite.Crypto;
using Ferrite.Transport;
using Ferrite.TL;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;
using System.Collections.Concurrent;
using System.Threading.Channels;

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

    

    private void ProcessEncryptedMessage(ReadOnlySequence<byte> bytes)
    {

    }

    private void ProcessUnencryptedMessage(ReadOnlySequence<byte> bytes)
    {
        SequenceReader reader = IAsyncBinaryReader.Create(bytes);
        long msgId  = reader.ReadInt64(true);
        int messageDataLength = reader.ReadInt32(true);
        int constructor = reader.ReadInt32(true);
        var msg = factory.Read(constructor, ref reader);
        OnMessageReceived(new MTProtoAsyncEventArgs(msg, _context, messageId:msgId));
    }

    private void ProcessFrame(ReadOnlySequence<byte> bytes)
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


