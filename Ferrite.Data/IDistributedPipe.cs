using System;
namespace Ferrite.Data;

public interface IDistributedPipe
{
    public Task<bool> SubscribeAsync(string channel);
    /// <summary>
    /// Releases all of the underlying resources and leaves the pipe in an unusable state.
    /// </summary>
    /// <returns></returns>
    public Task<bool> UnSubscribeAsync();
    public ValueTask<byte[]> ReadMessageAsync(CancellationToken cancellationToken = default);
    public Task<bool> WriteMessageAsync(string channel, byte[] message);
}

