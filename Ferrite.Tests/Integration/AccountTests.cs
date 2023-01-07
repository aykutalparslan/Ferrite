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
using System.Security.Cryptography;
using TL;
using WTelegram;
using Xunit;

namespace Ferrite.Tests.Integration;

// Includes a modified portion of:
// https://github.com/wiz0u/WTelegramClient/blob/e7ec282ac10afe4769b2af2d383efdf201af1348/src/Client.cs
public class AccountTests
{
    static AccountTests()
    {
        var path = "account-test-data";
        if(Directory.Exists(path)) Util.DeleteDirectory(path);
        Directory.CreateDirectory(path);
        var ferriteServer = ServerBuilder.BuildServer("10.0.2.2", 52223, path);
        var serverTask = ferriteServer.StartAsync(new IPEndPoint(IPAddress.Any, 52223), default);
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
            case "server_address": return "127.0.0.1:52223";
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
    public async Task RegisterDevice_Returns_True()
    {
        async Task RunTest()
        {
            using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
            await client.ConnectAsync();
            var auth = await Helpers.SignUp(client, "+15555555577");
            Assert.NotNull(client.TLConfig);
            var result = await client.Account_RegisterDevice(
                2,
                "testtoken",
                false,
                RandomNumberGenerator.GetBytes(32),
                Array.Empty<long>());
        }

        Task testTask = RunTest();
        await testTask.TimeoutAfter(4000);
    }
    
    [Fact]
    public async Task UnregisterDevice_Returns_True()
    {
        async Task RunTest()
        {
            using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
            await client.ConnectAsync();
            var auth = await Helpers.SignUp(client, "+15555555578");
            Assert.NotNull(client.TLConfig);
            var result = await client.Account_UnregisterDevice(
                2,
                "testtoken",
                Array.Empty<long>());
        }

        Task testTask = RunTest();
        await testTask.TimeoutAfter(4000);
    }
    
    [Fact]
    public async Task UpdateNotifySettings_Returns_True()
    {
        async Task RunTest()
        {
            using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
            await client.ConnectAsync();
            var auth = await Helpers.SignUp(client, "+15555555579");
            Assert.NotNull(client.TLConfig);
            var registerResult = await client.Account_RegisterDevice(
                2,
                "testtoken",
                false,
                RandomNumberGenerator.GetBytes(32),
                Array.Empty<long>());
            var result = await client.Account_UpdateNotifySettings(
                new InputNotifyUsers(),
                new InputPeerNotifySettings());
            Assert.True(result);
        }

        Task testTask = RunTest();
        await testTask.TimeoutAfter(4000);
    }
    
    [Fact]
    public async Task GetNotifySettings_Returns_PeerNotifySettings()
    {
        async Task RunTest()
        {
            using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
            await client.ConnectAsync();
            var auth = await Helpers.SignUp(client, "+15555555580");
            Assert.NotNull(client.TLConfig);
            var result = await client.Account_UpdateNotifySettings(
                new InputNotifyUsers(),
                new InputPeerNotifySettings());
            Assert.True(result);
            var settings = await client.Account_GetNotifySettings(
                new InputNotifyUsers());
            Assert.IsType<PeerNotifySettings>(settings);
        }

        Task testTask = RunTest();
        await testTask.TimeoutAfter(4000);
    }
    
    [Fact]
    public async Task ResetNotifySettings_Returns_True()
    {
        async Task RunTest()
        {
            using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
            await client.ConnectAsync();
            var auth = await Helpers.SignUp(client, "+15555555581");
            Assert.NotNull(client.TLConfig);
            var result = await client.Account_ResetNotifySettings();
            Assert.True(result);
        }

        Task testTask = RunTest();
        await testTask.TimeoutAfter(4000);
    }
}