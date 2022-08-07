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

public interface IUserRepository
{
    public bool PutUser(UserDTO user);
    public bool UpdateUsername(long userId, string username);
    public bool UpdateUserPhone(long userId, string phone);
    public UserDTO? GetUser(long userId);
    public UserDTO? GetUser(string phone);
    public long? GetUserId(string phone);
    public UserDTO? GetUserByUsername(string username);
    public bool DeleteUser(UserDTO user);
    public bool UpdateAccountTTL(long userId, int accountDaysTTL);
    public int GetAccountTTL(long userId);
}