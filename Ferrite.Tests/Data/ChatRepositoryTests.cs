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
using Ferrite.TL.slim.dto;
using Ferrite.TL.slim.layer150;
using Moq;
using Xunit;

namespace Ferrite.Tests.Data;

public class ChatRepositoryTests
{
    [Fact]
    public void Puts_Chat()
    {
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Put(It.IsAny<byte[]>(), 
            It.IsAny<long>())).Verifiable();

        var repo = mock.Create<ChatRepository>();
        using var chat = Chat.Builder()
            .Id(123)
            .Title("test"u8)
            .Photo(ChatPhotoEmpty.Builder().Build().ToReadOnlySpan())
            .ParticipantsCount(4)
            .Date((int)DateTimeOffset.Now.ToUnixTimeSeconds())
            .Version(5)
            .Build().TLBytes!.Value;
        repo.PutChat(chat);
        store.VerifyAll();
    }
    
    [Fact]
    public async Task PutsAndGets_Chat()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        ChatRepository repo = new (new RocksDBKVStore(ctx));
        using var chat = Chat.Builder()
            .Id(123)
            .Title("test"u8)
            .Photo(ChatPhotoEmpty.Builder().Build().ToReadOnlySpan())
            .ParticipantsCount(4)
            .Date((int)DateTimeOffset.Now.ToUnixTimeSeconds())
            .Version(5)
            .Build().TLBytes!.Value;
        repo.PutChat(chat);
        var chatFromRepo = await repo.GetChatAsync(123);
        Assert.Equal(chat.AsSpan().ToArray(), chatFromRepo!.Value.AsSpan().ToArray());
        Util.DeleteDirectory(path);
    }
}