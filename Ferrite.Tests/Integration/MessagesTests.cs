using System.Net;
using WTelegram;

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
}