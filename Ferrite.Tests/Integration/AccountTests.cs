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
using Authorization = TL.Authorization;
using PrivacyValueAllowUsers = Ferrite.TL.slim.layer150.PrivacyValueAllowUsers;

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
                new InputPeerNotifySettings()
                {
                    flags = InputPeerNotifySettings.Flags.has_mute_until | InputPeerNotifySettings.Flags.has_silent,
                    silent = true,
                    mute_until = 12345,
                });
            Assert.True(result);
            var settings = await client.Account_GetNotifySettings(
                new InputNotifyUsers());
            Assert.IsType<PeerNotifySettings>(settings);
            Assert.Equal(12345, settings.mute_until);
            Assert.True(settings.silent);
            Assert.False(settings.show_previews);
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
            var resultUpdate = await client.Account_UpdateNotifySettings(
                new InputNotifyUsers(),
                new InputPeerNotifySettings()
                {
                    flags = InputPeerNotifySettings.Flags.has_mute_until | InputPeerNotifySettings.Flags.has_silent,
                    silent = true,
                    mute_until = 12345,
                });
            Assert.True(resultUpdate);
            var settings = await client.Account_GetNotifySettings(
                new InputNotifyUsers());
            Assert.IsType<PeerNotifySettings>(settings);
            Assert.Equal(12345, settings.mute_until);
            Assert.True(settings.silent);
            var result = await client.Account_ResetNotifySettings();
            Assert.True(result);
            settings = await client.Account_GetNotifySettings(
                new InputNotifyUsers());
            Assert.IsType<PeerNotifySettings>(settings);
            Assert.Equal(0, settings.mute_until);
            Assert.False(settings.silent);
        }

        Task testTask = RunTest();
        await testTask.TimeoutAfter(4000);
    }
    
    [Fact]
    public async Task UpdateProfile_Returns_User()
    {
        async Task RunTest()
        {
            using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
            await client.ConnectAsync();
            var auth = await Helpers.SignUp(client, "+15555555582");
            Assert.NotNull(client.TLConfig);
            var result = await client.Account_UpdateProfile("xxx","yyy","zzz");
            Assert.IsType<User>(result);
            Assert.Equal("xxx", ((User)result).first_name);
            Assert.Equal("yyy", ((User)result).last_name);
        }

        Task testTask = RunTest();
        await testTask.TimeoutAfter(4000);
    }
    
    [Fact]
    public async Task UpdateStatus_Returns_True()
    {
        async Task RunTest()
        {
            using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
            await client.ConnectAsync();
            var auth = await Helpers.SignUp(client, "+15555555583");
            Assert.NotNull(client.TLConfig);
            var result = await client.Account_UpdateStatus(false);
            Assert.True(result);
        }

        Task testTask = RunTest();
        await testTask.TimeoutAfter(4000);
    }
    
    [Fact]
    public async Task ReportPeer_Returns_True()
    {
        async Task RunTest()
        {
            using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
            await client.ConnectAsync();
            var auth = await Helpers.SignUp(client, "+15555555584");
            Assert.NotNull(client.TLConfig);
            Assert.NotNull(auth);
            var peer = ((Auth_Authorization)auth).user.ToInputPeer();
            client.Dispose();
            using var client2 = new WTelegram.Client(ConfigPfs, new MemoryStream());
            await client2.ConnectAsync();
            auth = await Helpers.SignUp(client2, "+15555555585");
            var result = await client2.Account_ReportPeer(peer, ReportReason.Spam, "test message");
            Assert.True(result);
        }

        Task testTask = RunTest();
        await testTask.TimeoutAfter(4000);
    }
    
    [Fact]
    public async Task CheckUsername_Returns_True()
    {
        async Task RunTest()
        {
            using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
            await client.ConnectAsync();
            var auth = await Helpers.SignUp(client, "+15555555586");
            Assert.NotNull(client.TLConfig);
            var result = await client.Account_CheckUsername("test123_");
            Assert.True(result);
        }

        Task testTask = RunTest();
        await testTask.TimeoutAfter(4000);
    }
    
    [Fact]
    public async Task CheckUsername_Returns_False()
    {
        async Task RunTest()
        {
            using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
            await client.ConnectAsync();
            var auth = await Helpers.SignUp(client, "+15555555587");
            Assert.NotNull(client.TLConfig);
            var result = await client.Account_CheckUsername("test123.");
            Assert.False(result);
        }

        Task testTask = RunTest();
        await testTask.TimeoutAfter(4000);
    }
    
    [Fact]
    public async Task UpdateUsername_Updates_Username()
    {
        async Task RunTest()
        {
            using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
            await client.ConnectAsync();
            var auth = await Helpers.SignUp(client, "+15555555588");
            Assert.NotNull(client.TLConfig);
            var result = await client.Account_UpdateUsername("test123_");
            Assert.Equal("test123_", ((User)result).username);
        }

        Task testTask = RunTest();
        await testTask.TimeoutAfter(4000);
    }
    
    [Fact]
    public async Task UpdateUsername_Throws()
    {
        async Task RunTest()
        {
            using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
            await client.ConnectAsync();
            var auth = await Helpers.SignUp(client, "+15555555589");
            Assert.NotNull(client.TLConfig);
            await Assert.ThrowsAsync<RpcException>(async () =>
            {
                await client.Account_UpdateUsername("test123_.");
            });
        }

        Task testTask = RunTest();
        await testTask.TimeoutAfter(4000);
    }
    
    [Fact]
    public async Task SetPrivacy_Returns_PrivacyRules()
    {
        async Task RunTest()
        {
            using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
            await client.ConnectAsync();
            var auth = await Helpers.SignUp(client, "+15555555590");
            Assert.NotNull(client.TLConfig);
            List<InputPrivacyRule> rules = new();
            rules.Add(new InputPrivacyValueAllowAll());
            var result = await client.Account_SetPrivacy(InputPrivacyKey.PhoneCall, rules.ToArray());
            Assert.NotNull(result);
            Assert.Single(result.rules);
            Assert.IsType<PrivacyValueAllowAll>(result.rules[0]);
        }

        Task testTask = RunTest();
        await testTask.TimeoutAfter(4000);
    }
}