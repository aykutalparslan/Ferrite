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
using Ferrite.Data;
using Ferrite.Data.Repositories;
using Ferrite.TL.slim;
using Ferrite.TL.slim.layer150;
using Moq;
using Xunit;

namespace Ferrite.Tests.Data;

public class PrivacyRulesRepositoryTests
{
    [Fact]
    public void Puts_PrivacyRules()
    {
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Put(It.IsAny<byte[]>(), 
            It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>())).Verifiable();

        var repo = mock.Create<PrivacyRulesRepository>();
        repo.PutPrivacyRules(1, InputPrivacyKey.PhoneCall, GenerateRules());
        store.VerifyAll();
    }
    
    [Fact]
    public void Deletes_PrivacyRules()
    {
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Delete(It.IsAny<long>())).Verifiable();

        var repo = mock.Create<PrivacyRulesRepository>();
        repo.DeletePrivacyRules(1);
        store.VerifyAll();
    }
    
    [Fact]
    public async Task PutsAndGets_PrivacyRules()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        PrivacyRulesRepository repo = new (new RocksDBKVStore(ctx));
        repo.PutPrivacyRules(1, InputPrivacyKey.PhoneCall, GenerateRules());
        var rulesFromRepo = await repo.GetPrivacyRulesAsync(1, InputPrivacyKey.PhoneCall);
        Assert.Equal(2, rulesFromRepo.Count);
        Assert.Equal(PrivacyValueAllowContacts.Builder().Build().ToReadOnlySpan().ToArray(), 
            rulesFromRepo.First().AsSpan().ToArray());
        Util.DeleteDirectory(path);
    }

    private Vector GenerateRules()
    {
        var rules = new Vector();
        rules.AppendTLObject(PrivacyValueAllowContacts.Builder().Build().ToReadOnlySpan());
        rules.AppendTLObject(PrivacyValueDisallowAll.Builder().Build().ToReadOnlySpan());
        return rules;
    }
}