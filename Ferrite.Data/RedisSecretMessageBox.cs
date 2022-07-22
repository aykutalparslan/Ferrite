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

public class RedisSecretMessageBox : ISecretMessageBox
{
    private readonly ConnectionMultiplexer _redis;
    private readonly IAtomicCounter _counter;
    private readonly long _authKeyId;
    public RedisSecretMessageBox(ConnectionMultiplexer redis, long authKeyId)
    {
        _redis = redis;
        _authKeyId = authKeyId;
        _counter = new RedisCounter(redis, $"seq:qts:{authKeyId}");
    }
    public async Task<int> Qts()
    {
        return (int)await _counter.Get();
    }

    public async Task<int> IncrementQts()
    {
        return (int)await _counter.IncrementAndGet();
    }
}