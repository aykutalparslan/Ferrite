/*
 *   Project Ferrite is an Implementation Telegram Server API
 *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Affero General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Affero General Public License for more details.
 *
 *   You should have received a copy of the GNU Affero General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using Ferrite.Transport;
using Xunit;

namespace Ferrite.Tests.Transport
{
    public class WebSocketTests
    {
        [Fact]
        public void ShouldCompleteHandshake()
        {
            string request = "GET /chat HTTP/1.1\r\n"+
                             "Host: server.example.com\r\n" +
                             "Upgrade: websocket\r\n" +
                             "Connection: Upgrade\r\n" +
                             "Sec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==\r\n" +
                             "Origin: http://example.com\r\n" +
                             "Sec-WebSocket-Version: 13\r\n" +
                             "\r\n";

            var requestBytes = new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes(request));
            var reader = new SequenceReader<byte>(requestBytes);
            HttpParser<WebSocketHandler> parser = new();
            WebSocketHandler webSocketHandler = new();
            parser.ParseRequestLine(webSocketHandler, ref reader);
            parser.ParseHeaders(webSocketHandler, ref reader);
            Pipe p = new Pipe();
            var resp = webSocketHandler.GenerateHandshakeResponse();
            p.Writer.Write(resp.ToArray());
            p.Writer.FlushAsync().AsTask().Wait();
            var result = p.Reader.ReadAsync().GetAwaiter().GetResult();
            string expected = "HTTP/1.1 101 Switching Protocols\r\n" +
                              "Connection: upgrade\r\n" +
                              "Upgrade: websocket\r\n" +
                              "Sec-WebSocket-Accept: s3pPLMBiTxaQ9kYGzzhZRbK+xOo=\r\n" +
                              "\r\n";
            Assert.Equal(expected, Encoding.UTF8.GetString(result.Buffer.ToArray()));
        }
        [Fact]
        public void ShouldCompleteHandshakeWithProtocol()
        {
            string request = "GET /chat HTTP/1.1\r\n" +
                             "Host: server.example.com\r\n" +
                             "Upgrade: websocket\r\n" +
                             "Connection: Upgrade\r\n" +
                             "Sec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==\r\n" +
                             "Origin: http://example.com\r\n" +
                             "Sec-WebSocket-Protocol: chat, superchat\r\n" +
                             "Sec-WebSocket-Version: 13\r\n" +
                             "\r\n";

            var requestBytes = new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes(request));
            var reader = new SequenceReader<byte>(requestBytes);
            HttpParser<WebSocketHandler> parser = new();
            WebSocketHandler webSocketHandler = new();
            parser.ParseRequestLine(webSocketHandler, ref reader);
            parser.ParseHeaders(webSocketHandler, ref reader);
            Pipe p = new Pipe();
            var resp = webSocketHandler.GenerateHandshakeResponse();
            p.Writer.Write(resp.ToArray());
            p.Writer.FlushAsync().AsTask().Wait();
            var result = p.Reader.ReadAsync().GetAwaiter().GetResult();
            string expected = "HTTP/1.1 101 Switching Protocols\r\n" +
                              "Connection: upgrade\r\n" +
                              "Upgrade: websocket\r\n" +
                              "Sec-WebSocket-Accept: s3pPLMBiTxaQ9kYGzzhZRbK+xOo=\r\n" +
                              "Sec-WebSocket-Protocol: chat, superchat\r\n" +
                              "\r\n";
            Assert.Equal(expected, Encoding.UTF8.GetString(result.Buffer.ToArray()));
        }
    }
}

