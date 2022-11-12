// 
// Project Ferrite is an Implementation of the Telegram Server API
// Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 

using System.Buffers;
using DotNext.Buffers;
using DotNext.IO;
using DotNext.IO.Pipelines;
using Ferrite.Core.Exceptions;
using Ferrite.Core.RequestChain;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.slim;
using Ferrite.Utils;

namespace Ferrite.Core.Connection;

public class ProtoHandler : IProtoHandler
{
    private readonly ILogger _log;
    private readonly IRandomGenerator _random;
    private MTProtoPipe? _currentRequest;
    public IMTProtoSession? Session { get; set; }
    private readonly SparseBufferWriter<byte> _writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    
    public ProtoHandler(ILogger log, IRandomGenerator random)
    {
        _log = log;
        _random = random;
    }
    public ProtoMessage DecryptMessage(in ReadOnlySequence<byte> bytes)
    {
        if (bytes.Length < 16)
        {
            throw new ArgumentOutOfRangeException();
        }
        Span<byte> messageKey = stackalloc byte[16];
        bytes.Slice(0, 16).CopyTo(messageKey);
        AesIge aesIge = new(Session?.AuthKey, messageKey);
        var messageData = UnmanagedMemoryPool<byte>.Shared.Rent((int)bytes.Length - 16);
        var messageSpan = messageData.Memory.Span[..((int)bytes.Length - 16)];
        bytes.Slice(16).CopyTo(messageSpan);
        aesIge.Decrypt(messageSpan);

        var messageKeyActual = AesIge.GenerateMessageKey(Session?.AuthKey, messageSpan, true);
        if (!messageKey.SequenceEqual(messageKeyActual))
        {
            var ex = new MTProtoSecurityException("The security check for the 'msg_key' failed.");
            _log.Fatal(ex, ex.Message);
            throw ex;
        }
        SpanReader<byte> rd = new(messageSpan);
        var salt = rd.ReadInt64(true);
        var sessionId = rd.ReadInt64(true);
        var messageId = rd.ReadInt64(true);
        var sequenceNo = rd.ReadInt32(true);
        
        int messageDataLength = rd.ReadInt32(true);
        if (messageDataLength < rd.RemainingCount)
        {
            var data = new TLBytes(messageData, 32, messageDataLength);
            if (Session != null)
                return new ProtoMessage
                {
                    Headers = new ProtoHeaders(authKeyId: Session.AuthKeyId, salt: salt, sessionId: sessionId,
                        messageId: messageId, sequenceNo: sequenceNo),
                    MessageData = data,
                };
        }

        throw new MTProtoSecurityException("Could not decrypt the message.");
    }

    public ProtoMessage ReadPlaintextMessage(in ReadOnlySequence<byte> bytes)
    {
        if (bytes.Length < 16)
        {
            throw new ArgumentOutOfRangeException();
        }
        SequenceReader reader = IAsyncBinaryReader.Create(bytes);
        long messageId = reader.ReadInt64(true);
        int messageDataLength = reader.ReadInt32(true);
        if (messageDataLength > reader.RemainingSequence.Length)
        {
            throw new MTProtoSecurityException("Inconsistent messageDataLength.");
        }

        var messageData = UnmanagedMemoryAllocator.Allocate<byte>(messageDataLength);
        reader.Read(messageData.Span);
        var data = new TLBytes(messageData, 0, messageDataLength);
        return new ProtoMessage
        {
            Headers = new ProtoHeaders
            {
                MessageId = messageId,
            },        
            MessageData = data,
        };
    }

