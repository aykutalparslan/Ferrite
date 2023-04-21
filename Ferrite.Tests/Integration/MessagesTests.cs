using System.Net;
using TL;
using WTelegram;
using Xunit;

namespace Ferrite.Tests.Integration;

public class MessagesTests
{
    static MessagesTests()
    {
        var path = "messages-test-data";
        if(Directory.Exists(path)) Util.DeleteDirectory(path);
        Directory.CreateDirectory(path);
        var ferriteServer = ServerBuilder.BuildServer("127.0.0.1", 52226, path);
        var serverTask = ferriteServer.StartAsync(new IPEndPoint(IPAddress.Any, 52226), default);
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
            case "server_address": return "127.0.0.1:52226";
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
    public async Task GetMessages_ReturnsMessages()
    {
        using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
        await client.ConnectAsync();
        var auth = await Helpers.SignUp(client, "+15555555625");
        var result = await client.Messages_GetMessages(Array.Empty<InputMessage>());
        Assert.NotNull(result);
        Assert.IsType<Messages_Messages>(result);
    }
    
    [Fact]
    public async Task GetDialogs_Returns_Dialogs()
    {
        using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
        await client.ConnectAsync();
        var auth = await Helpers.SignUp(client, "+15555555626");
        var result = await client.Messages_GetDialogs();
        Assert.NotNull(result);
        Assert.IsType<Messages_Dialogs>(result);
    }
    
    [Fact]
    public async Task GetHistory_Returns_Messages()
    {
        using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
        await client.ConnectAsync();
        var auth = await Helpers.SignUp(client, "+15555555627");
        var result = await client.Messages_GetHistory(new InputPeerSelf());
        Assert.NotNull(result);
        Assert.IsType<Messages_Messages>(result);
    }
    
    [Fact]
    public async Task Search_Returns_Messages()
    {
        using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
        await client.ConnectAsync();
        var auth = await Helpers.SignUp(client, "+15555555628");
        var result = await client.Messages_Search(new InputPeerSelf(), "xxx");
        Assert.NotNull(result);
        Assert.IsType<Messages_Messages>(result);
    }
    
    [Fact]
    public async Task ReadHistory_Returns_AffectedMessages()
    {
        using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
        await client.ConnectAsync();
        var auth = await Helpers.SignUp(client, "+15555555629");
        var result = await client.Messages_ReadHistory(new InputPeerSelf());
        Assert.NotNull(result);
        Assert.IsType<Messages_AffectedMessages>(result);
    }
    
    [Fact]
    public async Task DeleteHistory_Returns_AffectedMessages()
    {
        using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
        await client.ConnectAsync();
        var auth = await Helpers.SignUp(client, "+15555555630");
        var result = await client.Messages_DeleteHistory(new InputPeerSelf());
        Assert.NotNull(result);
        Assert.IsType<Messages_AffectedMessages>(result);
    }
    
    [Fact]
    public async Task DeleteMessages_Returns_AffectedMessages()
    {
        using var client = new WTelegram.Client(ConfigPfs, new MemoryStream());
        await client.ConnectAsync();
        var auth = await Helpers.SignUp(client, "+15555555631");
        var result = await client.Messages_DeleteMessages(new int[]{13});
        Assert.NotNull(result);
        Assert.IsType<Messages_AffectedMessages>(result);
    }
}