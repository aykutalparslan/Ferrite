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

using Autofac.Extras.Moq;
using Ferrite.Data.Repositories;
using Ferrite.TL.slim.baseLayer;
using Ferrite.TL.slim.baseLayer.dto;
using Moq;
using Xunit;

namespace Ferrite.Tests.Data;

public class MessageRepositoryTests
{
    [Fact]
    public void Puts_OutgoingMessage()
    {
        using var from = PeerUser.Builder()
            .UserId(987)
            .Build();
        using var to = PeerUser.Builder()
            .UserId(765)
            .Build();
        using TLMessage message = Message.Builder()
            .Id(111)
            .FromId(from.ToReadOnlySpan())
            .PeerId(to.ToReadOnlySpan())
            .Date(555)
            .MessageProperty("test123"u8)
            .OutProperty(true)
            .Build();
        using TLSavedMessage savedMessage = SavedMessage.Builder()
            .OriginalMessage(message.AsSpan())
            .Pts(555)
            .Build();
        var savedBytes = savedMessage.AsSpan().ToArray();
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Put(savedBytes, 
            (long)987, (int)TLPeer.PeerType.PeerUser, 
            (long)765, true, 111, 555, It.IsAny<long>())).Verifiable();

        var repo = mock.Create<MessageRepository>();
        repo.PutMessage(987, message, 555);
        store.VerifyAll();
    }
    [Fact]
    public void Puts_IncomingMessage()
    {
        using var from = PeerUser.Builder()
            .UserId(987)
            .Build();
        using var to = PeerUser.Builder()
            .UserId(765)
            .Build();
        using TLMessage message = Message.Builder()
            .Id(111)
            .FromId(from.ToReadOnlySpan())
            .PeerId(to.ToReadOnlySpan())
            .Date(555)
            .MessageProperty("test123"u8)
            .OutProperty(false)
            .Build();
        using TLSavedMessage savedMessage = SavedMessage.Builder()
            .OriginalMessage(message.AsSpan())
            .Pts(555)
            .Build();
        var savedBytes = savedMessage.AsSpan().ToArray();
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Put(savedBytes, 
            (long)765, (int)TLPeer.PeerType.PeerUser, 
            (long)987, false, 111, 555, It.IsAny<long>())).Verifiable();

        var repo = mock.Create<MessageRepository>();
        repo.PutMessage(765, message, 555);
        store.VerifyAll();
    }
    
    [Fact]
    public void Deletes_Message()
    {
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.DeleteBySecondaryIndex("by_id",(long)111, 222)).Verifiable();

        var repo = mock.Create<MessageRepository>();
        repo.DeleteMessage(111, 222);
        store.VerifyAll();
    }
    
    [Fact]
    public async Task Deletes_MessageAsync()
    {
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.DeleteBySecondaryIndexAsync("by_id",(long)111, 222)).Verifiable();

        var repo = mock.Create<MessageRepository>();
        await repo.DeleteMessageAsync(111, 222);
        store.VerifyAll();
    }

    [Fact]
    public void Gets_Messages_ForUser()
    {
        List<byte[]> savedMessages = new();
        for (int i = 0; i < 5; i++)
        {
            using var from = PeerUser.Builder()
                .UserId(987)
                .Build();
            using var to = PeerUser.Builder()
                .UserId(765)
                .Build();
            using TLMessage message = Message.Builder()
                .Id(111-i)
                .FromId(from.ToReadOnlySpan())
                .PeerId(to.ToReadOnlySpan())
                .Date(555+i)
                .MessageProperty("test123"u8)
                .OutProperty(true)
                .Build();
            using TLSavedMessage savedMessage = SavedMessage.Builder()
                .OriginalMessage(message.AsSpan())
                .Pts(555)
                .Build();
            var savedBytes = savedMessage.AsSpan().ToArray();
            savedMessages.Add(savedBytes);
        }
        
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Iterate((long)987)).Returns(savedMessages);

        var repo = mock.Create<MessageRepository>();
        var results = repo.GetMessages(987).ToArray();
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(savedMessages[i], 
                results[i].AsSpan().ToArray());
        }
        store.VerifyAll();
    }

    [Fact]
    public void Gets_Messages_ForUserAndPeer()
    {
        List<byte[]> savedMessages = new();
        using var from = PeerUser.Builder()
            .UserId(987)
            .Build();
        using var to = PeerUser.Builder()
            .UserId(765)
            .Build();
        using TLMessage message = Message.Builder()
            .Id(111)
            .FromId(from.ToReadOnlySpan())
            .PeerId(to.ToReadOnlySpan())
            .Date(555)
            .MessageProperty("test123"u8)
            .OutProperty(true)
            .Build();
        using TLSavedMessage savedMessage = SavedMessage.Builder()
            .OriginalMessage(message.AsSpan())
            .Pts(555)
            .Build();
        var savedBytes = savedMessage.AsSpan().ToArray();
        savedMessages.Add(savedBytes);
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Iterate((long)987, 
            (int)TLInputPeer.InputPeerType.InputPeerUser,
            (long)765)).Returns(savedMessages);

        var repo = mock.Create<MessageRepository>();
        using TLInputPeer peer = InputPeerUser.Builder()
            .UserId(765)
            .Build();
        var results = repo.GetMessages(987, peer).ToArray();
        Assert.Single(results);
        Assert.Equal(savedMessages[0],
            results[0].AsSpan().ToArray());
        store.VerifyAll();
    }
    
    [Fact]
    public async Task Gets_MessagesAsync_ForUser()
    {
        List<byte[]> savedMessages = new();
        for (int i = 0; i < 5; i++)
        {
            using TLPeer from = PeerUser.Builder()
                .UserId(987)
                .Build();
            using TLPeer to = PeerUser.Builder()
                .UserId(765)
                .Build();
            using TLMessage message = Message.Builder()
                .Id(111-i)
                .FromId(from.AsSpan())
                .PeerId(to.AsSpan())
                .Date(555+i)
                .MessageProperty("test123"u8)
                .OutProperty(true)
                .Build();
            using TLSavedMessage savedMessage = SavedMessage.Builder()
                .OriginalMessage(message.AsSpan())
                .Pts(555)
                .Build();
            var savedBytes = savedMessage.AsSpan().ToArray();
            savedMessages.Add(savedBytes);
        }
        
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.IterateAsync((long)987))
            .Returns(savedMessages.ToAsyncEnumerable());

        var repo = mock.Create<MessageRepository>();
        var results = (await repo.GetMessagesAsync(987)).ToArray();
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(savedMessages[i], 
                results[i].AsSpan().ToArray());
        }
        store.VerifyAll();
    }

    [Fact]
    public async Task Gets_MessagesAsync_ForUserAndPeer()
    {
        List<byte[]> savedMessages = new();
        using TLPeer from = PeerUser.Builder()
            .UserId(987)
            .Build();
        using TLPeer to = PeerUser.Builder()
            .UserId(765)
            .Build();
        using TLMessage message = Message.Builder()
            .Id(111)
            .FromId(from.AsSpan())
            .PeerId(to.AsSpan())
            .Date(555)
            .MessageProperty("test123"u8)
            .OutProperty(true)
            .Build();
        using TLSavedMessage savedMessage = SavedMessage.Builder()
            .OriginalMessage(message.AsSpan())
            .Pts(555)
            .Build();
        var savedBytes = savedMessage.AsSpan().ToArray();
        savedMessages.Add(savedBytes);
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.IterateAsync((long)987, 
            (int)TLInputPeer.InputPeerType.InputPeerUser,
            (long)765)).Returns(savedMessages.ToAsyncEnumerable());

        var repo = mock.Create<MessageRepository>();
        using TLInputPeer peer = InputPeerUser.Builder()
            .UserId(765)
            .Build();
        var results = (await repo.GetMessagesAsync(987, peer)).ToArray();
        Assert.Single(results);
        Assert.Equal(savedMessages[0],
            results[0].AsSpan().ToArray());
        store.VerifyAll();
    }
    
    [Fact]
    public void Gets_Messages_WithPtsAndDate()
    {
        List<byte[]> savedMessages = new();
        DateTimeOffset date = DateTimeOffset.Now;
        for (int i = 0; i < 10; i++)
        {
            using var from = PeerUser.Builder()
                .UserId(987)
                .Build();
            using var to = PeerUser.Builder()
                .UserId(765)
                .Build();
            using TLMessage message = Message.Builder()
                .Id(111-i)
                .FromId(from.ToReadOnlySpan())
                .PeerId(to.ToReadOnlySpan())
                .Date((int)date.ToUnixTimeSeconds())
                .MessageProperty("test123"u8)
                .OutProperty(true)
                .Build();
            using TLSavedMessage savedMessage = SavedMessage.Builder()
                .OriginalMessage(message.AsSpan())
                .Pts(555+i)
                .Build();
            var savedBytes = savedMessage.AsSpan().ToArray();
            savedMessages.Add(savedBytes);
        }
        
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Iterate((long)987)).Returns(savedMessages);

        var repo = mock.Create<MessageRepository>();
        var results = repo.GetMessages(987, 558, 561, date).ToArray();
        Assert.Equal(4, results.Length);
        for (int i = 0; i < 4; i++)
        {
            Assert.Equal(savedMessages[i+3], 
                results[i].AsSpan().ToArray());
        }
        store.VerifyAll();
    }
    
    [Fact]
    public async Task  Gets_MessagesAsync_WithPtsAndDate()
    {
        List<byte[]> savedMessages = new();
        DateTimeOffset date = DateTimeOffset.Now;
        for (int i = 0; i < 10; i++)
        {
            using TLPeer from = PeerUser.Builder()
                .UserId(987)
                .Build();
            using TLPeer to = PeerUser.Builder()
                .UserId(765)
                .Build();
            using TLMessage message = Message.Builder()
                .Id(111-i)
                .FromId(from.AsSpan())
                .PeerId(to.AsSpan())
                .Date((int)date.ToUnixTimeSeconds())
                .MessageProperty("test123"u8)
                .OutProperty(true)
                .Build();
            using TLSavedMessage savedMessage = SavedMessage.Builder()
                .OriginalMessage(message.AsSpan())
                .Pts(555+i)
                .Build();
            var savedBytes = savedMessage.AsSpan().ToArray();
            savedMessages.Add(savedBytes);
        }
        
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.IterateAsync((long)987)).Returns(savedMessages.ToAsyncEnumerable);

        var repo = mock.Create<MessageRepository>();
        var results = (await repo.GetMessagesAsync(987, 558, 561, date)).ToArray();
        Assert.Equal(4, results.Length);
        for (int i = 0; i < 4; i++)
        {
            Assert.Equal(savedMessages[i+3], 
                results[i].AsSpan().ToArray());
        }
        store.VerifyAll();
    }

    [Fact]
    public void Gets_Message()
    {
        using TLPeer from = PeerUser.Builder()
            .UserId(987)
            .Build();
        using TLPeer to = PeerUser.Builder()
            .UserId(765)
            .Build();
        using TLMessage message = Message.Builder()
            .Id(111)
            .FromId(from.AsSpan())
            .PeerId(to.AsSpan())
            .Date(555)
            .MessageProperty("test123"u8)
            .OutProperty(true)
            .Build();
        using TLSavedMessage savedMessage = SavedMessage.Builder()
            .OriginalMessage(message.AsSpan())
            .Pts(555)
            .Build();
        var savedBytes = savedMessage.AsSpan().ToArray();
        
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.GetBySecondaryIndex("by_id",(long)987, 111)).Returns(savedBytes);

        var repo = mock.Create<MessageRepository>();
        var result = repo.GetMessage(987, 111);
        Assert.Equal(savedBytes, result.Value.AsSpan().ToArray());
    }
    
    [Fact]
    public async Task Gets_MessageAsync()
    {
        using TLPeer from = PeerUser.Builder()
            .UserId(987)
            .Build();
        using TLPeer to = PeerUser.Builder()
            .UserId(765)
            .Build();
        using TLMessage message = Message.Builder()
            .Id(111)
            .FromId(from.AsSpan())
            .PeerId(to.AsSpan())
            .Date(555)
            .MessageProperty("test123"u8)
            .OutProperty(true)
            .Build();
        using TLSavedMessage savedMessage = SavedMessage.Builder()
            .OriginalMessage(message.AsSpan())
            .Pts(555)
            .Build();
        var savedBytes = savedMessage.AsSpan().ToArray();
        
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.GetBySecondaryIndexAsync("by_id",(long)987, 111)).Returns(new ValueTask<byte[]?>(savedBytes));

        var repo = mock.Create<MessageRepository>();
        var result = await repo.GetMessageAsync(987, 111);
        Assert.Equal(savedBytes, result.Value.AsSpan().ToArray());
    }
}