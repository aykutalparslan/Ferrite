using System;
namespace Ferrite.Data;

public interface IDistributedPipe
{
    public void Subscribe(string channel);
    /// <summary>
    /// Releases all of the underlying resources and leaves the pipe in an unusable state.
    /// </summary>
    /// <returns></returns>
    public Task UnSubscribeAsync();
    public ValueTask<byte[]> ReadAsync(CancellationToken cancellationToken = default);
    public Task WriteAsync(string channel, byte[] message);
}

