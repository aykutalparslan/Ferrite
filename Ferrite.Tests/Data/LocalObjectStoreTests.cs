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

public class LocalObjectStoreTests
{
    [Theory]
    [InlineData(512)]
    [InlineData(1024)]
    [InlineData(2048)]
    [InlineData(4096)]
    [InlineData(8192)]
    public async Task LocalObjectStore_ShouldSaveAndGet(int len)
    {
        if(Directory.Exists("object-metadata")) DeleteDirectory("object-metadata");
        if(Directory.Exists("ferrite-files")) DeleteDirectory("ferrite-files");
        FasterContext<ObjectId, ObjectMetadata> ctx = new("object-metadata");
        LocalObjectStore store = new (ctx, "ferrite-files");
        var randomBytes = new byte[len * 10];
        Random.Shared.NextBytes(randomBytes);
        var l = new List<byte[]>();
        for (int i = 0; i < 10; i++)
        {
            var b = randomBytes.AsSpan(i * len, len).ToArray();
            l.Add(b);
        }

        for (int i = 0; i < 10; i++)
        {
            await store.SaveFilePart(1, i, new MemoryStream(l[i]));
        }
        for (int i = 0; i < 10; i++)
        {
            var fileStream = await store.GetFilePart(1, i);
            var actual = new byte[fileStream.Length];
            fileStream.Read(actual);
            fileStream.Close();
            Assert.Equal(l[i], actual);
        }
        if(Directory.Exists("object-metadata")) DeleteDirectory("object-metadata");
        if(Directory.Exists("ferrite-files")) DeleteDirectory("ferrite-files");
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