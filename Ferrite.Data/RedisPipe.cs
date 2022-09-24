using System;
using System.Threading.Tasks.Sources;
using StackExchange.Redis;

namespace Ferrite.Data;

public class RedisPipe: IMessagePipe
{
    private readonly ConnectionMultiplexer redis;
    private ChannelMessageQueue messageQueue;
    public RedisPipe(string config)
    {
        redis = ConnectionMultiplexer.Connect(config);
    }

    public async ValueTask<byte[]> ReadMessageAsync(CancellationToken cancellationToken=default)
    {
        if(messageQueue == null)
        {
            throw new InvalidOperationException("Subscribe must be called first.");
        }
        var message = await messageQueue.ReadAsync(cancellationToken);
        return (byte[])message.Message;
    }

    public async ValueTask<bool> SubscribeAsync(string channel)
    {
        Interlocked.CompareExchange<ChannelMessageQueue>(ref messageQueue,
            await redis.GetSubscriber().SubscribeAsync(channel),
            null);
        return true;
    }

    public async ValueTask<bool> UnSubscribeAsync()
    {
        if (messageQueue == null)
        {
            throw new InvalidOperationException("Not subscribed.");
        }
        await messageQueue.UnsubscribeAsync();
        return true;
    }

    public async ValueTask<bool> WriteMessageAsync(string channel, byte[] message)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        _ = await db.PublishAsync((RedisChannel)channel, (RedisValue)message);
        return true;
    }
}

