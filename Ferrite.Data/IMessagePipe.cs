using System;
namespace Ferrite.Data;

public interface IMessagePipe
{
    public ValueTask<bool> SubscribeAsync(string channel);
    /// <summary>
    /// Releases all of the underlying resources and leaves the pipe in an unusable state.
    /// </summary>
    /// <returns></returns>
    public ValueTask<bool> UnSubscribeAsync();
    public ValueTask<byte[]> ReadMessageAsync(CancellationToken cancellationToken = default);
    public ValueTask<bool> WriteMessageAsync(string channel, byte[] message);
}

