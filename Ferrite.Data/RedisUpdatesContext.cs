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

public class RedisUpdatesContext : IUpdatesContext
{
    private readonly ConnectionMultiplexer _redis;
    private readonly long _authKeyId;
    private readonly long _userId;
    private readonly IAtomicCounter _counter;
    private readonly IMessageBox _commonMessageBox;
    private readonly ISecretMessageBox _secondaryMessageBox;
    public RedisUpdatesContext(ConnectionMultiplexer redis, long authKeyId, long userId)
    {
        _redis = redis;
        _authKeyId = authKeyId;
        _userId = userId;
        _counter = new RedisCounter(redis, $"seq:updates:{userId}");
        _commonMessageBox = new RedisMessageBox(redis, userId);
        _secondaryMessageBox = new RedisSecretMessageBox(redis, authKeyId);
    }
    public async Task<int> Pts()
    {
        return await _commonMessageBox.Pts();
    }

    public async Task<int> IncrementPtsForMessage(PeerDTO peer, int messageId)
    {
        return await _commonMessageBox.IncrementPtsForMessage(peer, messageId);
    }

    public async Task<int> ReadMessages(PeerDTO peer, int maxId)
    {
        return await _commonMessageBox.ReadMessages(peer, maxId);
    }

    public async Task<int> UnreadMessages()
    {
        return await _commonMessageBox.UnreadMessages();
    }

    public async Task<int> IncrementPts()
    {
        return await _commonMessageBox.IncrementPts();
    }

    public async Task<int> Qts()
    {
        return await _secondaryMessageBox.Qts();
    }

    public async Task<int> IncrementQts()
    {
        return await _secondaryMessageBox.IncrementQts();
    }

    public async Task<int> Seq()
    {
        return (int)await _counter.Get();
    }

    public async Task<int> IncrementSeq()
    {
        return (int)await _counter.IncrementAndGet();
    }
}