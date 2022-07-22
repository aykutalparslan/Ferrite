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

using StackExchange.Redis;

namespace Ferrite.Data;

public class RedisMessageBox : IMessageBox
{
    private readonly ConnectionMultiplexer _redis;
    private readonly IAtomicCounter _counter;
    private readonly long _userId;
    private readonly SortedSet<RedisKey> _dialogs = new SortedSet<RedisKey>();
    public RedisMessageBox(ConnectionMultiplexer redis, long userId)
    {
        _redis = redis;
        _userId = userId;
        _counter = new RedisCounter(redis, $"seq:pts:{userId}");
    }

    public async Task<int> Pts()
    {
        return (int)await _counter.Get();
    }

    public async Task<int> IncrementPtsForMessage(PeerDTO peer, int messageId)
    {
        IDatabase db = _redis.GetDatabase();
        RedisKey key = $"msg:unread:{_userId}-{(int)peer.PeerType}-{peer.PeerId}";
        if (!_dialogs.Contains(key))
        {
            _dialogs.Add(key);
        }
        db.SortedSetAdd(key, messageId, messageId);
        return (int)await _counter.IncrementAndGet();
    }

    public async Task<int> ReadMessages(PeerDTO peer, int maxId)
    {
        IDatabase db = _redis.GetDatabase();
        RedisKey key = $"msg:unread:{_userId}-{(int)peer.PeerType}-{peer.PeerId}";
        await db.SortedSetRemoveRangeByScoreAsync(key, 0, maxId);
        int unread = 0;
        foreach (var k in _dialogs)
        {
            unread += (int)await db.SortedSetLengthAsync(k);
        }

        return unread;
    }

    public async Task<int> IncrementPts()
    {
        return (int)await _counter.IncrementAndGet();
    }
}