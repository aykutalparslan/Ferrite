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
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Xunit;

namespace Ferrite.Tests.Data;

public class LuceneContextTests
{
    [Fact]
    public void LuceneContext_Should_IndexSearchAndDelete()
    {
        LuceneContext context = new LuceneContext("lucene-test");
        for (int i = 0; i < 20; i++)
        {
            var doc = new Document
            {
                new StringField("name",
                    "name-" + (i * 11111),
                    Field.Store.YES),
                new TextField("favoritePhrase",
                    "phrase-" + (i * 11111),
                    Field.Store.YES)
            };
            context.Index(i.ToString(), doc);
        }

        var docs = context.Search(new MultiPhraseQuery
        {
            new Term("favoritePhrase", "phrase")
        }, 5);
        Assert.Equal(5, docs.Count());
        docs = context.Search(new MultiPhraseQuery
        {
            new Term("favoritePhrase", "55555")
        }, 1);
        context.Delete("7");
        docs = context.Search(new MultiPhraseQuery
        {
            new Term("favoritePhrase", "phrase")
        }, 30);
        Assert.Equal(19, docs.Count());
        DeleteDirectory("lucene-test");
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