    public ReadOnlySequence<byte> EncryptMessage(MTProtoMessage message)
    {
        if(message.Data == null) throw new ArgumentNullException();
        _writer.Clear();
        if (Session != null)
        {
            _writer.WriteInt64(Session.ServerSalt.Salt, true);
            _writer.WriteInt64(message.SessionId, true);
            _writer.WriteInt64(
                message.MessageType == MTProtoMessageType.Pong
                    ? message.MessageId
                    : Session.NextMessageId(message.IsResponse), true);
            _writer.WriteInt32(Session.GenerateSeqNo(message.IsContentRelated), true);
            _writer.WriteInt32(message.Data.Length, true);
            _writer.Write(message.Data);
            int paddingLength = _random.GetNext(12, 512);
            while ((message.Data.Length + paddingLength) % 16 != 0)
            {
                paddingLength++;
            }

            _writer.Write(_random.GetRandomBytes(paddingLength), false);

            using var messageData = UnmanagedMemoryPool<byte>.Shared.Rent((int)_writer.WrittenCount);
            var messageSpan = messageData.Memory.Slice(0, (int)_writer.WrittenCount).Span;
            _writer.ToReadOnlySequence().CopyTo(messageSpan);
            Span<byte> messageKey = AesIge.GenerateMessageKey(Session.AuthKey, messageSpan);
            AesIge aesIge = new AesIge(Session.AuthKey, messageKey, false);
            aesIge.Encrypt(messageSpan);
            _writer.Clear();
            _writer.WriteInt64(Session.AuthKeyId, true);
            _writer.Write(messageKey);
            _writer.Write(messageSpan);
        }

        var msg = _writer.ToReadOnlySequence();
        return msg;
    }

    public ReadOnlySequence<byte> PreparePlaintextMessage(MTProtoMessage message)
    {
        if(message.Data == null) throw new ArgumentNullException();
        _writer.Clear();
        _writer.WriteInt64(0, true);
        if (Session != null) _writer.WriteInt64(Session.NextMessageId(message.IsContentRelated), true);
        _writer.WriteInt32(message.Data.Length, true);
        _writer.Write(message.Data);
        var msg = _writer.ToReadOnlySequence();
        return msg;
    }

    public async ValueTask<StreamingProtoMessage> ProcessIncomingStreamAsync(ReadOnlySequence<byte> bytes, bool hasMore)
    {
        bool first = false;
        if (_currentRequest == null)
        {
            first = true;
            var incomingMessageKey = bytes.Slice(8,16).ToArray();
            var aesKey = new byte[32];
            var aesIV = new byte[32];
            AesIge.GenerateAesKeyAndIV(Session?.AuthKey, incomingMessageKey, true, aesKey, aesIV);
            _currentRequest = new MTProtoPipe(aesKey, aesIV, false);
            await _currentRequest.WriteAsync(bytes.Slice(24));
        }
        else
        { 
            await _currentRequest.WriteAsync(bytes);
        }
        StreamingProtoMessage resp = StreamingProtoMessage.Default;
        if (first)
        {
            var salt = await _currentRequest.Input.ReadInt64Async(true);
            var sessionId = await _currentRequest.Input.ReadInt64Async(true);
            var messageId = await _currentRequest.Input.ReadInt64Async(true);
            var sequenceNo = await _currentRequest.Input.ReadInt32Async(true);
            if (Session != null)
                resp = new StreamingProtoMessage
                {
                    Headers = new ProtoHeaders
                    {
                        AuthKeyId = Session.AuthKeyId,
                        Salt = salt,
                        SessionId = sessionId,
                        MessageId = messageId,
                        SequenceNo = sequenceNo,
                    },
                    MessageData = _currentRequest,
                };
        }

        if (hasMore) return resp;
        _currentRequest.Complete();
        _currentRequest = null;
        return resp;
    }

    public async ValueTask<ValueTuple<int, ReadOnlySequence<byte>, MTProtoPipe>> GenerateOutgoingStream(IFileOwner? message)
    {
        if (message == null) throw new ArgumentNullException();
        var data = await message.GetFileStream();
        if (data.Length < 0) throw new IOException();
        _log.Debug($"=>Stream data length is {data.Length}.");
        var resultHeader = GenerateResultHeader(message, data, out var pad);
        var cryptographicHeader = GenerateCryptographicHeader(resultHeader, data, pad);
        var (paddingLength, paddingBytes) = GeneratePadding(resultHeader, data, pad);
        var (streamLength, messageKey) = GenerateMessageKey(cryptographicHeader, resultHeader, data, pad, paddingBytes);
        var (aesKey, aesIV) = GenerateAesKeyAndIV(messageKey);
        _writer.Clear();
        if (Session != null) _writer.WriteInt64(Session.AuthKeyId, true);
        _writer.Write(messageKey);
        int frameLength = 24 + streamLength;
        var frameHeader = _writer.ToReadOnlySequence();
        
        MTProtoPipe pipe = new(aesKey, aesIV, true);
        _ = WriteStreamToPipe(message, pipe, cryptographicHeader, resultHeader, pad, paddingLength, paddingBytes);
        return (frameLength, frameHeader, pipe);
    }

