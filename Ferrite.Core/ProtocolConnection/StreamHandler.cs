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
using Ferrite.Core.Framing;
using Ferrite.Core.RequestChain;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.TL;
using Ferrite.TL.currentLayer.storage;
using Ferrite.TL.currentLayer.upload;
using Ferrite.TL.mtproto;
using Ferrite.Transport;
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
    public async Task HandleIncomingStreamAsync(ReadOnlySequence<byte> bytes, MTProtoConnection connection,
        EndPoint? endPoint, MTProtoSession session, bool hasMore)
    {
        SequenceReader reader = IAsyncBinaryReader.Create(bytes);
        if (_currentRequest == null)
        {
            if (bytes.Length < 24)
            {
                return;
            }
            
            long authKeyId = reader.ReadInt64(true);
            if (authKeyId != 0)
            {
                if (!session.TryFetchAuthKey(authKeyId) &&
                    session.AuthKeyId == 0)
                {
                    connection.SendTransportError(404);
                }
            }
            if (session.PermAuthKeyId != 0)
            {
                session.SaveCurrentSession(session.PermAuthKeyId, connection);
            }

            var incomingMessageKey = new byte[16];
            reader.Read(incomingMessageKey);
            var aesKey = new byte[32];
            var aesIV = new byte[32];
            AesIge.GenerateAesKeyAndIV(session.AuthKey, incomingMessageKey, true, aesKey, aesIV);
            _currentRequest = new MTProtoPipe(aesKey, aesIV, false);
            await _currentRequest.WriteAsync(reader);
            await ProcessPipe(_currentRequest, connection, endPoint, session);
        }
        else
        { 
            await _currentRequest.WriteAsync(reader);
        }

        if (!hasMore)
        {
            _currentRequest.Complete();
            await _currentRequest.DisposeAsync();
            _currentRequest = null;
        }
    }
    private async Task ProcessPipe(MTProtoPipe pipe, MTProtoConnection connection,
        EndPoint? endPoint, MTProtoSession session)
    {
        TLExecutionContext context = new TLExecutionContext(session.SessionData);
        context.Salt = await pipe.Input.ReadInt64Async(true);
        context.SessionId = await pipe.Input.ReadInt64Async(true);
        context.AuthKeyId = session.AuthKeyId;
        context.PermAuthKeyId = session.PermAuthKeyId;
        context.MessageId = await pipe.Input.ReadInt64Async(true);
        context.SequenceNo = await pipe.Input.ReadInt32Async(true);
        if (endPoint is IPEndPoint ep)
        {
            context.IP = ep.Address.ToString();
        }

        int messageDataLength = await pipe.Input.ReadInt32Async(true);
        int constructor = await pipe.Input.ReadInt32Async(true);
        if (session.SessionId == 0)
        {
            session.CreateNewSession(context.SessionId, context.MessageId, connection);
        }

        if (session.IsValidMessageId(context.MessageId))
        {
            try
            {
                if (constructor == TL.currentLayer.TLConstructor.Upload_SaveFilePart)
                {
                    var msg = _factory.Resolve<SaveFilePart>();
                    await msg.SetPipe(pipe);
                    _ = _requestChain.Process(connection, msg, context);
                }
                else if (constructor == TLConstructor.Upload_SaveBigFilePart)
                {
                    var msg = _factory.Resolve<SaveBigFilePart>();
                    await msg.SetPipe(pipe);
                    _ = _requestChain.Process(connection, msg, context);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
            }
        }
    }
    
    public async Task HandleOutgoingStream(IFileOwner message, MTProtoConnection connection,
        MTProtoSession session, IFrameEncoder encoder, Handler? webSocketHandler)
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
        var frameHead = encoder.EncodeHead(24 + (int)stream.Length);
        var header = _writer.ToReadOnlySequence();
        if (webSocketHandler != null)
        {
            webSocketHandler.WriteHeaderTo(connection.TransportConnection.Transport.Output, 
                frameHead.Length + 24 + (int)stream.Length);
        }
        connection.TransportConnection.Transport.Output.Write(encoder.EncodeBlock(frameHead));
        connection.TransportConnection.Transport.Output.Write(encoder.EncodeBlock(header));
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
            connection.TransportConnection.Transport.Output.Write(encoder.EncodeBlock(encrypted.Buffer));
            pipe.Input.AdvanceTo(encrypted.Buffer.End);
            await connection.TransportConnection.Transport.Output.FlushAsync();
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
        connection.TransportConnection.Transport.Output.Write(encoder.EncodeBlock(readResult.Buffer));
        pipe.Input.AdvanceTo(readResult.Buffer.End);
        while (!readResult.IsCompleted)
        {
            readResult = await pipe.Input.ReadAsync();
            connection.TransportConnection.Transport.Output.Write(encoder.EncodeBlock(readResult.Buffer));
            pipe.Input.AdvanceTo(readResult.Buffer.End);
        }

        var frameTail = encoder.EncodeTail();
        if (frameTail.Length > 0)
        {
            connection.TransportConnection.Transport.Output.Write(frameTail);
        }
    }

    public ValueTask DisposeAsync()
    {
        if (_currentRequest != null) return _currentRequest.DisposeAsync();
        return default;
    }
}