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

using System.Security.Cryptography;
using Autofac.Extras.Moq;
using Ferrite.Data;
using Ferrite.Data.Repositories;
using Ferrite.TL.slim.dto;
using Xunit;

namespace Ferrite.Tests.Data;

public class BlockedPeersRepositoryTests
{
    [Fact]
    public void Puts_BlockedPeer()
    {
        var peerType = PeerType.User;
        var date = DateTimeOffset.Now;
        var blockedBytes = BlockedPeer.Builder()
            .PeerType((int)peerType)
            .PeerId(222)
            .Date((int)date.ToUnixTimeSeconds())
            .Build().TLBytes!.Value.AsSpan().ToArray();
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Put(blockedBytes, 
            (long)111, (long)222, (int)peerType)).Verifiable();

        var repo = mock.Create<BlockedPeersRepository>();
        repo.PutBlockedPeer(111, 222, peerType, date);
        store.VerifyAll();
    }
    
    [Fact]
    public void Deletes_BlockedPeer()
    {
        var peerType = PeerType.User;
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Delete((long)111, (long)222, (int)peerType)).Verifiable();

        var repo = mock.Create<BlockedPeersRepository>();
        repo.DeleteBlockedPeer(111, 222, peerType);
        store.VerifyAll();
    }
    
    [Fact]
    public void Gets_BlockedPeers()
    {
        List<byte[]> peerData = new();
        for (int i = 0; i < 2; i++)
        {
            peerData.Add(RandomNumberGenerator.GetBytes(32));
        }
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Iterate((long)111)).Returns(peerData);

        var repo = mock.Create<BlockedPeersRepository>();
        var peers = repo.GetBlockedPeers(111);
        for (int i = 0; i < 2; i++)
        {
            Assert.Equal(peers[i].AsSpan().ToArray(), peerData[i]);
        }
        store.VerifyAll();
    }
    
    [Fact]
    public void PutsAndGets_BlockedPeers()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        var repo = new BlockedPeersRepository(new RocksDBKVStore(ctx));
        List<byte[]> peerData = new();
        for (int i = 0; i < 3; i++)
        {
            var date = DateTimeOffset.Now;
            repo.PutBlockedPeer(111, 222 + i, PeerType.User, date);
            var blockedBytes = BlockedPeer.Builder()
                .PeerType((int)PeerType.User)
                .PeerId(222 + i)
                .Date((int)date.ToUnixTimeSeconds())
                .Build().TLBytes!.Value.AsSpan().ToArray();
            peerData.Add(blockedBytes);
        }

        var blocked = repo.GetBlockedPeers(111);
        for (int i = 0; i < 3; i++)
        {
            Assert.Equal(blocked[i].AsSpan().ToArray(), peerData[i]);
        }
        Util.DeleteDirectory(path);
    }
    
    [Fact]
    public void PutsAndDeletes_BlockedPeer()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        var repo = new BlockedPeersRepository(new RocksDBKVStore(ctx));
        List<byte[]> peerData = new();
        for (int i = 0; i < 3; i++)
        {
            var date = DateTimeOffset.Now;
            repo.PutBlockedPeer(111, 222 + i, PeerType.User, date);
            var blockedBytes = BlockedPeer.Builder()
                .PeerType((int)PeerType.User)
                .PeerId(222 + i)
                .Date((int)date.ToUnixTimeSeconds())
                .Build().TLBytes!.Value.AsSpan().ToArray();
            peerData.Add(blockedBytes);
        }

        repo.DeleteBlockedPeer(111, 222, PeerType.User);
        var blocked = repo.GetBlockedPeers(111);
        Assert.Equal(2, blocked.Count);
        Util.DeleteDirectory(path);
    }
}