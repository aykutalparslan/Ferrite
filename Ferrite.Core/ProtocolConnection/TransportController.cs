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
using System.IO.Pipelines;
using DotNext.Buffers;
using Ferrite.Core.Features;
using Ferrite.Core.Framing;
using Ferrite.TL;
using Ferrite.Transport;

namespace Ferrite.Core;

public class TransportController : IQuickAckFeature,
    ITransportErrorFeature, INotifySessionCreatedFeature,
    IWebSocketFeature
{
    private readonly IQuickAckFeature _quickAck;
    private readonly ITransportErrorFeature _transportError;
    private readonly INotifySessionCreatedFeature _notifySessionCreated;
    private readonly IWebSocketFeature _webSocket;

    public TransportController(IQuickAckFeature quickAck,
        ITransportErrorFeature transportError,
        INotifySessionCreatedFeature notifySessionCreated,
        IWebSocketFeature webSocket)
    {
        _quickAck = quickAck;
        _transportError = transportError;
        _notifySessionCreated = notifySessionCreated;
        _webSocket = webSocket;
    }

    public void SendQuickAck(int ack, SparseBufferWriter<byte> writer, IFrameEncoder encoder, IWebSocketFeature webSocket,
        MTProtoConnection connection)
    {
        _quickAck.SendQuickAck(ack, writer, encoder, webSocket, connection);
    }

    public void SendTransportError(int errorCode, SparseBufferWriter<byte> writer, IFrameEncoder encoder,
        IWebSocketFeature webSocket,
        MTProtoConnection connection)
    {
        _transportError.SendTransportError(errorCode, writer, encoder, webSocket, connection);
    }

    public void NotifySessionCreated(ITLObjectFactory factory, MTProtoConnection connection, MTProtoSession session,
        long firstMessageId,
        long serverSalt)
    {
        _notifySessionCreated.NotifySessionCreated(factory, connection, session, firstMessageId, serverSalt);
    }

    public bool WebSocketHandshakeCompleted => _webSocket.WebSocketHandshakeCompleted;
    public PipeReader WebSocketReader => _webSocket.WebSocketReader;

    public async ValueTask<SequencePosition> ProcessWebSocketHandshake(ReadOnlySequence<byte> data)
    {
        return await _webSocket.ProcessWebSocketHandshake(data);
    }

    public async ValueTask<SequencePosition> DecodeWebSocketData(ReadOnlySequence<byte> buffer)
    {
        return await _webSocket.DecodeWebSocketData(buffer);
    }

    public void WriteWebSocketHeader(int length)
    {
        _webSocket.WriteWebSocketHeader(length);
    }
}