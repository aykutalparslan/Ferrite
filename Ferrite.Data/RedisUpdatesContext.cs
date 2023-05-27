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
    private readonly long? _authKeyId;
    private readonly long _userId;
    private readonly IAtomicCounter _counter;
    private readonly IMessageBox _commonMessageBox;
    private readonly ISecretMessageBox? _secondaryMessageBox;
    public RedisUpdatesContext(ConnectionMultiplexer redis, long? authKeyId, long userId)
    {
        _redis = redis;
        _authKeyId = authKeyId;
        _userId = userId;
        _counter = new RedisCounter(redis, $"seq:updates:{userId}");
        _commonMessageBox = new RedisMessageBox(redis, userId);
        _secondaryMessageBox = authKeyId != null ? new RedisSecretMessageBox(redis, (long)authKeyId) : null;
    }
    public async ValueTask<int> Pts()
    {
        return await _commonMessageBox.Pts();
    }

    public async ValueTask<int> IncrementPtsForMessage(int peerType, long peerId, int messageId)
    {
        return await _commonMessageBox.IncrementPtsForMessage(peerType,peerId, messageId);
    }

    public async ValueTask<int> NextMessageId()
    {
        return await _commonMessageBox.NextMessageId();
    }

    public async ValueTask<int> ReadMessages(int peerType, long peerId, int maxId)
    {
        return await _commonMessageBox.ReadMessages(peerType, peerId, maxId);
    }

    public async ValueTask<int> ReadMessagesMaxId(int peerType, long peerId)
    {
        return await _commonMessageBox.ReadMessagesMaxId(peerType, peerId);
    }

    public async ValueTask<int> UnreadMessages()
    {
        return await _commonMessageBox.UnreadMessages();
    }

    public async ValueTask<int> UnreadMessages(int peerType, long peerId)
    {
        return await _commonMessageBox.UnreadMessages(peerType, peerId);
    }

    public async ValueTask<int> IncrementPts()
    {
        return await _commonMessageBox.IncrementPts();
    }

    public async ValueTask<int> Qts()
    {
        return _secondaryMessageBox != null ? await _secondaryMessageBox.Qts() : 0;
    }

    public async ValueTask<int> IncrementQts()
    {
        return _secondaryMessageBox != null ? await _secondaryMessageBox.IncrementQts() : 0;
    }

    public async Task<int> Seq()
    {
        return (int)await _counter.Get();
    }

    public async Task<int> IncrementSeq()
    {
        int seq = (int)await _counter.IncrementAndGet();
        if (seq == 0)
        {
            seq = (int)await _counter.IncrementAndGet();
        }
        return seq;
    }
}