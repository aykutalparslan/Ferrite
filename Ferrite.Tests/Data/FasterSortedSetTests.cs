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
using Xunit;

namespace Ferrite.Tests.Data;

public class FasterSortedSetTests
{
    [Fact]
    public async Task FasterSortedSet_Should_Add()
    {

        string path = "faster-sorted-set-test-" + Random.Shared.Next();
        FasterSortedSet<long> sortedSet =
            new FasterSortedSet<long>(new FasterContext<string, SortedSet<long>>(path), "test1");

        for (int i = 0; i < 20; i++)
        {
            sortedSet.Add(i);
        }
        await sortedSet.Add(20);

        var s = sortedSet.Get();
        Assert.Equal(21, s.Count);
        for (int i = 0; i < 21; i++)
        { 
            Assert.True(s.Contains(i));
        }
        DeleteDirectory(path);
    }
    [Fact]
    public async Task FasterSortedSet_Should_Remove()
    {
        string path = "faster-sorted-set-test-" + Random.Shared.Next();
        FasterSortedSet<long> sortedSet =
            new FasterSortedSet<long>(new FasterContext<string, SortedSet<long>>(path), "test1");

        for (int i = 0; i < 20; i++)
        { 
            sortedSet.Add(i);
        }
        await sortedSet.Add(20);
        await sortedSet.Remove(3);
        await sortedSet.Remove(8);
        await sortedSet.Remove(12);
        var s = sortedSet.Get();
        Assert.Equal(18, s.Count);
        for (int i = 0; i < 20; i++)
        {
            if (i != 3 && i != 8 && i != 12)
            {
                Assert.True(s.Contains(i));
            }
        }
        DeleteDirectory(path);
    }
    [Fact]
    public async Task FasterSortedSet_Should_RemoveEqualOrLess()
    {
        string path = "faster-sorted-set-test-" + Random.Shared.Next();
        FasterSortedSet<long> sortedSet =
            new FasterSortedSet<long>(new FasterContext<string, SortedSet<long>>(path), "test1");

        for (int i = 0; i < 20; i++)
        { 
            sortedSet.Add(i);
        }
        await sortedSet.Add(20);
        await sortedSet.RemoveEqualOrLess(5);
        var s = sortedSet.Get();
        Assert.Equal(15, s.Count);
        for (int i = 0; i < 20; i++)
        {
            if (i > 5)
            {
                Assert.True(s.Contains(i));
            }
        }
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