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

namespace Ferrite.Core.Features;

public class WebSocketFeature : IWebSocketFeature
{
    public bool HandshakeCompleted { get; private set; }
    public Handler Handler { get; }
    public PipeReader Reader { get; }
    private Pipe _webSocketPipe;
    private readonly ITransportConnection _connection;

    public WebSocketFeature(ITransportConnection connection)
    {
        _connection = connection;
        Handler = new();
        _webSocketPipe = new Pipe();
        Reader = _webSocketPipe.Reader;
    }
    public async ValueTask<SequencePosition> ProcessWebSocketHandshake(ReadOnlySequence<byte> data)
    {
        var pos = ParseHeaders(data);
        if (!Handler.HeadersComplete)return pos;
        Handler.WriteHandshakeResponseTo(_connection.Transport.Output);
        await _connection.Transport.Output.FlushAsync();
        HandshakeCompleted = true;
        return pos;
    }

    public async ValueTask<SequencePosition> Decode(ReadOnlySequence<byte> buffer)
    {
        var pos = Handler.DecodeTo(buffer, _webSocketPipe.Writer);
        await _webSocketPipe.Writer.FlushAsync();
        return pos;
    }

    private SequencePosition ParseHeaders(ReadOnlySequence<byte> data)
    {
        var reader = new SequenceReader<byte>(data);
        HttpParser<Handler> parser = new HttpParser<Handler>();
        if (!Handler.RequestLineComplete)
        {
            parser.ParseRequestLine(Handler, ref reader);
        }
        parser.ParseHeaders(Handler, ref reader);
        return reader.Position;
    }
}