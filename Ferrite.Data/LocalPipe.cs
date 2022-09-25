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

using System.Threading.Channels;
using NonBlocking;

namespace Ferrite.Data;

public class LocalPipe : IMessagePipe
{
    private static readonly ConcurrentDictionary<string, Channel<byte[]>> _channels;
    static LocalPipe()
    {
        _channels = new ConcurrentDictionary<string, Channel<byte[]>>();
    }

    private string _channel;
    
    public ValueTask<bool> SubscribeAsync(string channel)
    {
        if (!_channels.ContainsKey(channel))
        {
            _channels.TryAdd(channel, Channel.CreateUnbounded<byte[]>());
        }

        _channel = channel;
        return ValueTask.FromResult<bool>(true);
    }

    public ValueTask<bool> UnSubscribeAsync()
    {
        if (_channels.ContainsKey(_channel))
        {
            _channels.TryRemove(_channel, out var c);
        }
        return ValueTask.FromResult<bool>(true);
    }

    public async ValueTask<byte[]> ReadMessageAsync(CancellationToken cancellationToken = default)
    {
        if (!_channels.TryGetValue(_channel, out var c))
        {
            return await c.Reader.ReadAsync(cancellationToken);
        }

        return null;
    }

    public async ValueTask<bool> WriteMessageAsync(string channel, byte[] message)
    {
        if (!_channels.ContainsKey(channel))
        {
            _channels.TryAdd(channel, Channel.CreateUnbounded<byte[]>());
        }

        if (!_channels.TryGetValue(channel, out var c)) return false;
        await c.Writer.WriteAsync(message);
        return true;
    }
}