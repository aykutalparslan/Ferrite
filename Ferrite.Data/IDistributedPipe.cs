using System;
namespace Ferrite.Data;

public interface IDistributedPipe
{
    public void Subscribe(string channel);
    public Task UnSubscribeAsync();
    public Task<byte[]> ReadAsync(CancellationToken cancellationToken = default);
    public Task WriteAsync(string channel, byte[] message);
}

