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
using Ferrite.Core.RequestChain;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.TL;
using Ferrite.TL.currentLayer.storage;
using Ferrite.TL.currentLayer.upload;
using Ferrite.TL.mtproto;
using Ferrite.Utils;
using TLConstructor = Ferrite.TL.currentLayer.TLConstructor;

namespace Ferrite.Core;

public class StreamHandler : IStreamHandler
{
    private readonly ILogger _log;
    private readonly ITLHandler _requestChain;
    private readonly ITLObjectFactory _factory;
    private MTProtoPipe? _currentRequest;
    private readonly SparseBufferWriter<byte> _writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly IRandomGenerator _random;
    
    public StreamHandler(ILogger log, ITLHandler requestChain,
        ITLObjectFactory factory, IRandomGenerator random)
    {
        _log = log;
        _requestChain = requestChain;
        _factory = factory;
        _random = random;
    }

    public async Task HandleOutgoingStream(IFileOwner message, MTProtoConnection connection,
        MTProtoSession session)
    {
        if (message == null) return;
        var rpcResult = _factory.Resolve<RpcResult>();
        rpcResult.ReqMsgId = message.ReqMsgId;
        var data = await message.GetFileStream();
        if (data.Length < 0) return;
        _log.Debug($"=>Stream data length is {data.Length}.");
        var fileImpl = _factory.Resolve<FileImpl>();
        fileImpl.Type = _factory.Resolve<FileJpegImpl>();
        fileImpl.Mtime = (int)DateTimeOffset.Now.ToUnixTimeSeconds();
        fileImpl.Bytes = Array.Empty<byte>();
        rpcResult.Result = fileImpl;
        byte[] resultHeader = new byte[24 + (data.Length < 254 ? 1 : 4)];
        rpcResult.TLBytes.Slice(0, 24).CopyTo(resultHeader);
        int pad = 0;
        if (data.Length < 254)
        {
            resultHeader[24] = (byte)data.Length;
            pad = (int)((4 - ((data.Length + 1) % 4)) % 4);
        }
        else
        {
            resultHeader[24] = 254;
            resultHeader[25] = (byte)(data.Length & 0xff);
            resultHeader[26] = (byte)((data.Length >> 8) & 0xff);
            resultHeader[27] = (byte)((data.Length >> 16) & 0xff);
            pad = (int)((4 - ((data.Length + 4) % 4)) % 4);
        }
        _writer.Clear();
        _writer.WriteInt64(session.ServerSalt.Salt, true);
        _writer.WriteInt64(session.SessionId, true);
        _writer.WriteInt64(session.NextMessageId(true), true);
        _writer.WriteInt32(session.GenerateSeqNo(true), true);
        _writer.WriteInt32(resultHeader.Length + (int)data.Length + pad, true);
        var cryptographicHeader = _writer.ToReadOnlySequence().ToArray();
        int paddingLength = _random.GetNext(12, 512);
        while ((resultHeader.Length + data.Length + pad + paddingLength) % 16 != 0)
        {
            paddingLength++;
        }
        var paddingBytes = _random.GetRandomBytes(paddingLength);
        Queue<Stream> streams = new Queue<Stream>();
        streams.Enqueue(new MemoryStream(cryptographicHeader));
        streams.Enqueue(new MemoryStream(resultHeader));
        streams.Enqueue(data);
        streams.Enqueue(new MemoryStream(new byte[pad]));
        streams.Enqueue(new MemoryStream(paddingBytes));
        var stream = new ConcatenatedStream(streams, 0, Int32.MaxValue);

        var messageKey = AesIge.GenerateMessageKey(session.AuthKey, stream).ToArray();
        byte[] aesKey = new byte[32];
        byte[] aesIV = new byte[32];
        AesIge.GenerateAesKeyAndIV(session.AuthKey, messageKey, false, aesKey, aesIV);
        //--
        _writer.Clear();
        _writer.WriteInt64(session.AuthKeyId, true);
        _writer.Write(messageKey);
        connection.WriteFrameHeader((int)(24 + stream.Length));
        var header = _writer.ToReadOnlySequence();
        connection.WriteFrameBlock(header);
        //--
        MTProtoPipe pipe = new MTProtoPipe(aesKey, aesIV, true);
        await pipe.WriteAsync(cryptographicHeader);
        await pipe.WriteAsync(resultHeader);
        var dataStream = await message.GetFileStream();
        int remaining = (int)dataStream.Length;;
        var buffer = new byte[1024];
        while (remaining > 0)
        {
            var read = await dataStream.ReadAsync(buffer.AsMemory(0, Math.Min(remaining, 1024)));
            await pipe.WriteAsync(new ReadOnlySequence<byte>(buffer, 0, read));
            remaining -= read;
            var encrypted = await pipe.Input.ReadAsync();
            connection.WriteFrameBlock(encrypted.Buffer);
            pipe.Input.AdvanceTo(encrypted.Buffer.End);
            await connection.FlushSocketAsync();
        }
        if (pad > 0)
        {
            await pipe.WriteAsync(new byte[pad]);
        }
        if (paddingLength > 0)
        {
            await pipe.WriteAsync(paddingBytes);
        }

        await pipe.CompleteAsync();

        var readResult = await pipe.Input.ReadAsync();
        connection.WriteFrameBlock(readResult.Buffer);
        pipe.Input.AdvanceTo(readResult.Buffer.End);
        while (!readResult.IsCompleted)
        {
            readResult = await pipe.Input.ReadAsync();
            connection.WriteFrameBlock(readResult.Buffer);
            pipe.Input.AdvanceTo(readResult.Buffer.End);
        }
        connection.WriteFrameTail();
    }

    public ValueTask DisposeAsync()
    {
        if (_currentRequest != null) return _currentRequest.DisposeAsync();
        return default;
    }
}