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

using System.Text;
using System.Text.Unicode;
using Ferrite.TL.slim.dto;
using MessagePack;

namespace Ferrite.Data.Repositories;

public class DeviceInfoRepository : IDeviceInfoRepository
{
    private readonly IKVStore _store;
    private readonly IKVStore _storeUsers;
    public DeviceInfoRepository(IKVStore store, IKVStore storeUsers)
    {
        _store = store;
        _store.SetSchema(new TableDefinition("ferrite", "devices",
            new KeyDefinition("pk",
                new DataColumn { Name = "auth_key_id", Type = DataType.Long },
                new DataColumn { Name = "app_token", Type = DataType.String })));
        _storeUsers = storeUsers;
        _storeUsers.SetSchema(new TableDefinition("ferrite", "device_users",
            new KeyDefinition("pk",
                new DataColumn { Name = "auth_key_id", Type = DataType.Long },
                new DataColumn { Name = "user_id", Type = DataType.Long },
                new DataColumn { Name = "app_token", Type = DataType.String })));
    }
    public bool PutDeviceInfo(long authKeyId, TLDeviceInfo deviceInfo)
    {
        var infoBytes = deviceInfo.AsSpan().ToArray();
        var token = Encoding.UTF8.GetString(deviceInfo.AsDeviceInfo().Token);
        _store.Put(infoBytes, authKeyId, token);
        var userIds = deviceInfo.AsDeviceInfo().OtherUids;
        for(int i = 0; i < userIds.Count; i++)
        {
            var userId = userIds[i];
            using TLDeviceUser user = DeviceUser.Builder()
                .UserId(userId)
                .Token(deviceInfo.AsDeviceInfo().Token)
                .Build();
            _storeUsers.Put(user.AsSpan().ToArray(), 
                authKeyId, userId);
        }

        return true;
    }

    public TLDeviceInfo? GetDeviceInfo(long authKeyId)
    {
        var infoBytes = _store.Get(authKeyId);
        if (infoBytes == null) return null;
        return new TLDeviceInfo(infoBytes, 0 ,infoBytes.Length);
    }

    public bool DeleteDeviceInfo(long authKeyId, string token, ICollection<long> otherUserIds)
    {
        _store.Delete(authKeyId, token);
        foreach (var userId in otherUserIds)
        {
            _storeUsers.Delete(authKeyId, userId);
        }

        return true;
    }
}