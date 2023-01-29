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
using Ferrite.TL.slim.dto;
using Ferrite.TL.slim.layer150;
using Moq;
using Xunit;

namespace Ferrite.Tests.Data;

public class ReportReasonRepositoryTests
{
    [Fact]
    public void Puts_PeerReportReason()
    {
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Put(It.IsAny<byte[]>(),
            It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>())).Verifiable();

        ReportReasonRepository repo = mock.Create<ReportReasonRepository>();
        using var reportReason = InputReportReasonSpam.Builder().Build();
        using var reason = ReportReasonWithMessage.Builder()
            .ReportReason(reportReason.ToReadOnlySpan())
            .Message("test"u8).Build();
        repo.PutPeerReportReason(1, 1, 2, reason);
        store.VerifyAll();
    }
}