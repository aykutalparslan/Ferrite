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
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.Transport;

namespace Ferrite.Core;

public class ProtoTransport : IQuickAckFeature,
    ITransportErrorFeature, IWebSocketFeature
{
    private readonly IQuickAckFeature _quickAck;
    private readonly ITransportErrorFeature _transportError;
    private readonly IWebSocketFeature _webSocket;

    public ProtoTransport(IQuickAckFeature quickAck,
        ITransportErrorFeature transportError,
        IWebSocketFeature webSocket)
    {
        _quickAck = quickAck;
        _transportError = transportError;
        _webSocket = webSocket;
    }

    public bool WebSocketHandshakeCompleted => _webSocket.WebSocketHandshakeCompleted;
    public PipeReader WebSocketReader => _webSocket.WebSocketReader;

    public HandshakeResponse ProcessWebSocketHandshake(ReadOnlySequence<byte> data)
    {
        return _webSocket.ProcessWebSocketHandshake(data);
    }

    public async ValueTask<SequencePosition> DecodeWebSocketData(ReadOnlySequence<byte> buffer)
    {
        return await _webSocket.DecodeWebSocketData(buffer);
    }

    public ReadOnlySequence<byte> GenerateWebSocketHeader(int length)
    {
        return _webSocket.GenerateWebSocketHeader(length);
    }

    public ReadOnlySequence<byte> GenerateQuickAck(int ack, MTProtoTransport transport)
    {
        return _quickAck.GenerateQuickAck(ack, transport);
    }

    public ReadOnlySequence<byte> GenerateTransportError(int errorCode)
    {
        return _transportError.GenerateTransportError(errorCode);
    }
}