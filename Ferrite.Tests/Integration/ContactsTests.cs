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

using System.Net;
using TL;
using TL.Methods;
using WTelegram;
using Xunit;

namespace Ferrite.Tests.Integration;

public class ContactsTests
{
    static ContactsTests()
    {
        var path = "contacts-test-data";
        if(Directory.Exists(path)) Util.DeleteDirectory(path);
        Directory.CreateDirectory(path);
        var ferriteServer = ServerBuilder.BuildServer("127.0.0.1", 52225, path);
        var serverTask = ferriteServer.StartAsync(new IPEndPoint(IPAddress.Any, 52225), default);
        Client.LoadPublicKey(@"-----BEGIN RSA PUBLIC KEY-----
MIIBCgKCAQEAt1YElR7/5enRYr788g210K6QZzUAmaithnSzmQsKb+XL5KhQHrJw
VNMINO17SkB6i4fxG7ydDyFDbLA0Ls6jQZ0mX33Gl8vYWsczPbkbqzs9N6GkOo10
dDVoGObvSHRSwT9zkDixGiq/3b+WPhFcpdJ4OY+5ElD89+fXYgUIhskjYoI8P/PJ
LCf6GLfneJ00N+wDVxAiwOaD6dxvj8kiUDHSwdTXWQ56Nc/Il121fIGbEmho+ShA
fzwQPynnEsA0EyTsqtYHle+KowMhnQYpcvK/iv290NXwRjB4jWtH7tNT/PgB5tud
1LJ9Ta3FusvnDE35w97G6q+yXltErSpM/QIDAQAB
-----END RSA PUBLIC KEY-----
");
    }
    
    private static string ConfigPfs(string what)
    {
        switch (what)
        {
            case "server_address": return "127.0.0.1:52225";
            case "api_id": return "11111";
            case "api_hash": return "11111111111111111111111111111111";
            case "phone_number": return "+15555555555";
            case "verification_code": return "12345";
            case "first_name": return "aaa";
            case "last_name": return "bbb";
            case "pfs_enabled": return "yes";
            default: return null;
        }
    }
    
    [Fact]
    public async Task GetContactIDs_Returns_VectorOfInt()
    {
        using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
        await client.ConnectAsync();
        var auth = await Helpers.SignUp(client, "+15555555614");
        var result = await client.Contacts_GetContactIDs();
        Assert.IsType<int[]>(result);
        Assert.NotNull(result);
    }
    
    [Fact]
    public async Task GetStatuses_Returns_VectorOfContactStatus()
    {
        using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
        await client.ConnectAsync();
        var auth = await Helpers.SignUp(client, "+15555555615");
        var result = await client.Contacts_GetStatuses();
        Assert.IsType<ContactStatus[]>(result);
        Assert.NotNull(result);
    }
    
    [Fact]
    public async Task GetContacts_Returns_Contacts()
    {
        using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
        await client.ConnectAsync();
        var auth = await Helpers.SignUp(client, "+15555555616");
        var result = await client.Contacts_GetContacts();
        Assert.IsType<Contacts_Contacts>(result);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ImportContacts_Returns_ImportedContacts()
    {
        using var clientContacts = new WTelegram.Client(ConfigPfs, new MemoryStream());
        using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
        await client.ConnectAsync();
        var auth = await Helpers.SignUp(client, "+15555555617");
        List<InputContact> inputContacts = new();
        string phoneNumber = "+905555555550";
        await clientContacts.ConnectAsync();
        await Helpers.SignUp(clientContacts, phoneNumber);
        InputPhoneContact c = new()
        {
            first_name = "aaa0",
            last_name = "aaa0",
            phone = phoneNumber,
            client_id = 1
        };
        inputContacts.Add(c);
        var result = await client.Contacts_ImportContacts(inputContacts.ToArray());
        Assert.IsType<Contacts_ImportedContacts>(result);
        Assert.NotNull(result);
        Assert.Single(result.imported);
        Assert.Single(result.users);
    }

    [Fact]
    public async Task DeleteContacts_Returns_Updates()
    {
        using var clientContacts = new WTelegram.Client(ConfigPfs, new MemoryStream());
        using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
        await client.ConnectAsync();
        var auth = await Helpers.SignUp(client, "+15555555618");
        List<InputContact> inputContacts = new();
        List<InputUserBase> inputUsers = new();
        string phoneNumber = "+905555555551";
        await clientContacts.ConnectAsync();
        var authContact = (Auth_Authorization)await Helpers.SignUp(clientContacts, phoneNumber);
        InputPhoneContact c = new()
        {
            first_name = "aaa1",
            last_name = "aaa1",
            phone = phoneNumber,
            client_id = 1
        };
        inputContacts.Add(c);
        inputUsers.Add(authContact.user);
        await client.Contacts_ImportContacts(inputContacts.ToArray());
        var result = await client.Contacts_DeleteContacts(inputUsers.ToArray());
        Assert.IsType<Updates>(result);
        Assert.NotNull(result);
        Assert.Single(((Updates)result).Users);
        Assert.Single(((Updates)result).UpdateList);
    }
    
    [Fact]
    public async Task DeleteByPhones_Returns_True()
    {
        using var clientContacts = new WTelegram.Client(ConfigPfs, new MemoryStream());
        using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
        await client.ConnectAsync();
        var auth = await Helpers.SignUp(client, "+15555555619");
        List<InputContact> inputContacts = new();
        List<string> phoneNumbers = new();
        string phoneNumber = "+905555555572";
        phoneNumbers.Add(phoneNumber);
        await clientContacts.ConnectAsync();
        var a = await Helpers.SignUp(clientContacts, phoneNumber);
        InputPhoneContact c = new()
        {
            first_name = "bbb2",
            last_name = "bbb2",
            phone = phoneNumber,
            client_id = 1
        };
        inputContacts.Add(c);
        await client.Contacts_ImportContacts(inputContacts.ToArray());
        var result = await client.Contacts_DeleteByPhones(phoneNumbers.ToArray());
        Assert.True(result);
    }
    
    [Fact]
    public async Task Block_Returns_True()
    {
        using var clientContacts = new WTelegram.Client(ConfigPfs, new MemoryStream());
        using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
        await client.ConnectAsync();
        var auth = await Helpers.SignUp(client, "+15555555620");
        List<InputContact> inputContacts = new();
        List<InputUser> inputUsers = new();
        string phoneNumber = "+905555555581";
        await clientContacts.ConnectAsync();
        var a = await Helpers.SignUp(clientContacts, phoneNumber);
        inputUsers.Add(((Auth_Authorization)a).user);
        InputPhoneContact c = new()
        {
            first_name = "bbb1",
            last_name = "bbb1",
            phone = phoneNumber,
            client_id = 1
        };
        inputContacts.Add(c);

        await clientContacts.Contacts_ImportContacts(inputContacts.ToArray());
        var result = await client.Contacts_Block(inputUsers[0]);
        Assert.True(result);
    }
    
    [Fact]
    public async Task Unblock_Returns_True()
    {
        using var clientContacts = new WTelegram.Client(ConfigPfs, new MemoryStream());
        using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
        await client.ConnectAsync();
        var auth = await Helpers.SignUp(client, "+15555555621");
        List<InputContact> inputContacts = new();
        List<InputUser> inputUsers = new();
        for (int i = 0; i < 10; i++)
        {
            string phoneNumber = "+90555555559" + i;
            var a = await Helpers.SignUp(clientContacts, phoneNumber);
            inputUsers.Add(((Auth_Authorization)a).user);
            clientContacts.Reset();
            InputPhoneContact c = new()
            {
                first_name = "bbb" + i,
                last_name = "bbb" + i,
                phone = phoneNumber,
                client_id = i + 1
            };
            inputContacts.Add(c);
        }
        await clientContacts.Contacts_ImportContacts(inputContacts.ToArray());
        var result = await client.Contacts_Unblock(inputUsers[0]);
        Assert.True(result);
    }
    
    [Fact]
    public async Task GetBlocked_Returns_Blocked()
    {
        using var clientContacts = new WTelegram.Client(ConfigPfs, new MemoryStream());
        using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
        await client.ConnectAsync();
        var auth = await Helpers.SignUp(client, "+15555555622");
        List<InputContact> inputContacts = new();
        List<InputUser> inputUsers = new();
        for (int i = 0; i < 10; i++)
        {
            string phoneNumber = "+90555555560" + i;
            var a = await Helpers.SignUp(clientContacts, phoneNumber);
            inputUsers.Add(((Auth_Authorization)a).user);
            clientContacts.Reset();
            InputPhoneContact c = new()
            {
                first_name = "bbb" + i,
                last_name = "bbb" + i,
                phone = phoneNumber,
                client_id = i + 1
            };
            inputContacts.Add(c);
        }
        await clientContacts.Contacts_ImportContacts(inputContacts.ToArray());
        var result = await client.Contacts_Block(inputUsers[0]);
        Assert.True(result);
        var resulBlocked = await client.Contacts_GetBlocked();
        Assert.Single(resulBlocked.blocked);
        Assert.Single(resulBlocked.users);
    }
    
    [Fact]
    public async Task Search_Returns_Found()
    {
        using var clientContacts = new WTelegram.Client(ConfigPfs, new MemoryStream());
        using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
        await client.ConnectAsync();
        var auth = await Helpers.SignUp(client, "+15555555623");
        List<InputContact> inputContacts = new();
        List<InputUser> inputUsers = new();
        for (int i = 0; i < 10; i++)
        {
            string phoneNumber = "+90555555561" + i;
            var a = await Helpers.SignUp(clientContacts, phoneNumber);
            inputUsers.Add(((Auth_Authorization)a).user);
            clientContacts.Reset();
            InputPhoneContact c = new()
            {
                first_name = "bbb" + i,
                last_name = "bbb" + i,
                phone = phoneNumber,
                client_id = i + 1
            };
            inputContacts.Add(c);
        }
        await clientContacts.Contacts_ImportContacts(inputContacts.ToArray());
        var results = await client.Contacts_Search("bbb-1");
        Assert.NotNull(results);
        Assert.Single(results.results);
        Assert.Single(results.users);
    }
    
    [Fact]
    public async Task ResolveUsername_Returns_Resolved()
    {
        using var clientContacts = new WTelegram.Client(ConfigPfs, new MemoryStream());
        using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
        await client.ConnectAsync();
        var auth = await Helpers.SignUp(client, "+15555555624");
        string phoneNumber = "+905555555622";
        var a = await Helpers.SignUp(clientContacts, phoneNumber);
        await clientContacts.Account_UpdateUsername("test1234");
        var result = await client.Contacts_ResolveUsername("test1234");
        Assert.NotNull(result);
        Assert.Single(result.users);
        Assert.Equal(((Auth_Authorization)a).user.ID,result.peer.ID);
    }
}