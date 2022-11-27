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

public class UserRepository : IUserRepository
{
    private readonly IKVStore _store;
    private readonly IKVStore _storeTTL;

    public UserRepository(IKVStore store, IKVStore storeTTL)
    {
        _store = store;
        _store.SetSchema(new TableDefinition("ferrite", "users",
            new KeyDefinition("pk",
                new DataColumn { Name = "user_id", Type = DataType.Long },
                new DataColumn { Name = "phone", Type = DataType.String },
                new DataColumn { Name = "username", Type = DataType.String }),
            new KeyDefinition("by_phone",
                new DataColumn { Name = "phone", Type = DataType.String }),
            new KeyDefinition("by_username",
                new DataColumn { Name = "username", Type = DataType.String })));
        _storeTTL = storeTTL;
        _storeTTL.SetSchema(new TableDefinition("ferrite", "account_ttls",
            new KeyDefinition("pk",
                new DataColumn { Name = "user_id", Type = DataType.Long })));
    }

    public bool PutUser(UserDTO user)
    {
        var userBytes = MessagePackSerializer.Serialize(user);
        return _store.Put(userBytes, user.Id, user.Phone, user.Username ?? "");
    }

    public bool UpdateUsername(long userId, string username)
    {
        var userBytes = _store.Get(userId);
        if (userBytes != null)
        {
            var user = MessagePackSerializer.Deserialize<UserDTO>(userBytes);
            string oldUsername = user.Username ?? "";
            if (username == oldUsername) return false;
            user.Username = username;
            userBytes = MessagePackSerializer.Serialize(user);
            _store.Put(userBytes, user.Id, user.Phone, user.Username ?? "");
            _store.Delete(user.Id, user.Phone, oldUsername);
            return true;
        }

        return false;
    }

    public bool UpdateUserPhone(long userId, string phone)
    {
        var userBytes = _store.Get(userId);
        if (userBytes != null)
        {
            var user = MessagePackSerializer.Deserialize<UserDTO>(userBytes);
            _store.Delete(user); //TODO: fix this by committing together with put
            user.Phone = phone;
            userBytes = MessagePackSerializer.Serialize(user);
            _store.Put(userBytes, user.Id, user.Phone, user.Username ?? "");
            return true;
        }

        return false;
    }

    public UserDTO? GetUser(long userId)
    {
        var userBytes = _store.Get(userId);
        if (userBytes != null)
        {
            return MessagePackSerializer.Deserialize<UserDTO>(userBytes);
        }

        return null;
    }

    public UserDTO? GetUser(string phone)
    {
        var userBytes = _store.GetBySecondaryIndex("by_phone", phone);
        if (userBytes != null)
        {
            return MessagePackSerializer.Deserialize<UserDTO>(userBytes);
        }

        return null;
    }

    public long? GetUserId(string phone)
    {
        var userBytes = _store.GetBySecondaryIndex("by_phone", phone);
        if (userBytes != null)
        {
            var user = MessagePackSerializer.Deserialize<UserDTO>(userBytes);
            return user?.Id;
        }

        return null;
    }

    public UserDTO? GetUserByUsername(string username)
    {
        var userBytes = _store.GetBySecondaryIndex("by_username", username);
        if (userBytes != null)
        {
            return MessagePackSerializer.Deserialize<UserDTO>(userBytes);
        }

        return null;
    }

    public bool DeleteUser(UserDTO user)
    {
        return _store.Delete(user.Id); // delete by prefix as this is less error prone
    }

    public bool UpdateAccountTTL(long userId, int accountDaysTTL)
    {
        var expire = DateTimeOffset.Now.AddDays(accountDaysTTL).ToUnixTimeSeconds();
        return _storeTTL.Put(BitConverter.GetBytes(expire), userId);
    }

    public int GetAccountTTL(long userId)
    {
        var val = _storeTTL.Get(userId);
        if (val == null) return 365;
        var expire = BitConverter.ToInt64(val);
        var expireDays = DateTimeOffset.FromUnixTimeSeconds(expire) - DateTimeOffset.Now;
        return expireDays.Days;
    }
}