using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Net.Http.Headers;

namespace Ferrite.Transport
{
    public class WebSocketHandler : IHttpRequestLineHandler, IHttpHeadersHandler
    {
        

        public IHeaderDictionary RequestHeaders { get; } = new HeaderDictionary();
        public bool RequestLineComplete { get; private set; }
        public bool HeadersComplete { get; private set; }
        public HttpVersion Version { get; private set; }

        //---
        // https://github.com/dotnet/aspnetcore/blob/main/src/Middleware/WebSockets/src/HandshakeHelpers.cs
        // Licensed to the .NET Foundation under one or more agreements.
        // The .NET Foundation licenses this file to you under the MIT license.
        // "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
        // This uses C# compiler's ability to refer to static data directly. For more information see https://vcsjones.dev/2019/02/01/csharp-readonly-span-bytes-static
        private static ReadOnlySpan<byte> EncodedWebSocketKey => new byte[]
        {
            (byte)'2', (byte)'5', (byte)'8', (byte)'E', (byte)'A', (byte)'F', (byte)'A', (byte)'5', (byte)'-',
            (byte)'E', (byte)'9', (byte)'1', (byte)'4', (byte)'-', (byte)'4', (byte)'7', (byte)'D', (byte)'A',
            (byte)'-', (byte)'9', (byte)'5', (byte)'C', (byte)'A', (byte)'-', (byte)'C', (byte)'5', (byte)'A',
            (byte)'B', (byte)'0', (byte)'D', (byte)'C', (byte)'8', (byte)'5', (byte)'B', (byte)'1', (byte)'1'
        };

        private static ReadOnlySpan<byte> CRLF => new byte[] { (byte)'\r', (byte)'\n' };
        private static ReadOnlySpan<byte> Seperator => new byte[] { (byte)':', (byte)' ' };
        private static ReadOnlySpan<byte> Http11 => new byte[]
        {
            (byte)'H', (byte)'T', (byte)'T', (byte)'P', (byte)'/', (byte)'1', (byte)'.', (byte)'1', (byte)' '
        };
        private static ReadOnlySpan<byte> Http2 => new byte[]
        {
            (byte)'H', (byte)'T', (byte)'T', (byte)'P', (byte)'/', (byte)'2', (byte)' '
        };
        private static ReadOnlySpan<byte> Http3 => new byte[]
        {
            (byte)'H', (byte)'T', (byte)'T', (byte)'P', (byte)'/', (byte)'3', (byte)' '
        };
        private static ReadOnlySpan<byte> Resp101 => new byte[]
        {
            (byte)'1', (byte)'0', (byte)'1', (byte)' ',
            (byte)'S', (byte)'w', (byte)'i', (byte)'t', (byte)'c',(byte)'h', (byte)'i', (byte)'n', (byte)'g', (byte)' ',
            (byte)'P',(byte)'r', (byte)'o', (byte)'t', (byte)'o', (byte)'c', (byte)'o',(byte)'l', (byte)'s', (byte)'\r', (byte)'\n'
        };
        private static ReadOnlySpan<byte> ConnectionUpgrade => new byte[]
        {
            (byte)'C', (byte)'o', (byte)'n', (byte)'n',(byte)'e', (byte)'c', (byte)'t', (byte)'i', (byte)'o', (byte)'n',
            (byte)':', (byte)' ',
            (byte)'u', (byte)'p', (byte)'g', (byte)'r',(byte)'a', (byte)'d', (byte)'e', (byte)'\r', (byte)'\n'
        };
        private static ReadOnlySpan<byte> UpgradeWebsocket => new byte[]
        {
            (byte)'U', (byte)'p', (byte)'g', (byte)'r',(byte)'a', (byte)'d', (byte)'e',
            (byte)':', (byte)' ',
            (byte)'w',(byte)'e', (byte)'b', (byte)'s', (byte)'o', (byte)'c', (byte)'k',(byte)'e', (byte)'t', (byte)'\r', (byte)'\n'
        };

