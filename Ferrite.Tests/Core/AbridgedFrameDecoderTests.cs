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
using System.Buffers.Binary;
using System.Security.Cryptography;
using Autofac.Extras.Moq;
using Ferrite.Core.Framing;
using Ferrite.Crypto;
using Ferrite.Services;
using Ferrite.TL.slim;
using Moq;
using Xunit;

namespace Ferrite.Tests.Core;

public class AbridgedFrameDecoderTests
{
    [Fact]
    public void Should_Decode_WithSingleLengthByte()
    {
        using var mock = AutoMock.GetLoose();
        var proto = mock.Mock<IMTProtoService>();
        proto.Setup(x => x.GetAuthKey(It.IsAny<long>()))
            .Returns(RandomNumberGenerator.GetBytes(192));
        proto.Setup(x => x.PutAuthKeyAsync(It.IsAny<long>(), 
            It.IsAny<byte[]>())).ReturnsAsync(true);

        var frame = new byte[125];
        Random.Shared.NextBytes(frame);
        frame[0] = 124/4;
        var decoder = new AbridgedFrameDecoder(proto.Object);
        var data = new ReadOnlySequence<byte>(frame);
        var hasMore = decoder.Decode(data,
            out var decodedFrame,
            out var isStream, 
            out bool requiresQuickAck, 
            out var position);
        Assert.Equal(frame[1..], 
            decodedFrame.ToArray());
        Assert.False(hasMore);
        Assert.False(isStream);
        Assert.False(requiresQuickAck);
        Assert.Equal(data.End, position);
    }
    [Fact]
    public void Should_DecodeObfuscated_WithSingleLengthByte()
    {
        using var mock = AutoMock.GetLoose();
        var proto = mock.Mock<IMTProtoService>();
        proto.Setup(x => x.GetAuthKey(It.IsAny<long>()))
            .Returns(RandomNumberGenerator.GetBytes(192));
        proto.Setup(x => x.PutAuthKeyAsync(It.IsAny<long>(), 
            It.IsAny<byte[]>())).ReturnsAsync(true);

        var frame = new byte[125];
        var encodedFrame = new byte[125];
        Random.Shared.NextBytes(frame);
        var key = new byte[32];
        Random.Shared.NextBytes(key);
        var iv = new byte[16];
        Random.Shared.NextBytes(iv);
        Aes256Ctr aes256Ctr = new (key.ToArray(), iv.ToArray());
        frame[0] = 124/4;
        aes256Ctr.Transform(new ReadOnlySequence<byte>(frame), encodedFrame);
        var decoder = new AbridgedFrameDecoder(new (key.ToArray(), iv.ToArray()), proto.Object);
        var data = new ReadOnlySequence<byte>(encodedFrame);
        var hasMore = decoder.Decode(data,
            out var decodedFrame,
            out var isStream, 
            out bool requiresQuickAck, 
            out var position);
        Assert.Equal(frame[1..], 
            decodedFrame.ToArray());
        Assert.False(hasMore);
        Assert.False(isStream);
        Assert.False(requiresQuickAck);
        Assert.Equal(data.End, position);
    }
    [Fact]
    public void Should_DecodeEmptyFrame_WithSingleLengthByte()
    {
        using var mock = AutoMock.GetLoose();
        var proto = mock.Mock<IMTProtoService>();
        proto.Setup(x => x.GetAuthKey(It.IsAny<long>()))
            .Returns(RandomNumberGenerator.GetBytes(192));
        proto.Setup(x => x.PutAuthKeyAsync(It.IsAny<long>(), 
            It.IsAny<byte[]>())).ReturnsAsync(true);

        var frame = new byte[65];
        Random.Shared.NextBytes(frame[1..]);
        frame[0] = 124/4;
        var decoder = new AbridgedFrameDecoder(proto.Object);
        var data = new ReadOnlySequence<byte>(frame);
        var hasMore = decoder.Decode(data,
            out var decodedFrame,
            out var isStream, 
            out bool requiresQuickAck, 
            out var position);
        Assert.Equal(Array.Empty<byte>(), 
            decodedFrame.ToArray());
        Assert.False(hasMore);
        Assert.False(isStream);
        Assert.False(requiresQuickAck);
    }
    [Fact]
    public void Should_DecodeEmptyFrame_WithNoData()
    {
        using var mock = AutoMock.GetLoose();
        var proto = mock.Mock<IMTProtoService>();
        proto.Setup(x => x.GetAuthKey(It.IsAny<long>()))
            .Returns(RandomNumberGenerator.GetBytes(192));
        proto.Setup(x => x.PutAuthKeyAsync(It.IsAny<long>(), 
            It.IsAny<byte[]>())).ReturnsAsync(true);

        var frame = Array.Empty<byte>();
        var decoder = new AbridgedFrameDecoder(proto.Object);
        var data = new ReadOnlySequence<byte>(frame);
        var hasMore = decoder.Decode(data,
            out var decodedFrame,
            out var isStream, 
            out bool requiresQuickAck, 
            out var position);
        Assert.Equal(Array.Empty<byte>(), 
            decodedFrame.ToArray());
        Assert.False(hasMore);
        Assert.False(isStream);
        Assert.False(requiresQuickAck);
    }
    [Fact]
    public void Should_Decode_MultipleFrames()
    {
        using var mock = AutoMock.GetLoose();
        var proto = mock.Mock<IMTProtoService>();
        proto.Setup(x => x.GetAuthKey(It.IsAny<long>()))
            .Returns(RandomNumberGenerator.GetBytes(192));
        proto.Setup(x => x.PutAuthKeyAsync(It.IsAny<long>(), 
            It.IsAny<byte[]>())).ReturnsAsync(true);

        var frames = new byte[125+2052];
        Random.Shared.NextBytes(frames);
        frames[0] = 124/4;
        int len = 2048 / 4;
        frames[125] = 0x7f;
        frames[126] = (byte)(len & 0xff);
        frames[127] = (byte)((len >> 8) & 0xff);
        frames[128] = (byte)((len >> 16) & 0xff);
        var decoder = new AbridgedFrameDecoder(proto.Object);
        var data = new ReadOnlySequence<byte>(frames);
        var hasMore = decoder.Decode(data,
            out var decodedFrame,
            out var isStream, 
            out bool requiresQuickAck, 
            out var position);
        Assert.Equal(frames[1..125], 
            decodedFrame.ToArray());
        Assert.True(hasMore);
        Assert.False(isStream);
        Assert.False(requiresQuickAck);
        hasMore = decoder.Decode(data.Slice(position),
            out decodedFrame,
            out isStream, 
            out requiresQuickAck, 
            out position);
        Assert.Equal(frames[129..], 
            decodedFrame.ToArray());
        Assert.False(hasMore);
        Assert.False(isStream);
        Assert.False(requiresQuickAck);
        Assert.Equal(data.End, position);
    }
    [Fact]
    public void Should_DecodeMultipleFrames_WithQuickAck()
    {
        using var mock = AutoMock.GetLoose();
        var proto = mock.Mock<IMTProtoService>();
        proto.Setup(x => x.GetAuthKey(It.IsAny<long>()))
            .Returns(RandomNumberGenerator.GetBytes(192));
        proto.Setup(x => x.PutAuthKeyAsync(It.IsAny<long>(), 
            It.IsAny<byte[]>())).ReturnsAsync(true);

        var frames = new byte[125+2052];
        Random.Shared.NextBytes(frames);
        frames[0] = 124/4;
        frames[0] |= 1 << 7;
        int len = 2048 / 4;
        frames[125] = 0x7f;
        frames[126] = (byte)(len & 0xff);
        frames[127] = (byte)((len >> 8) & 0xff);
        frames[128] = (byte)((len >> 16) & 0xff);
        frames[128] |= 1 << 7;
        var decoder = new AbridgedFrameDecoder(proto.Object);
        var data = new ReadOnlySequence<byte>(frames);
        var hasMore = decoder.Decode(data,
            out var decodedFrame,
            out var isStream, 
            out bool requiresQuickAck, 
            out var position);
        Assert.Equal(frames[1..125], 
            decodedFrame.ToArray());
        Assert.True(hasMore);
        Assert.False(isStream);
        Assert.True(requiresQuickAck);
        hasMore = decoder.Decode(data.Slice(position),
            out decodedFrame,
            out isStream, 
            out requiresQuickAck, 
            out position);
        Assert.Equal(frames[129..], 
            decodedFrame.ToArray());
        Assert.False(hasMore);
        Assert.False(isStream);
        Assert.True(requiresQuickAck);
        Assert.Equal(data.End, position);
    }
    [Fact]
    public void Should_DecodeWithFourLengthBytes()
    {
        using var mock = AutoMock.GetLoose();
        var proto = mock.Mock<IMTProtoService>();
        proto.Setup(x => x.GetAuthKey(It.IsAny<long>()))
            .Returns(RandomNumberGenerator.GetBytes(192));
        proto.Setup(x => x.PutAuthKeyAsync(It.IsAny<long>(), 
            It.IsAny<byte[]>())).ReturnsAsync(true);

        var frame = new byte[2052];
        Random.Shared.NextBytes(frame);
        int len = 2048 / 4;
        frame[0] = 0x7f;
        frame[1] = (byte)(len & 0xff);
        frame[2] = (byte)((len >> 8) & 0xff);
        frame[3] = (byte)((len >> 16) & 0xff);
        
        var decoder = new AbridgedFrameDecoder(proto.Object);
        var data = new ReadOnlySequence<byte>(frame);
        var hasMore = decoder.Decode(data,
            out var decodedFrame,
            out var isStream, 
            out bool requiresQuickAck, 
            out var position);
        Assert.Equal(frame[4..], 
            decodedFrame.ToArray());
        Assert.False(hasMore);
        Assert.False(isStream);
        Assert.False(requiresQuickAck);
        Assert.Equal(data.End, position);
    }
    [Fact]
    public void Should_DecodeObfuscated_WithFourLengthBytes()
    {
        using var mock = AutoMock.GetLoose();
        var proto = mock.Mock<IMTProtoService>();
        proto.Setup(x => x.GetAuthKey(It.IsAny<long>()))
            .Returns(RandomNumberGenerator.GetBytes(192));
        proto.Setup(x => x.PutAuthKeyAsync(It.IsAny<long>(), 
            It.IsAny<byte[]>())).ReturnsAsync(true);

        var frame = new byte[2052];
        Random.Shared.NextBytes(frame);
        int len = 2048 / 4;
        frame[0] = 0x7f;
        frame[1] = (byte)(len & 0xff);
        frame[2] = (byte)((len >> 8) & 0xff);
        frame[3] = (byte)((len >> 16) & 0xff);
        var key = new byte[32];
        Random.Shared.NextBytes(key);
        var iv = new byte[16];
        Random.Shared.NextBytes(iv);
        Aes256Ctr aes256Ctr = new (key.ToArray(), iv.ToArray());
        var encodedFrame = new byte[2052];
        aes256Ctr.Transform(new ReadOnlySequence<byte>(frame), encodedFrame);
        var decoder = new AbridgedFrameDecoder(new Aes256Ctr(key.ToArray(), iv.ToArray()), proto.Object);
        var data = new ReadOnlySequence<byte>(encodedFrame);
        var hasMore = decoder.Decode(data,
            out var decodedFrame,
            out var isStream, 
            out bool requiresQuickAck, 
            out var position);
        Assert.Equal(frame[4..], 
            decodedFrame.ToArray());
        Assert.False(hasMore);
        Assert.False(isStream);
        Assert.False(requiresQuickAck);
        Assert.Equal(data.End, position);
    }
    [Fact]
    public void Should_DecodeObfuscated_Stream()
    {
        var authKey = new byte[192];
        Random.Shared.NextBytes(authKey);
        using var mock = AutoMock.GetLoose();
        var proto = mock.Mock<IMTProtoService>();
        proto.Setup(x => x.GetAuthKey(It.IsAny<long>()))
            .Returns(authKey);
        
        var frame = new byte[2000];
        Random.Shared.NextBytes(frame);
        BinaryPrimitives.WriteInt32LittleEndian(frame.AsSpan()[60..], Constructors.layer146_SaveFilePart);
        var messageKey = AesIge.GenerateMessageKey(authKey, frame.AsSpan()[28..], true);
        messageKey.CopyTo(frame.AsSpan()[12..]);
        var aes = new AesIge(authKey, messageKey);
        aes.Encrypt(frame.AsSpan()[28..]);
        int len = 1996 / 4;
        frame[0] = 0x7f;
        frame[1] = (byte)(len & 0xff);
        frame[2] = (byte)((len >> 8) & 0xff);
        frame[3] = (byte)((len >> 16) & 0xff);
        var key = new byte[32];
        Random.Shared.NextBytes(key);
        var iv = new byte[16];
        Random.Shared.NextBytes(iv);
        Aes256Ctr aes256Ctr = new (key.ToArray(), iv.ToArray());
        var encodedFrame = new byte[2000];
        aes256Ctr.Transform(new ReadOnlySequence<byte>(frame), encodedFrame);
        var decoder = new AbridgedFrameDecoder(new Aes256Ctr(key.ToArray(), iv.ToArray()), proto.Object);
        var data = new ReadOnlySequence<byte>(encodedFrame);
        var hasMore = decoder.Decode(data,
            out var decodedFrame,
            out var isStream, 
            out bool requiresQuickAck, 
            out var position);
        Assert.Equal(frame[4..1028], 
            decodedFrame.ToArray());
        Assert.True(hasMore);
        Assert.True(isStream);
        Assert.False(requiresQuickAck);
        hasMore = decoder.Decode(data.Slice(position),
            out decodedFrame,
            out isStream, 
            out requiresQuickAck, 
            out position);
        Assert.Equal(frame[1028..], 
            decodedFrame.ToArray());
        Assert.False(hasMore);
        Assert.True(isStream);
        Assert.False(requiresQuickAck);
        Assert.Equal(data.End, position);
    }
    [Fact]
    public void Should_DecodeEmptyFrame_WithMissingLengthBytes()
    {
        using var mock = AutoMock.GetLoose();
        var proto = mock.Mock<IMTProtoService>();
        proto.Setup(x => x.GetAuthKey(It.IsAny<long>()))
            .Returns(RandomNumberGenerator.GetBytes(192));
        proto.Setup(x => x.PutAuthKeyAsync(It.IsAny<long>(), 
            It.IsAny<byte[]>())).ReturnsAsync(true);

        var frame = new byte[2];
        int len = 2048 / 4;
        frame[0] = 0x7f;
        frame[1] = (byte)(len & 0xff);
        
        var decoder = new AbridgedFrameDecoder(proto.Object);
        var hasMore = decoder.Decode(new ReadOnlySequence<byte>(frame),
            out var decodedFrame,
            out var isStream, 
            out bool requiresQuickAck, 
            out var position);
        Assert.Equal(Array.Empty<byte>(), 
            decodedFrame.ToArray());
        Assert.False(hasMore);
        Assert.False(isStream);
        Assert.False(requiresQuickAck);
    }
}