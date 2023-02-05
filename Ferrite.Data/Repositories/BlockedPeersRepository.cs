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

using Ferrite.TL.slim;
using Ferrite.TL.slim.layer150.dto;
using MessagePack;

namespace Ferrite.Data.Repositories;

public class BlockedPeersRepository : IBlockedPeersRepository
{
    private readonly IKVStore _store;
    public BlockedPeersRepository(IKVStore store)
    {
        _store = store;
        store.SetSchema(new TableDefinition("ferrite", "blocked_peers",
            new KeyDefinition("pk",
                new DataColumn { Name = "user_id", Type = DataType.Long },
                new DataColumn { Name = "peer_id", Type = DataType.Long },
                new DataColumn { Name = "peer_type", Type = DataType.Int })));
    }
    public bool PutBlockedPeer(long userId, long peerId, PeerType peerType, DateTimeOffset date)
    {
        var blockedBytes = BlockedPeer.Builder()
            .PeerType((int)peerType)
            .PeerId(peerId)
            .Date((int)date.ToUnixTimeSeconds())
            .Build().TLBytes!.Value;
        return _store.Put(blockedBytes.AsSpan().ToArray(), userId, peerId, (int)peerType);
    }

    public bool DeleteBlockedPeer(long userId, long peerId, PeerType peerType)
    {
        return _store.Delete(userId, peerId, (int)peerType);
    }

    public IReadOnlyList<TLBlockedPeer> GetBlockedPeers(long userId)
    {
        List<TLBlockedPeer> blockedPeers = new();
        var iter = _store.Iterate(userId);
        foreach (var peerBlockedBytes in iter)
        {
            blockedPeers.Add(new TLBlockedPeer(peerBlockedBytes, 0 , peerBlockedBytes.Length));
        }

        return blockedPeers;
    }
}