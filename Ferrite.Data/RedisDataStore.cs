using System;
using System.Buffers.Binary;
using StackExchange.Redis;

namespace Ferrite.Data;

public class RedisDataStore: IDistributedStore
{
    private readonly ConnectionMultiplexer redis;

    public RedisDataStore(string config)
    {
        redis = ConnectionMultiplexer.Connect(config);
    }

    public byte[] GetAuthKey(byte[] authKeyId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        return db.StringGet((RedisKey)authKeyId);
    }

    public byte[] GetSession(byte[] sessionId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        return db.StringGet((RedisKey)sessionId);
    }

    public Task<bool> PutAuthKeyAsync(byte[] authKeyId, byte[] authKey)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        return db.StringSetAsync((RedisKey)authKeyId, (RedisValue)authKey);
    }

    public Task<bool> PutSessionAsync(byte[] sessionId, byte[] sessionData)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        return db.StringSetAsync((RedisKey)sessionId, (RedisValue)sessionData);
    }

    public Task<bool> RemoveSessionAsync(byte[] sessionId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        return db.KeyDeleteAsync((RedisKey)sessionId);
    }
}

