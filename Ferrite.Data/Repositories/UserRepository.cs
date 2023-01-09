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
using Ferrite.TL.slim;
using Ferrite.TL.slim.layer150;

namespace Ferrite.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IKVStore _store;
    private readonly IKVStore _storeTtl;
    private readonly IKVStore _storeAbout;

    public UserRepository(IKVStore store, IKVStore storeTtl, IKVStore storeAbout)
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
        _storeTtl = storeTtl;
        _storeTtl.SetSchema(new TableDefinition("ferrite", "account_ttls",
            new KeyDefinition("pk",
                new DataColumn { Name = "user_id", Type = DataType.Long })));
        _storeAbout = storeAbout;
        _storeAbout.SetSchema(new TableDefinition("ferrite", "users_about",
            new KeyDefinition("pk",
                new DataColumn { Name = "user_id", Type = DataType.Long })));
    }

    public bool PutUser(TLBytes user)
    {
        var u = new User(user.AsSpan());
        return _store.Put(user.AsSpan().ToArray(),
            u.Id, Encoding.UTF8.GetString(u.Phone),
            u.Username.Length > 0 ? Encoding.UTF8.GetString(u.Username) : "");
    }

    public bool UpdateUsername(long userId, string username)
    {
        var userBytes = _store.Get(userId);
        if (userBytes != null)
        {
            var user = new User(userBytes);
            string oldUsername = user.Username.Length > 0 ? Encoding.UTF8.GetString(user.Username) : "";
            if (username == oldUsername) return false;
            string userPhone = Encoding.UTF8.GetString(user.Phone);
            var userNew = user.Clone().Username(Encoding.UTF8.GetBytes(username)).Build();
            _store.Put(userNew.TLBytes!.Value.AsSpan().ToArray(),
                user.Id, userPhone, username);
            _store.Delete(user.Id, userPhone, oldUsername);
            return true;
        }

        return false;
    }

    public bool UpdateUserPhone(long userId, string phone)
    {
        var userBytes = _store.Get(userId);
        if (userBytes != null)
        {
            var user = new User(userBytes);
            string oldPhone = user.Phone.Length > 0 ? Encoding.UTF8.GetString(user.Phone) : "";
            if (oldPhone == phone) return false;
            var userNew = user.Clone().Phone(Encoding.UTF8.GetBytes(phone)).Build();
            string username = user.Username.Length > 0 ? Encoding.UTF8.GetString(user.Username) : "";
            _store.Put(userNew.TLBytes!.Value.AsSpan().ToArray(), 
                user.Id, phone, username);
            _store.Delete(user.Id, oldPhone, username);
            return true;
        }

        return false;
    }

    public TLBytes? GetUser(long userId)
    {
        var userBytes = _store.Get(userId);
        if (userBytes != null)
        {
            return new TLBytes(userBytes, 0, userBytes.Length);
        }

        return null;
    }

    public TLBytes? GetUser(string phone)
    {
        var userBytes = _store.GetBySecondaryIndex("by_phone", phone);
        if (userBytes != null)
        {
            return new TLBytes(userBytes, 0, userBytes.Length);
        }

        return null;
    }

    public long? GetUserId(string phone)
    {
        var userBytes = _store.GetBySecondaryIndex("by_phone", phone);
        if (userBytes != null)
        {
            var user = new User(userBytes);
            return user.Id;
        }

        return null;
    }

    public TLBytes? GetUserByUsername(string username)
    {
        var userBytes = _store.GetBySecondaryIndex("by_username", username);
        if (userBytes != null)
        {
            return new TLBytes(userBytes, 0, userBytes.Length);
        }

        return null;
    }

    public bool DeleteUser(long userId)
    {
        return _store.Delete(userId); // delete by prefix
    }

    public bool UpdateAccountTtl(long userId, int accountDaysTtl)
    {
        var expire = DateTimeOffset.Now.AddDays(accountDaysTtl).ToUnixTimeSeconds();
        return _storeTtl.Put(BitConverter.GetBytes(expire), userId);
    }

    public int GetAccountTtl(long userId)
    {
        var val = _storeTtl.Get(userId);
        if (val == null) return 365;
        var expire = BitConverter.ToInt64(val);
        var expireDays = DateTimeOffset.FromUnixTimeSeconds(expire) - DateTimeOffset.Now;
        return expireDays.Days;
    }

    public bool PutAbout(long userId, string about)
    {
        return _storeAbout.Put(Encoding.UTF8.GetBytes(about), userId);
    }

    public string? GetAbout(long userId)
    {
        var about = _storeAbout.Get(userId);
        return about == null ? null : Encoding.UTF8.GetString(about);
    }
}