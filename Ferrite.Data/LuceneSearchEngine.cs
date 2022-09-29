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

using Ferrite.Data.Search;
using Lucene.Net.Analysis.En;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Surround.Parser;
using Lucene.Net.QueryParsers.Surround.Query;
using Lucene.Net.Search;
using Lucene.Net.Util;

namespace Ferrite.Data;

public class LuceneSearchEngine : ISearchEngine
{
    private readonly string _path;
    private readonly LuceneContext _users;
    private readonly LuceneContext _messages;

    public LuceneSearchEngine(string path)
    {
        _path = path;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        _users = new(Path.Combine(path,"lucene-users"));
        _messages = new(Path.Combine(path,"lucene-messages"));
    }

    public ValueTask<bool> IndexUser(UserSearchModel user)
    {
        var doc = new Document();
        if (user.Username != null) LuceneContext.AddField(user.Username, doc, "username");
        if (user.Phone != null) LuceneContext.AddField(user.Phone, doc, "phone");
        if (user.FirstName != null) LuceneContext.AddField(user.FirstName, doc, "firstname");
        if (user.LastName != null) LuceneContext.AddField(user.LastName, doc, "lastname");
        _users.Index(user.Id.ToString(), doc);
        return ValueTask.FromResult(true);
    }

    public ValueTask<bool> DeleteUser(long userId)
    {
        _users.Delete(userId.ToString());
        return ValueTask.FromResult(true);
    }

    public ValueTask<List<UserSearchModel>> SearchUser(string q, int limit)
    {
        var query = new BooleanQuery();
        query.Add(new BooleanClause(new PrefixQuery(new Term("username", q)), Occur.SHOULD));
        query.Add(new BooleanClause(new PrefixQuery(new Term("firstname", q)), Occur.SHOULD));
        query.Add(new BooleanClause(new PrefixQuery(new Term("lastname", q)), Occur.SHOULD));
        //query.Add(new BooleanClause(new PrefixQuery(new Term("phone", q)), Occur.SHOULD));
        var docs = _users.Search(query, int.Min(limit, 50));
        List<UserSearchModel> results = new();
        foreach (var d in docs)
        {
            UserSearchModel m = new UserSearchModel(
                long.Parse(d.GetField("_id").GetStringValue()),
                d.GetField("username") != null ? d.GetField("username").GetStringValue() : null,
                d.GetField("firstname") != null ? d.GetField("firstname").GetStringValue() : null,
                d.GetField("lastname") != null ? d.GetField("lastname").GetStringValue() : null,
                d.GetField("phone") != null ? d.GetField("phone").GetStringValue() : null);
            results.Add(m);
        }

        return ValueTask.FromResult(results);
    }

    public ValueTask<bool> IndexMessage(MessageSearchModel message)
    {
        var doc = new Document();
        LuceneContext.AddField(message.Message, doc, "message");
        LuceneContext.AddField(message.Date, doc, "date");
        LuceneContext.AddField(message.FromId, doc, "fromid");
        LuceneContext.AddField(message.FromType, doc, "fromtype");
        LuceneContext.AddField(message.MessageId, doc, "messageid");
        LuceneContext.AddField(message.PeerId, doc, "peerid");
        LuceneContext.AddField(message.PeerType, doc, "peertype");
        LuceneContext.AddField(message.UserId, doc, "userid");
        if(message.TopMessageId != null) LuceneContext.AddField(message.TopMessageId, doc, "topmessageid");
        
        _messages.Index(message.Id, doc);
        return ValueTask.FromResult(true);
    }

    public ValueTask<bool> DeleteMessage(string id)
    {
        _messages.Delete(id);
        return ValueTask.FromResult(true);
    }

    public ValueTask<List<MessageSearchModel>> SearchMessages(string q)
    {
        List<MessageSearchModel> results = new();
        var query = new MultiPhraseQuery();
        string[] terms = q.Split(" ");
        foreach (var t in terms)
        {
            query.Add(new Term("message", t));
        }
        
        var docs = _messages.Search(query, 50);
        foreach (var d in docs)
        {
            MessageSearchModel m = new MessageSearchModel(
                d.GetField("_id").GetStringValue(),
                (long)d.GetField("userid").GetInt64Value(),
                (int)d.GetField("fromtype").GetInt32Value(),
                (long)d.GetField("fromid").GetInt64Value(),
                (int)d.GetField("peertype").GetInt32Value(),
                (long)d.GetField("peerid").GetInt64Value(),
                (int)d.GetField("messageid").GetInt32Value(),
                d.GetField("topmessageid") != null ? (int)d.GetField("topmessageid").GetInt32Value() : null,
            d.GetField("message").GetStringValue(),
                (int)d.GetField("date").GetInt32Value());
            results.Add(m);
        }
        return ValueTask.FromResult(results);
    }
}