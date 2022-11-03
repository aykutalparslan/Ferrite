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
using Ferrite.Core.Features;
using Ferrite.Core.Framing;
using Ferrite.Core.RequestChain;
using Ferrite.Crypto;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.slim;
using Ferrite.Transport;

namespace Ferrite.Core;

public class UnencryptedMessageHandler : IUnencryptedMessageHandler
{
    private readonly ITLHandler _requestChain;
    private readonly SparseBufferWriter<byte> _writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly IRandomGenerator _random;
    public UnencryptedMessageHandler(ITLHandler requestChain, IRandomGenerator random)
    {
        _requestChain = requestChain;
        _random = random;
    }
    public void HandleIncomingMessage(in ReadOnlySequence<byte> bytes, 
        MTProtoConnection connection, MTProtoSession session)
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
        TLExecutionContext context = new(session.SessionData);
        if (connection.TransportConnection.RemoteEndPoint is IPEndPoint endpoint)
        {
            context.IP = endpoint.Address.ToString();
        }
        context.MessageId = msgId;
        context.AuthKeyId = session.AuthKeyId;
        context.PermAuthKeyId = session.PermAuthKeyId;
        _requestChain.Process(connection, new TLBytes(
            messageData, 0, messageDataLength), context);
    }
    
    public ReadOnlySequence<byte> GenerateOutgoingMessage(MTProtoMessage message, MTProtoSession session)
    {
        if(message.Data == null) throw new ArgumentNullException();
        _writer.Clear();
        _writer.WriteInt64(0, true);
        _writer.WriteInt64(session.NextMessageId(message.IsContentRelated), true);
        _writer.WriteInt32(message.Data.Length, true);
        _writer.Write(message.Data);
        var msg = _writer.ToReadOnlySequence();
        return msg;
    }
}