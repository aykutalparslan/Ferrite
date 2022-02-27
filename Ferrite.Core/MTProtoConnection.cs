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

namespace Ferrite.Core;

public class MTProtoConnection
{
    public MTProtoTransport TransportType { get; private set; }

    const byte Abridged = 0xef;
    const int AbridgedInt = unchecked((int)0xefefefef);
    const int Intermediate = unchecked((int)0xeeeeeeee);
    const int PaddedIntermediate = unchecked((int)0xdddddddd);
    const int Full = unchecked((int)0xdddddddd);
    private readonly byte[] _lengthBytesAbridged = new byte[4];
    private int _sequenceTcpFull;
    private long _authKeyId;
    private ISocketConnection socketConnection;
    private Task? receiveTask;
    //private Task? sendTask;
    private ITLObjectFactory factory;


    public MTProtoConnection(ISocketConnection connection, ITLObjectFactory objectFactory)
    {
        socketConnection = connection;
        TransportType = MTProtoTransport.Unknown;
        factory = objectFactory;
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

    private async Task SendUnencrypted(ITLObject message)
    {
        try
        {
            socketConnection.Transport.Output.Write(message.TLBytes);
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
    private SequencePosition Process(in ReadOnlySequence<byte> buffer)
    {
        SequenceReader<byte> reader = new SequenceReader<byte>(buffer);
        if (TransportType == MTProtoTransport.Unknown)
        {
            DetectTransport(ref reader);
        }

        bool hasMore = false;
        do
        {
            if (TransportType == MTProtoTransport.Abridged)
            {
                hasMore = ReadFrameAbridged(ref reader);
            }
            else if (TransportType == MTProtoTransport.Intermediate)
            {
                hasMore = ReadFrameIntermediate(ref reader);
            }
            else if (TransportType == MTProtoTransport.PaddedIntermediate)
            {
                hasMore = ReadFramePaddedIntermediate(ref reader);
            }
            else if (TransportType == MTProtoTransport.Full)
            {
                hasMore = ReadFrameFull(ref reader);
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
        long messageId  = reader.ReadInt64(true);
        int messageDataLength = reader.ReadInt32(true);
        int constructor = reader.ReadInt32(true);
        var msg = factory.Read(constructor, ref reader);
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

    private bool ReadFrameAbridged(ref SequenceReader<byte> reader)
    {
        if(reader.Remaining == 0)
        {
            return false;
        }
        long len = 0;
        reader.TryRead(out var firstbyte);
        if(firstbyte < 127)
        {
            len = firstbyte * 4;
        }
        else if(firstbyte == 127)
        {
            if (reader.Remaining < 3)
            {
                return false;
            }
            if (reader.TryCopyTo(_lengthBytesAbridged))
            {
                var tmp = new SpanReader<byte>(_lengthBytesAbridged);
                len = tmp.ReadInt32(true) * 4;              
            }
            if (reader.Remaining < len)
            {
                return false;
            }
            ReadOnlySequence<byte> frame = reader.UnreadSequence.Slice(0, len);
            reader.Advance(len);
            ProcessFrame(frame);
            if(reader.Remaining != 0)
            {
                return true;
            }
        }
        return false;
    }

    private bool ReadFrameIntermediate(ref SequenceReader<byte> reader)
    {
        if (reader.Remaining < 4)
        {
            return false;
        }
        reader.TryReadLittleEndian(out int len);
        if(reader.Remaining < len)
        {
            return false;
        }
        ReadOnlySequence<byte> frame = reader.UnreadSequence.Slice(0, len);
        reader.Advance(len);
        ProcessFrame(frame);
        if (reader.Remaining != 0)
        {
            return true;
        }
        return false;
    }

    private bool ReadFramePaddedIntermediate(ref SequenceReader<byte> reader)
    {
        if (reader.Remaining < 4)
        {
            return false;
        }
        reader.TryReadLittleEndian(out int len);
        if (reader.Remaining < len)
        {
            return false;
        }
        ReadOnlySequence<byte> frame = reader.UnreadSequence.Slice(0, len);
        reader.Advance(len);
        ProcessFrame(frame);
        if (reader.Remaining != 0)
        {
            return true;
        }
        return false;
    }

    private bool ReadFrameFull(ref SequenceReader<byte> reader)
    {
        if (reader.Remaining < 4)
        {
            return false;
        }
        reader.TryReadLittleEndian(out int len);
        if (reader.Remaining < len)
        {
            return false;
        }
        reader.Rewind(4);
        uint crc32 = reader.UnreadSequence.Slice(0,len).GetCrc32();
        reader.Advance(4);
        reader.TryReadLittleEndian(out int seq);
        _sequenceTcpFull = seq;
        ReadOnlySequence<byte> frame = reader.Sequence.Slice(reader.Position, len-12);
        reader.Advance(len-12);
        reader.TryReadLittleEndian(out int checksum);
        if(crc32 == (uint)checksum)
        {
            ProcessFrame(frame);
        }
        if (reader.Remaining != 0)
        {
            return true;
        }
        return false;
    }

    private void DetectTransport(ref SequenceReader<byte> reader)
    {
        if(reader.Remaining > 0)
        {
            reader.TryPeek(out var firstbyte);
            if(firstbyte == Abridged)
            {
                TransportType = MTProtoTransport.Abridged;
                reader.Advance(1);
                return;
            }
        }

        if (reader.Remaining > 3)
        {
            reader.TryReadLittleEndian(out int firstint);
            if (firstint == Intermediate)
            {
                TransportType = MTProtoTransport.Intermediate;
                return;
            } else if (firstint == PaddedIntermediate)
            {
                TransportType = MTProtoTransport.PaddedIntermediate;
                return;
            } 
        }

        if (reader.Remaining > 7)
        {
            reader.TryReadLittleEndian(out int secondint);
            if (secondint == Full)
            {
                TransportType = MTProtoTransport.Full;
                reader.Rewind(8);
                return;
            }
        }
        if(reader.Remaining > 63)
        {
            reader.Advance(48);
            reader.TryReadLittleEndian(out int identifier);
            if (identifier == AbridgedInt)
            {
                TransportType = MTProtoTransport.Abridged;
                reader.Advance(4);
                return;
            } else if(identifier == Intermediate)
            {
                TransportType = MTProtoTransport.Intermediate;
                reader.Advance(4);
                return;
            } else if (identifier == PaddedIntermediate)
            {
                TransportType = MTProtoTransport.PaddedIntermediate;
                reader.Advance(4);
                return;
            } else
            {
                TransportType = MTProtoTransport.Full;
                reader.Advance(4);
                return;
            }
        }
    }
}


