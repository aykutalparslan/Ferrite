using System;
using System.Threading.Tasks.Sources;
using StackExchange.Redis;

namespace Ferrite.Data;

public class RedisPipe: IDistributedPipe
{
    private readonly ConnectionMultiplexer redis;
    private ChannelMessageQueue messageQueue;
    public RedisPipe(string config)
    {
        redis = ConnectionMultiplexer.Connect(config);
    }

    public async ValueTask<byte[]> ReadAsync(CancellationToken cancellationToken=default)
    {
        if(messageQueue == null)
        {
            throw new InvalidOperationException("Subscribe must be called first.");
        }
        var message = await messageQueue.ReadAsync(cancellationToken);
        return (byte[])message.Message;
    }

    public async Task<bool> SubscribeAsync(string channel)
    {
        Interlocked.CompareExchange<ChannelMessageQueue>(ref messageQueue,
            redis.GetSubscriber().Subscribe(channel),
            null);
        return true;
    }

    public async Task<bool> UnSubscribeAsync()
    {
        if (messageQueue == null)
        {
            throw new InvalidOperationException("Not subscribed.");
        }
        await messageQueue.UnsubscribeAsync();
        return true;
    }

    public async Task<bool> WriteAsync(string channel, byte[] message)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        _ = await db.PublishAsync((RedisChannel)channel, (RedisValue)message);
        return true;
    }
}

