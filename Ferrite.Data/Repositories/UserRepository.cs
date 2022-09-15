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

public class UserRepository : IUserRepository
{
    public bool PutUser(UserDTO user)
    {
        throw new NotImplementedException();
    }

    public bool UpdateUsername(long userId, string username)
    {
        throw new NotImplementedException();
    }

    public bool UpdateUserPhone(long userId, string phone)
    {
        throw new NotImplementedException();
    }

    public UserDTO? GetUser(long userId)
    {
        throw new NotImplementedException();
    }

    public UserDTO? GetUser(string phone)
    {
        throw new NotImplementedException();
    }

    public long? GetUserId(string phone)
    {
        throw new NotImplementedException();
    }

    public UserDTO? GetUserByUsername(string username)
    {
        throw new NotImplementedException();
    }

    public bool DeleteUser(UserDTO user)
    {
        throw new NotImplementedException();
    }

    public bool UpdateAccountTTL(long userId, int accountDaysTTL)
    {
        throw new NotImplementedException();
    }

    public int GetAccountTTL(long userId)
    {
        throw new NotImplementedException();
    }
}