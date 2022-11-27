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

using Ferrite.Crypto;

namespace Ferrite.Services.Gateway;

public class VerificationGateway : IVerificationGateway
{
    private readonly IRandomGenerator _random;

    public VerificationGateway(IRandomGenerator random)
    {
        _random = random;
    }
    
    public ValueTask<string> SendNotification(string phone)
    {
        return ValueTask.FromResult(PrintCode(GetCode()));
    }

    public ValueTask<string> SendCodeViaCall(string phone)
    {
        return ValueTask.FromResult(PrintCode(GetCode()));
    }

    public ValueTask<string> SendCodeViaFlashCall(string phone)
    {
        return ValueTask.FromResult(PrintCode(GetCode()));
    }

    public ValueTask<string> SendCodeViaMissedCall(string phone)
    {
        return ValueTask.FromResult(PrintCode(GetCode()));
    }

    public ValueTask<string> SendEmail(string phone)
    {
        return ValueTask.FromResult(PrintCode(GetCode()));
    }

    public ValueTask<string> SendSms(string phone)
    {
        return ValueTask.FromResult(PrintCode(GetCode()));
    }

    private string PrintCode(int code)
    {
        string codeStr = code.ToString();
        Console.WriteLine($"Verification code is ==> {codeStr}");
        return codeStr;
    }

    private int GetCode()
    {
#if DEBUG
        var code = 12345;
#else
        var code = _random.GetNext(10000, 99999);
#endif
        return code;
    }
}