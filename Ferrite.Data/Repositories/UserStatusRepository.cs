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

using MessagePack;

namespace Ferrite.Data.Repositories;

public class UserStatusRepository : IUserStatusRepository
{
    private readonly IKVStore _store;
    public UserStatusRepository(IKVStore store)
    {
        _store = store;
        _store.SetSchema(new TableDefinition("ferrite", "user_statuses",
            new KeyDefinition("pk",
                new DataColumn { Name = "user_id", Type = DataType.Long })));
    }
    public bool PutUserStatus(long userId, bool status)
    {
        UserStatusDTO userStatus = new UserStatusDTO()
        {
            Status = UserStatusType.Online,
            WasOnline = (int)DateTimeOffset.Now.ToUnixTimeSeconds(),
            Expires = 10,
        };
        var serialized = MessagePackSerializer.Serialize(userStatus);
        return _store.Put(serialized, userId);
    }

    public UserStatusDTO GetUserStatusAsync(long userId)
    {
        
        var serialized = MessagePackSerializer.Serialize(_store.Get(userId));
        if (serialized == null)
        {
            return UserStatusDTO.Empty;
        }
        var userStatus = MessagePackSerializer.Deserialize<UserStatusDTO>(serialized);
        if (userStatus.WasOnline + userStatus.Expires < DateTimeOffset.Now.ToUnixTimeSeconds())
        {
            return userStatus;
        }
        else if(userStatus.WasOnline < DateTimeOffset.Now.AddDays(-1).ToUnixTimeSeconds())
        {
            return UserStatusDTO.Recently;
        }
        else if(userStatus.WasOnline < DateTimeOffset.Now.AddDays(-7).ToUnixTimeSeconds())
        {
            return UserStatusDTO.LastWeek;
        }
        else if(userStatus.WasOnline < DateTimeOffset.Now.AddDays(-30).ToUnixTimeSeconds())
        {
            return UserStatusDTO.LastMonth;
        }
        else
        {
            return userStatus with { Status = UserStatusType.Offline };
        }
    }
}