        private static ReadOnlySpan<byte> WebSocketAccept => new byte[]
        {
            (byte)'S', (byte)'e', (byte)'c', (byte)'-',(byte)'W', (byte)'e', (byte)'b',
            (byte)'S', (byte)'o', (byte)'c',(byte)'k', (byte)'e', (byte)'t', (byte)'-',
            (byte)'A', (byte)'c',(byte)'c', (byte)'e', (byte)'p', (byte)'t',
        };

        private static ReadOnlySpan<byte> WebSocketProtocol => new byte[]
        {
            (byte)'S', (byte)'e', (byte)'c', (byte)'-',(byte)'W', (byte)'e', (byte)'b',
            (byte)'S', (byte)'o', (byte)'c',(byte)'k', (byte)'e', (byte)'t', (byte)'-',
            (byte)'P', (byte)'r',(byte)'o', (byte)'t', (byte)'o', (byte)'c', (byte)'o', (byte)'l'
        };

        // Verify Method, Upgrade, Connection, version,  key, etc..
        public static void GenerateResponseHeaders(string key, string? subProtocol, IHeaderDictionary headers)
        {
            headers.Connection = "upgrade";
            headers.Upgrade = "websocket";
            headers.SecWebSocketAccept = CreateResponseKey(key);

            if (!string.IsNullOrWhiteSpace(subProtocol))
            {
                headers.SecWebSocketProtocol = subProtocol;
            }
        }
        /// <summary>
        /// Validates the Sec-WebSocket-Key request header
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsRequestKeyValid(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            Span<byte> temp = stackalloc byte[16];
            var success = Convert.TryFromBase64String(value, temp, out var written);
            return success && written == 16;
        }
        public static string CreateResponseKey(string requestKey)
        {
            // "The value of this header field is constructed by concatenating /key/, defined above in step 4
            // in Section 4.2.2, with the string "258EAFA5-E914-47DA-95CA-C5AB0DC85B11", taking the SHA-1 hash of
            // this concatenated value to obtain a 20-byte value and base64-encoding"
            // https://tools.ietf.org/html/rfc6455#section-4.2.2

            // requestKey is already verified to be small (24 bytes) by 'IsRequestKeyValid()' and everything is 1:1 mapping to UTF8 bytes
            // so this can be hardcoded to 60 bytes for the requestKey + static websocket string
            Span<byte> mergedBytes = stackalloc byte[60];
            Encoding.UTF8.GetBytes(requestKey, mergedBytes);
            EncodedWebSocketKey.CopyTo(mergedBytes[24..]);

            Span<byte> hashedBytes = stackalloc byte[20];
            var written = SHA1.HashData(mergedBytes, hashedBytes);
            if (written != 20)
            {
                throw new InvalidOperationException("Could not compute the hash for the 'Sec-WebSocket-Accept' header.");
            }

            return Convert.ToBase64String(hashedBytes);
        }
        //---

        public void WriteHandshakeResponseTo(PipeWriter output)
        {
            if (HeadersComplete && RequestLineComplete &&
                RequestHeaders.Connection.ToString().ToLowerInvariant() == "upgrade" &&
                RequestHeaders.Upgrade.ToString().ToLowerInvariant() == "websocket")
            {
                var websocketKey = RequestHeaders.SecWebSocketKey.ToString();
                var protocol = RequestHeaders.SecWebSocketProtocol.ToString();
                if (IsRequestKeyValid(websocketKey))
                {
                    
                    if (Version == HttpVersion.Http11)
                    {
                        output.Write(Http11);
                    }
                    else if (Version == HttpVersion.Http2)
                    {
                        output.Write(Http2);
                    }
                    else if (Version == HttpVersion.Http3)
                    {
                        output.Write(Http3);
                    }
                    output.Write(Resp101);
                    output.Write(ConnectionUpgrade);
                    output.Write(UpgradeWebsocket);
                    output.Write(WebSocketAccept);
                    output.Write(Seperator);
                    output.Write(Encoding.UTF8.GetBytes(CreateResponseKey(websocketKey)));
                    output.Write(CRLF);
                    if (!string.IsNullOrWhiteSpace(protocol))
                    {
                        output.Write(WebSocketProtocol);
                        output.Write(Seperator);
                        output.Write(Encoding.UTF8.GetBytes(protocol));
                        output.Write(CRLF);
                    }
                    output.Write(CRLF);
                }
            }
        }

