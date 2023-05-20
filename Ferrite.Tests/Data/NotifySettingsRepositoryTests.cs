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

using Ferrite.Data.Repositories;
using Ferrite.TL.slim.baseLayer;
using Xunit;

namespace Ferrite.Tests.Data;

public class NotifySettingsRepositoryTests
{
    [Fact]
    public void PutsAndGets_Settings()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        NotifySettingsRepository repo = new (new RocksDBKVStore(ctx));
        using var notifySettings = PeerNotifySettings.Builder()
            .Silent(true)
            .MuteUntil(1000000)
            .ShowPreviews(true)
            .Build();
        repo.PutNotifySettings(123, 0, 0, 
            0, 0, notifySettings);
        var settings = repo.GetNotifySettings(123,
            0, 0, 0, 0);
        Assert.Equal(1, settings.Count);
        Assert.Equal(notifySettings.TLBytes!.Value.AsSpan().ToArray(), 
            settings.First().AsSpan().ToArray());
        Util.DeleteDirectory(path);
    }
    
    [Fact]
    public void PutsDeletes_Settings()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        NotifySettingsRepository repo = new (new RocksDBKVStore(ctx));
        using var notifySettings = PeerNotifySettings.Builder()
            .Silent(true)
            .MuteUntil(1000000)
            .ShowPreviews(true)
            .Build();
        repo.PutNotifySettings(123, 0, 0, 
            0, 0, notifySettings);
        repo.DeleteNotifySettings(123);
        var settings = repo.GetNotifySettings(123,
            0, 0, 0, 0);
        Assert.Equal(0, settings.Count);
        Util.DeleteDirectory(path);
    }
}