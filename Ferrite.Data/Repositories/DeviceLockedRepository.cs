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

namespace Ferrite.Data.Repositories;

public class DeviceLockedRepository : IDeviceLockedRepository
{
    private readonly IVolatileKVStore _store;
    public DeviceLockedRepository(IVolatileKVStore store)
    {
        _store = store;
        store.SetSchema(new TableDefinition("ferrite", "locked_devices",
            new KeyDefinition("pk",
                new DataColumn { Name = "auth_key_id", Type = DataType.Long })));
    }
    public bool PutDeviceLocked(long authKeyId, TimeSpan period)
    {
        var lockedUntil = DateTimeOffset.Now.Add(period).ToUnixTimeMilliseconds();
        _store.Put(BitConverter.GetBytes(lockedUntil), period, authKeyId);
        return true;
    }

    public TimeSpan? GetDeviceLocked(long authKeyId)
    {
        var status = _store.Get(authKeyId);
        if (status != null)
        {
            long lockedUntil = BitConverter.ToInt64(status);
            var now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            return TimeSpan.FromMilliseconds(lockedUntil - now);
        }

        return null;
    }
}