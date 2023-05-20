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

using Ferrite.TL.slim;
using Ferrite.TL.slim.baseLayer;
using Ferrite.TL.slim.baseLayer.dto;
using MessagePack;
using TLUserStatus = Ferrite.TL.slim.baseLayer.TLUserStatus;

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
        var statusBytes = UserStatusFull.Builder()
                .Status(status)
                .WasOnline((int)DateTimeOffset.Now.ToUnixTimeSeconds())
                .Expires(10)
                .Build().TLBytes!.Value;
        return _store.Put(statusBytes.AsSpan().ToArray(), userId);
    }

    public async ValueTask<TLUserStatus> GetUserStatusAsync(long userId)
    {
        var serialized = await _store.GetAsync(userId);
        if (serialized == null)
        {
            return new UserStatusEmpty();
        }

        var userStatus = new TLUserStatusFull(serialized, 0, serialized.Length);
        if (!userStatus.AsUserStatusFull().Status)
        {
            return new UserStatusOffline();
        }

        int wasOnline = userStatus.AsUserStatusFull().WasOnline;
        if (wasOnline + userStatus.AsUserStatusFull().Expires < DateTimeOffset.Now.ToUnixTimeSeconds())
        {
            return new UserStatusOnline(userStatus.AsUserStatusFull().Expires);
        }
        if(wasOnline < DateTimeOffset.Now.AddDays(-1).ToUnixTimeSeconds())
        {
            return new UserStatusRecently();
        }
        if(wasOnline < DateTimeOffset.Now.AddDays(-7).ToUnixTimeSeconds())
        {
            return new UserStatusLastWeek();
        }
        if(wasOnline < DateTimeOffset.Now.AddDays(-30).ToUnixTimeSeconds())
        {
            return new UserStatusLastMonth();
        }
        return new UserStatusOffline();
    }
}