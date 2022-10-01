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

using Ferrite.Data;
using Ferrite.Data.Search;
using Xunit;

namespace Ferrite.Tests.Data;

public class LuceneSearchEngineTests
{
    [Fact]
    public async Task LuceneSearchEngine_ShouldIndexAndSearchUsers()
    {
        string path = "users_test" + Random.Shared.Next();
        LuceneSearchEngine search = new(path);
        var expected = new UserSearchModel(1,
            "test",
            "aaa",
            "bbb",
            "555");
        await search.IndexUser(expected);
        var result = await search.SearchUser("te", 10);
        Assert.Equal(expected, result.FirstOrDefault());
        result = await search.SearchUser("aa", 10);
        Assert.Equal(expected, result.FirstOrDefault());
        result = await search.SearchUser("bb", 10);
        Assert.Equal(expected, result.FirstOrDefault());
        DeleteDirectory(path);
    }
    [Fact]
    public async Task LuceneSearchEngine_ShouldSearchUsersWithMissingFields()
    {
        string path = "users_test" + Random.Shared.Next();
        LuceneSearchEngine search = new(path);
        var expected = new UserSearchModel(1,
            null,
            "aaa",
            "bbb",
            "555");
        await search.IndexUser(expected);
        var result = await search.SearchUser("aa", 10);
        Assert.Equal(expected, result.FirstOrDefault());
        result = await search.SearchUser("bb", 10);
        Assert.Equal(expected, result.FirstOrDefault());
        DeleteDirectory(path);
    }
    [Theory]
    [InlineData("test asdf hhg", "test")]
    [InlineData("ccc", "ccc")]
    public async Task LuceneSearchEngine_ShouldIndexAndSearchMessages(string message, string q)
    {
        string path = "messages_test" + Random.Shared.Next();
        LuceneSearchEngine search = new(path);
        var expected = new MessageSearchModel(111+"-"+222,
            111,
            1,
            222,
            2,
            111,
            5,
            3,
            message,
            0);
        await search.IndexMessage(expected);
        var result = await search.SearchMessages(q);
        Assert.Equal(expected, result.FirstOrDefault());
        DeleteDirectory(path);
    }
    //From: https://stackoverflow.com/a/329502/2015348
    private static void DeleteDirectory(string target_dir)
    {
        string[] files = Directory.GetFiles(target_dir);
        string[] dirs = Directory.GetDirectories(target_dir);

        foreach (string file in files)
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        foreach (string dir in dirs)
        {
            DeleteDirectory(dir);
        }

        Directory.Delete(target_dir, false);
    }
}