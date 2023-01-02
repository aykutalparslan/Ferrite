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

using TL;
using TL.Methods;
using WTelegram;
using Xunit;

namespace Ferrite.Tests.Integration;

internal class Helpers
{
    internal static async Task<Auth_AuthorizationBase?> SignUp(Client client, string phoneNumber)
    {
        var code = await client.Invoke(new Auth_SendCode()
        {
            phone_number = phoneNumber, 
            api_id = 11111, 
            api_hash = "11111111111111111111111111111111", 
            settings = new CodeSettings()
        });
        var signIn = await client.Invoke(new Auth_SignIn()
        {
            phone_number = phoneNumber,
            phone_code_hash = code.phone_code_hash,
            phone_code = "12345",
            flags = Auth_SignIn.Flags.has_phone_code
        });
        Assert.IsType<Auth_AuthorizationSignUpRequired>(signIn);
        var result = await client.Invoke(new Auth_SignUp()
        {
            phone_number = phoneNumber,
            phone_code_hash = code.phone_code_hash,
            first_name = "aaa",
            last_name = "bbb",
        });
        return result;
    }
}