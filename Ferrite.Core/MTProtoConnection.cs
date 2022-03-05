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
    //private Task? sendTask;
    private readonly ITLObjectFactory factory;
    

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
        //sendTask = Send();
    }

    private async Task Receive()
    {
        try
        {
            while (true)
            {
                var result = await socketConnection.Transport.Input.ReadAsync();
                var position = Process(result.Buffer);
                socketConnection.Transport.Input.AdvanceTo(position);
            }
        }
        catch (Exception ex)
        {

        }
    }

    public async Task SendUnencrypted(ITLObject message)
    {
        try
        {
            var data = message.TLBytes;

            socketConnection.Transport.Output.Write(encoder.Encode(data));
            _ = await socketConnection.Transport.Output.FlushAsync();
        }
        catch (Exception ex)
        {

        }
    }
    /*
    private async Task Send()
    {
        try
        {
            while (true)
            {

            }
        }
        catch (Exception ex)
        {

        }
    }
    */

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
        OnMessageReceived(new MTProtoAsyncEventArgs(msg, messageId:msgId));
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
}


