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
using Ferrite.Core.Exceptions;
using Ferrite.Crypto;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.Transport;
using Ferrite.Utils;

namespace Ferrite.Core;

public class MessageHandler : IMessageHandler
{
    private readonly ILogger _log;
    private readonly IProcessorManager _processorManager;
    private readonly ITLObjectFactory _factory;
    private readonly IRandomGenerator _random;
    private readonly SparseBufferWriter<byte> _writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    
    public MessageHandler(ILogger log, IProcessorManager processorManager,
        ITLObjectFactory factory, IRandomGenerator random)
    {
        _log = log;
        _processorManager = processorManager;
        _factory = factory;
        _random = random;
    }
    public void HandleIncomingMessage(in ReadOnlySequence<byte> bytes, MTProtoConnection connection, 
        EndPoint? endPoint, MTProtoSession session, bool requiresQuickAck)
    {
        if (bytes.Length < 16)
        {
            return;
        }
        SequenceReader reader = IAsyncBinaryReader.Create(bytes);
        Span<byte> messageKey = stackalloc byte[16];
        reader.Read(messageKey);
        AesIge aesIge = new AesIge(session.AuthKey, messageKey);
        using var messageData = UnmanagedMemoryPool<byte>.Shared.Rent((int)reader.RemainingSequence.Length);
        var messageSpan = messageData.Memory.Span.Slice(0, (int)reader.RemainingSequence.Length);
        reader.Read(messageSpan);
        aesIge.Decrypt(messageSpan);

        var messageKeyActual = AesIge.GenerateMessageKey(session.AuthKey, messageSpan, true);
        if (!messageKey.SequenceEqual(messageKeyActual))
        {
            var ex = new MTProtoSecurityException("The security check for the 'msg_key' failed.");
            _log.Fatal(ex, ex.Message);
            throw ex;
        }
        SequenceReader rd = IAsyncBinaryReader.Create(messageData.Memory);
        TLExecutionContext context = new TLExecutionContext(session.SessionData);
        context.Salt = rd.ReadInt64(true);
        context.SessionId = rd.ReadInt64(true);
        context.AuthKeyId = session.AuthKeyId;
        context.PermAuthKeyId = session.PermAuthKeyId;
        context.MessageId = rd.ReadInt64(true);
        context.SequenceNo = rd.ReadInt32(true);
        if (endPoint is IPEndPoint ep)
        {
            context.IP = ep.Address.ToString();
        }
        if (requiresQuickAck)
        {
            context.QuickAck = session.GenerateQuickAck(messageSpan);
        }
        int messageDataLength = rd.ReadInt32(true);
        int constructor = rd.ReadInt32(true);
        if (session.SessionId == 0)
        {
            session.CreateNewSession(context.SessionId, 
                context.MessageId, 
                connection);
        }

        if (session.IsValidMessageId(context.MessageId))
        {
            try
            {
                var msg = _factory.Read(constructor, ref rd);
                _processorManager.Process(connection, msg, context);
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
            }
        }
    }
    
    public void HandleOutgoingMessage(MTProtoMessage message, MTProtoConnection connection,
        MTProtoSession session, IFrameEncoder encoder, WebSocketHandler? webSocketHandler)
    {
        if (message.Data == null) { return; }
        _writer.Clear();
        _writer.WriteInt64(session.ServerSalt.Salt, true);
        _writer.WriteInt64(message.SessionId, true);
        _writer.WriteInt64(message.MessageType == MTProtoMessageType.Pong ?
            message.MessageId :
            session.NextMessageId(message.IsResponse), true);
        _writer.WriteInt32(session.GenerateSeqNo(message.IsContentRelated), true);
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
        Span<byte> messageKey = AesIge.GenerateMessageKey(session.AuthKey, messageSpan);
        AesIge aesIge = new AesIge(session.AuthKey, messageKey, false);
        aesIge.Encrypt(messageSpan);
        _writer.Clear();
        _writer.WriteInt64(session.AuthKeyId, true);
        _writer.Write(messageKey);
        _writer.Write(messageSpan);
        var msg = _writer.ToReadOnlySequence();
        var encoded = encoder.Encode(msg);
        if (webSocketHandler != null)
        {
            webSocketHandler.WriteHeaderTo(connection.TransportConnection.Transport.Output, encoded.Length);
        }
        connection.TransportConnection.Transport.Output.Write(encoded);
    }
}