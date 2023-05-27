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
    private readonly IAtomicCounter _ptsCounter;
    private readonly IAtomicCounter _messageIdCounter;
    private readonly long _userId;
    public RedisMessageBox(ConnectionMultiplexer redis, long userId)
    {
        _redis = redis;
        _userId = userId;
        _ptsCounter = new RedisCounter(redis, $"seq:pts:{userId}");
        _messageIdCounter = new RedisCounter(redis, $"seq:message:id:{userId}");
    }

    public async ValueTask<int> Pts()
    {
        return (int)await _ptsCounter.Get();
    }

    public async ValueTask<int> IncrementPtsForMessage(int peerType, long peerId, int messageId)
    {
        IDatabase db = _redis.GetDatabase();
        RedisKey key = $"msg:unread:{_userId}-{peerType}-{peerId}";
        RedisKey dialogsKey = $"msg:dialogs:{_userId}";
        db.SortedSetAdd(dialogsKey, $"msg:unread:{_userId}-{peerType}-{peerId}", 0);
        db.SortedSetAdd(key, messageId, messageId);
        return (int)await _ptsCounter.IncrementAndGet();
    }

    public async ValueTask<int> NextMessageId()
    {
        int messageId = (int)await _messageIdCounter.IncrementAndGet();
        if (messageId == 0)
        {
            messageId = (int)await _messageIdCounter.IncrementAndGet();
        }
        return messageId;
    }

    public async ValueTask<int> ReadMessages(int peerType, long peerId, int maxId)
    {
        IDatabase db = _redis.GetDatabase();
        RedisKey key = $"msg:unread:{_userId}-{peerType}-{peerId}";
        await db.SortedSetRemoveRangeByScoreAsync(key, 0, maxId);
        GetUnread(db, out var unread);
        RedisKey keyRead = $"msg:max-read:{_userId}-{peerType}-{peerId}";
        //TODO: Use a LUA script for this
        bool success = false;
        while (!success)
        {
            var oldValue = (int)await db.StringGetAsync(keyRead);
            if (oldValue > maxId)
            {
                break;
            }
            var tran = db.CreateTransaction();
            tran.AddCondition(Condition.StringEqual(keyRead, oldValue));
            await tran.StringSetAsync(keyRead, maxId);
            success = await tran.ExecuteAsync();
        }
        
        return unread;
    }

    private void GetUnread(IDatabase db, out int unread)
    {
        unread = 0;
        RedisKey dialogsKey = $"msg:dialogs:{_userId}";
        var dialogs = db.SortedSetScan(dialogsKey);
        foreach (var e in dialogs)
        {
            unread += (int) db.SortedSetLength(new RedisKey((string)e.Element));
        }
    }

    public async ValueTask<int> ReadMessagesMaxId(int peerType, long peerId)
    {
        IDatabase db = _redis.GetDatabase();
        RedisKey keyRead = $"msg:max-read:{_userId}-{peerType}-{peerId}";
        return (int)await db.StringGetAsync(keyRead);
    }

    public async ValueTask<int> UnreadMessages()
    {
        IDatabase db = _redis.GetDatabase();
        GetUnread(db, out var unread);

        return unread;
    }

    public async ValueTask<int> UnreadMessages(int peerType, long peerId)
    {
        IDatabase db = _redis.GetDatabase();
        RedisKey key = $"msg:unread:{_userId}-{peerType}-{peerId}";
        return (int)await db.SortedSetLengthAsync(key);
    }

    public async ValueTask<int> IncrementPts()
    {
        int pts = (int)await _ptsCounter.IncrementAndGet();
        if (pts == 0)
        {
            pts = (int)await _ptsCounter.IncrementAndGet();
        }
        return pts;
    }
}