        public void WriteHeaderTo(PipeWriter output, long length)
        {
            if (length <= 125)
            {
                Span<byte> header = stackalloc byte[2];
                header[0] = 0b10000010;
                header[1] = (byte)length;
                output.Write(header);
            }
            else if (length <= 65535)
            {
                Span<byte> header = stackalloc byte[4];
                header[0] = 0b10000010;
                header[1] = (byte)126;
                BinaryPrimitives.WriteUInt16BigEndian(header.Slice(2), (ushort)length);
                output.Write(header);
            }
            else
            {
                Span<byte> header = stackalloc byte[10];
                header[0] = 0b10000010;
                header[1] = (byte)127;
                BinaryPrimitives.WriteUInt64BigEndian(header.Slice(2), (ulong)length);
                output.Write(header);
            }
        }

        // https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_server#decoding_messages
        public SequencePosition DecodeTo(in ReadOnlySequence<byte> buff, PipeWriter output)
        {
            SequenceReader<byte> reader = new SequenceReader<byte>(buff);
            Span<byte> bytes = stackalloc byte[10];
            if (reader.Remaining < 2)
            {
                return reader.Position;
            }
            for (int i = 0; i <2; i++)
            {
                reader.TryRead(out var b);
                bytes[i] = b;
            }
            bool fin = (bytes[0] & 0b10000000) != 0,
                mask = (bytes[1] & 0b10000000) != 0;

            int opcode = bytes[0] & 0b00001111,
                msglen = bytes[1] - 128,
                offset = 2;

            if (msglen == 126)
            {
                if (reader.Remaining < 2)
                {
                    reader.Rewind(offset);
                    return reader.Position;
                }
                for (int i = 2; i < 4; i++)
                {
                    reader.TryRead(out var b);
                    bytes[i] = b;
                }
                msglen = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(2));
                offset = 4;
            }
            else if (msglen == 127)
            {
                if (reader.Remaining < 8)
                {
                    reader.Rewind(offset);
                    return reader.Position;
                }
                for (int i = 2; i < 10; i++)
                {
                    reader.TryRead(out byte b);
                    bytes[i] = b;
                }
                msglen = (int)BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(2));
                offset = 10;
            }
            if (msglen > reader.Remaining)
            {
                reader.Rewind(offset);
                return reader.Position;
            }
            if (mask)
            {
                Span<byte> masks = stackalloc byte[4];
                Span<byte> d = stackalloc byte[1];
                for (int i = 0; i < 4; i++)
                {
                    reader.TryRead(out var b);
                    masks[i] = b;
                }
                offset += 4;

                for (int i = 0; i < msglen; ++i)
                {
                    reader.TryRead(out byte b);
                    d[0] = (byte)(b ^ masks[i % 4]);
                    output.Write(d);
                }
            }
            
            return reader.Position;
        }

        public void OnHeader(ReadOnlySpan<byte> name, ReadOnlySpan<byte> value)
        {
            string key = name.GetHeaderName();
            var valueStr = value.GetRequestHeaderString(key, _ => Encoding.ASCII, checkForNewlineChars: false);
            RequestHeaders.Append(key, valueStr);
        }

        public void OnHeadersComplete(bool endStream)
        {
            HeadersComplete = true;
        }

        public void OnStartLine(HttpVersionAndMethod versionAndMethod, TargetOffsetPathLength targetPath, Span<byte> startLine)
        {
            if (versionAndMethod.Method == HttpMethod.Get &&
                versionAndMethod.Version >= HttpVersion.Http11)
            {
                Version = versionAndMethod.Version;
                RequestLineComplete = true;
            }
            else
            {
                throw new Exception("Invalid request.");
            }
        }

        public void OnStaticIndexedHeader(int index)
        {
            throw new NotImplementedException();
        }

        public void OnStaticIndexedHeader(int index, ReadOnlySpan<byte> value)
        {
            throw new NotImplementedException();
        }
    }
}

