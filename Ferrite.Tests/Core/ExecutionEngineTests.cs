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

using System.Buffers.Binary;
using Autofac;
using Autofac.Extras.Moq;
using Ferrite.Core.Execution;
using Ferrite.Core.Execution.Functions;
using Ferrite.TL;
using Ferrite.TL.slim;
using Ferrite.Utils;
using Microsoft.Toolkit.HighPerformance.Buffers;
using Moq;
using Xunit;

namespace Ferrite.Tests.Core;

public class ExecutionEngineTests
{
    [Fact]
    public async Task Should_Invoke_ReqPq()
    {
        var reqPqMulti = MemoryOwner<byte>.Allocate(4);
        BinaryPrimitives.WriteInt32LittleEndian(reqPqMulti.Span, 
            Constructors.mtproto_ReqPqMulti);
        using var tlBytes = new TLBytes(reqPqMulti, 0, 4);

        var reqPqMock = new Mock<ITLFunction>();
        using var autoMock = AutoMock.GetStrict(builder => 
            builder.RegisterInstance(reqPqMock.Object)
                .Keyed<ITLFunction>(new FunctionKey(148, 
                    Constructors.mtproto_ReqPqMulti)));

        var engine = autoMock.Create<ExecutionEngine>();
        await engine.Invoke(tlBytes, 
            new TLExecutionContext(
                new Dictionary<string, object>()));
        
        reqPqMock.Verify(x => 
            x.Process(It.IsAny<TLBytes>(), 
                It.IsAny<TLExecutionContext>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_Log_Exception()
    {
        var reqPqMulti = MemoryOwner<byte>.Allocate(4);
        BinaryPrimitives.WriteInt32LittleEndian(reqPqMulti.Span,
            Constructors.mtproto_ReqPqMulti);
        using var tlBytes = new TLBytes(reqPqMulti, 0, 4);

        var reqPqMock = new Mock<ITLFunction>();
        reqPqMock.Setup(x => x.Process(It.IsAny<TLBytes>(),
                It.IsAny<TLExecutionContext>()))
            .Throws<Exception>().Verifiable("it does not work");

        using var autoMock = AutoMock.GetStrict(builder =>
            builder.RegisterInstance(reqPqMock.Object)
                .Keyed<ITLFunction>(new FunctionKey(146,
                    Constructors.mtproto_ReqPqMulti)));
        
        var loggerMock = autoMock.Mock<ILogger>();
        loggerMock.Setup(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
        var engine = autoMock.Create<ExecutionEngine>();


        await engine.Invoke(tlBytes,
                new TLExecutionContext(
                    new Dictionary<string, object>()));

        loggerMock.Verify(x =>
                x.Error(It.IsAny<Exception>(), It.IsAny<string>()),
            Times.Once);
    }
}