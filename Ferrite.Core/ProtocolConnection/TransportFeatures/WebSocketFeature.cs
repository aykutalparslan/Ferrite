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
using Ferrite.Transport;

namespace Ferrite.Core.ProtocolConnection.TransportFeatures;

public class WebSocketFeature : IWebSocketFeature
{
    public bool WebSocketHandshakeCompleted { get; private set; }
    private WebSocketHandler _handler;

    public WebSocketHandler? WebSocketHandler => WebSocketHandshakeCompleted ? _handler : null;

    public PipeReader WebSocketReader { get; }
    private readonly Pipe _webSocketPipe;

    public WebSocketFeature()
    {
        _handler = new();
        _webSocketPipe = new Pipe();
        WebSocketReader = _webSocketPipe.Reader;
    }
    public HandshakeResponse ProcessWebSocketHandshake(ReadOnlySequence<byte> data)
    {
        var pos = ParseHeaders(data);
        if (!_handler.HeadersComplete) return new HandshakeResponse(pos, 
            new ReadOnlySequence<byte>(), false);
        var response = _handler.GenerateHandshakeResponse();
        WebSocketHandshakeCompleted = true;
        return new HandshakeResponse(pos, 
            response, true);
    }

    public async ValueTask<SequencePosition> DecodeWebSocketData(ReadOnlySequence<byte> buffer)
    {
        var pos = _handler.DecodeTo(buffer, _webSocketPipe.Writer);
        await _webSocketPipe.Writer.FlushAsync();
        return pos;
    }

    public ReadOnlySequence<byte> GenerateWebSocketHeader(int length)
    {
        if (!WebSocketHandshakeCompleted) return new ReadOnlySequence<byte>();
        var header = WebSocketHandler.GenerateHeader(length);
        return new ReadOnlySequence<byte>(header);
    }

    private SequencePosition ParseHeaders(ReadOnlySequence<byte> data)
    {
        var reader = new SequenceReader<byte>(data);
        HttpParser<WebSocketHandler> parser = new HttpParser<WebSocketHandler>();
        if (!_handler.RequestLineComplete)
        {
            parser.ParseRequestLine(_handler, ref reader);
        }
        parser.ParseHeaders(_handler, ref reader);
        return reader.Position;
    }
}