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

using Autofac.Extras.Moq;
using Ferrite.Data.Repositories;
using Ferrite.Services;
using Ferrite.Services.Gateway;
using Ferrite.TL.slim;
using Ferrite.TL.slim.layer148;
using Ferrite.TL.slim.layer148.auth;
using Moq;
using Xunit;

namespace Ferrite.Tests.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task SendCode_Sends_Code()
    {
        using var mocker = AutoMock.GetLoose();
        var verificationGateway = mocker.Mock<IVerificationGateway>();
        verificationGateway.Setup(v => 
            v.SendSms(It.IsAny<string>())).ReturnsAsync("12345");
        var unitOfWork = mocker.Mock<IUnitOfWork>();
        var phoneCodeRepository = mocker.Mock<IPhoneCodeRepository>();
        unitOfWork.SetupGet(u => u.PhoneCodeRepository).Returns(phoneCodeRepository.Object);
        var authService = mocker.Create<AuthService>();
        using var sendCode = GenerateSendCode();
        using var sentCode = await authService.SendCode(sendCode);
        Assert.Equal(Constructors.layer148_SentCode, 
            sentCode.Constructor);
        verificationGateway.Verify(v=>v.SendSms(It.IsAny<string>()), Times.Once);
        phoneCodeRepository.Verify(p=>p.PutPhoneCode(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(),It.IsAny<TimeSpan>()), Times.Once);
        unitOfWork.Verify(u=>u.SaveAsync(), Times.Once);
    }

    private TLBytes GenerateSendCode()
    {
        using var codeSettings = CodeSettings.Builder().Build();
        var sendCode = SendCode.Builder()
            .Settings(codeSettings.ToReadOnlySpan())
            .ApiHash("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"u8)
            .ApiId(12345)
            .PhoneNumber("+15555555555"u8)
            .Build();
        return (TLBytes)sendCode.TLBytes!;
    }
    
    [Fact]
    public async Task ResendCode_Sends_Code()
    {
        /*
         * Server supports SMS only. Normally we would need to use a different method per the specification
         * 
         * auth.resendCode
         * Resend the login code via another medium, the phone code type is determined by the return value of the
         * previous auth.sendCode/auth.resendCode: see login for more info.
         */
        using var mocker = AutoMock.GetLoose();
        var verificationGateway = mocker.Mock<IVerificationGateway>();
        verificationGateway.Setup(v => 
            v.SendSms(It.IsAny<string>())).ReturnsAsync("12345");
        var unitOfWork = mocker.Mock<IUnitOfWork>();
        var phoneCodeRepository = mocker.Mock<IPhoneCodeRepository>();
        phoneCodeRepository.Setup(p =>
            p.GetPhoneCode(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("test");
        unitOfWork.SetupGet(u => u.PhoneCodeRepository).Returns(phoneCodeRepository.Object);
        var authService = mocker.Create<AuthService>();
        using var sendCode = GenerateSendCode();
        using var sentCode = await authService.SendCode(sendCode);
        using var resendCode = GenerateResendCode();
        using var sentCode2 = await authService.ResendCode(resendCode);
        Assert.Equal(Constructors.layer148_SentCode, 
            sentCode2.Constructor);
        verificationGateway.Verify(v=>v.SendSms(It.IsAny<string>()), Times.Once);
        phoneCodeRepository.Verify(p=>p.PutPhoneCode(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(),It.IsAny<TimeSpan>()), Times.Exactly(2));
        phoneCodeRepository.Verify(p=>p.GetPhoneCode(
            It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        unitOfWork.Verify(u=>u.SaveAsync(), Times.Exactly(2));
    }
    
    private TLBytes GenerateResendCode()
    {
        var sendCode = ResendCode.Builder()
            .PhoneNumber("+15555555555"u8)
            .PhoneCodeHash("test"u8)
            .Build();
        return (TLBytes)sendCode.TLBytes!;
    }
}