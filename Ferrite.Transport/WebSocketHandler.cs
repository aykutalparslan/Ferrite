using System;
using System.Buffers;
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

        // Verify Method, Upgrade, Connection, version,  key, etc..
        public static void GenerateResponseHeaders(string key, string? subProtocol, IHeaderDictionary headers)
        {
            headers["Connection"] = "upgrade";
            headers["Upgrade"] = "websocket";
            headers["Sec-WebSocket-Accept"] = CreateResponseKey(key);

            if (!string.IsNullOrWhiteSpace(subProtocol))
            {
                headers["Sec-WebSocket-Protocol"] = subProtocol;
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

        // TODO: optimize this
        public string GetHandshakeResponse()
        {
            string resp = "";
            if (HeadersComplete && RequestLineComplete &&
                RequestHeaders["Connection"].ToString().ToLowerInvariant() == "upgrade" &&
                RequestHeaders["Upgrade"].ToString().ToLowerInvariant() == "websocket")
            {
                var websocketKey = RequestHeaders["Sec-WebSocket-Key"].ToString();
                var protocol = RequestHeaders["Sec-WebSocket-Protocol"].ToString();
                if (IsRequestKeyValid(websocketKey))
                {
                    HeaderDictionary responseHeaders = new();
                    GenerateResponseHeaders(websocketKey, protocol, responseHeaders);
                    resp = HttpUtilities.VersionToString(Version) + " 101 Switching Protocols\r\n";
                    foreach (var header in responseHeaders)
                    {
                        resp += header.Key + ": " + header.Value + "\r\n";
                    }
                    resp+="\r\n";
                }
            }
            return resp;
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