    private static byte[] GenerateResultHeader(IFileOwner message, Stream data, out int pad)
    {
        var resultHeader = new byte[24 + (data.Length < 254 ? 1 : 4)];
        message.TLObjectHeader.AsSpan().CopyTo(resultHeader);
        pad = data.Length < 254
            ? (int)((4 - ((data.Length + 1) % 4)) % 4)
            : (int)((4 - ((data.Length + 4) % 4)) % 4);
        if (data.Length < 254)
        {
            resultHeader[24] = (byte)data.Length;
        }
        else
        {
            resultHeader[24] = 254;
            resultHeader[25] = (byte)(data.Length & 0xff);
            resultHeader[26] = (byte)((data.Length >> 8) & 0xff);
            resultHeader[27] = (byte)((data.Length >> 16) & 0xff);
        }

        return resultHeader;
    }

    private static async Task WriteStreamToPipe(IFileOwner message, MTProtoPipe pipe, byte[] cryptographicHeader,
        byte[] resultHeader, int pad, int paddingLength, byte[] paddingBytes)
    {
        await pipe.WriteAsync(cryptographicHeader);
        await pipe.WriteAsync(resultHeader);
        var dataStream = await message.GetFileStream();
        int remaining = (int)dataStream.Length;
        ;
        var buffer = new byte[1024];
        while (remaining > 0)
        {
            var read = await dataStream.ReadAsync(buffer.AsMemory(0, Math.Min(remaining, 1024)));
            await pipe.WriteAsync(new ReadOnlySequence<byte>(buffer, 0, read));
            remaining -= read;
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
    }

    private ValueTuple<byte[], byte[]> GenerateAesKeyAndIV(byte[] messageKey)
    {
        byte[] aesKey = new byte[32];
        byte[] aesIV = new byte[32];
        AesIge.GenerateAesKeyAndIV(Session?.AuthKey, messageKey, false, aesKey, aesIV);
        return (aesKey, aesIV);
    }

    private ValueTuple<int, byte[]> GenerateMessageKey(byte[] cryptographicHeader, byte[] resultHeader, Stream data, int pad,
        byte[] paddingBytes)
    {
        Queue<Stream> streams = new Queue<Stream>();
        streams.Enqueue(new MemoryStream(cryptographicHeader));
        streams.Enqueue(new MemoryStream(resultHeader));
        streams.Enqueue(data);
        streams.Enqueue(new MemoryStream(new byte[pad]));
        streams.Enqueue(new MemoryStream(paddingBytes));
        var stream = new ConcatenatedStream(streams, 0, Int32.MaxValue);

        var messageKey = AesIge.GenerateMessageKey(Session?.AuthKey, stream).ToArray();
        return ((int)stream.Length, messageKey);
    }

    private ValueTuple<int, byte[]> GeneratePadding(byte[] resultHeader, Stream data, int pad)
    {
        int paddingLength = _random.GetNext(12, 512);
        while ((resultHeader.Length + data.Length + pad + paddingLength) % 16 != 0)
        {
            paddingLength++;
        }

        var paddingBytes = _random.GetRandomBytes(paddingLength);
        return (paddingLength, paddingBytes);
    }

    private byte[] GenerateCryptographicHeader(byte[] resultHeader, Stream data, int pad)
    {
        _writer.Clear();
        if (Session != null)
        {
            _writer.WriteInt64(Session.ServerSalt.Salt, true);
            _writer.WriteInt64(Session.SessionId, true);
            _writer.WriteInt64(Session.NextMessageId(true), true);
            _writer.WriteInt32(Session.GenerateSeqNo(true), true);
        }

        _writer.WriteInt32(resultHeader.Length + (int)data.Length + pad, true);
        var cryptographicHeader = _writer.ToReadOnlySequence().ToArray();
        return cryptographicHeader;
    }
}