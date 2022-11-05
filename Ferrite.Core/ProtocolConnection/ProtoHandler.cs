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
using System.Net;
using DotNext.Buffers;
using DotNext.IO;
using DotNext.IO.Pipelines;
using Ferrite.Core.Exceptions;
using Ferrite.Core.RequestChain;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.currentLayer.upload;
using Ferrite.TL.slim;
using Ferrite.Utils;

namespace Ferrite.Core;

public class ProtoHandler : IProtoHandler
{
    private readonly ILogger _log;
    private readonly ITLHandler _requestChain;
    private readonly ITLObjectFactory _factory;
    private readonly IRandomGenerator _random;
    private MTProtoPipe? _currentRequest;
    public MTProtoSession Session { get; set; }
    private readonly SparseBufferWriter<byte> _writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    
    public ProtoHandler(ILogger log, ITLObjectFactory factory, 
        IRandomGenerator random)
    {
        _log = log;
        _factory = factory;
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
        AesIge aesIge = new(Session.AuthKey, messageKey);
        var messageData = UnmanagedMemoryPool<byte>.Shared.Rent((int)bytes.Length - 16);
        var messageSpan = messageData.Memory.Span[..((int)bytes.Length - 16)];
        bytes.Slice(16).CopyTo(messageSpan);
        aesIge.Decrypt(messageSpan);

        var messageKeyActual = AesIge.GenerateMessageKey(Session.AuthKey, messageSpan, true);
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
            return new ProtoMessage
            {
                AuthKeyId = Session.AuthKeyId,
                Salt = salt,
                SessionId = sessionId,
                MessageId = messageId,
                SequenceNo = sequenceNo,
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
            MessageId = messageId,
            MessageData = data,
        };
    }

    public ReadOnlySequence<byte> EncryptMessage(MTProtoMessage message)
    {
        if(message.Data == null) throw new ArgumentNullException();
        _writer.Clear();
        _writer.WriteInt64(Session.ServerSalt.Salt, true);
        _writer.WriteInt64(message.SessionId, true);
        _writer.WriteInt64(message.MessageType == MTProtoMessageType.Pong ?
            message.MessageId :
            Session.NextMessageId(message.IsResponse), true);
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
        var msg = _writer.ToReadOnlySequence();
        return msg;
    }

    public ReadOnlySequence<byte> PreparePlaintextMessage(MTProtoMessage message)
    {
        if(message.Data == null) throw new ArgumentNullException();
        _writer.Clear();
        _writer.WriteInt64(0, true);
        _writer.WriteInt64(Session.NextMessageId(message.IsContentRelated), true);
        _writer.WriteInt32(message.Data.Length, true);
        _writer.Write(message.Data);
        var msg = _writer.ToReadOnlySequence();
        return msg;
    }

    public async Task<StreamingProtoMessage?> ProcessIncomingStreamAsync(ReadOnlySequence<byte> bytes, bool hasMore)
    {
        bool first = false;
        if (_currentRequest == null)
        {
            first = true;
            var incomingMessageKey = bytes.Slice(8,16).ToArray();
            var aesKey = new byte[32];
            var aesIV = new byte[32];
            AesIge.GenerateAesKeyAndIV(Session.AuthKey, incomingMessageKey, true, aesKey, aesIV);
            _currentRequest = new MTProtoPipe(aesKey, aesIV, false);
            await _currentRequest.WriteAsync(bytes.Slice(24));
        }
        else
        { 
            await _currentRequest.WriteAsync(bytes);
        }

        var context = await CreateExecutionContext();
        if (context == null) return null;

        if (!hasMore)
        {
            var resp = new StreamingProtoMessage(_currentRequest, context, first, !hasMore);
            _currentRequest.Complete();
            await _currentRequest.DisposeAsync();
            _currentRequest = null;
            return resp;
        }
        else
        {
            return new StreamingProtoMessage(_currentRequest, context, first, !hasMore);
        }
    }
    
    private async Task<TLExecutionContext?> CreateExecutionContext()
    {
        TLExecutionContext context = new(Session.SessionData)
        {
            Salt = await _currentRequest!.Input.ReadInt64Async(true),
            SessionId = await _currentRequest!.Input.ReadInt64Async(true),
            AuthKeyId = Session.AuthKeyId,
            PermAuthKeyId = Session.PermAuthKeyId,
            MessageId = await _currentRequest!.Input.ReadInt64Async(true),
            SequenceNo = await _currentRequest!.Input.ReadInt32Async(true),
            IP = Session.EndPoint?.Address.ToString() ?? string.Empty
        };

        int messageDataLength = await _currentRequest!.Input.ReadInt32Async(true);
        int constructor = await _currentRequest!.Input.ReadInt32Async(true);
        if (Session.SessionId == 0)
        {
            Session.CreateNewSession(context.SessionId, context.MessageId);
        }

        if (Session.IsValidMessageId(context.MessageId) && (
                constructor == TL.currentLayer.TLConstructor.Upload_SaveFilePart ||
                constructor == TL.currentLayer.TLConstructor.Upload_SaveBigFilePart))
        {
            return context;
        }

        return null;
    }

    public Task ProcessOutgoingStream(IFileOwner message)
    {
        throw new NotImplementedException();
    }
}