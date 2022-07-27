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

using Elasticsearch.Net;
using Ferrite.Data.Search;
using Nest;

namespace Ferrite.Data;

public class ElasticSearchEngine : ISearchEngine
{
    private readonly ElasticClient _client;
    public ElasticSearchEngine(string url,string username, string password, string fingerprint)
    {
        var uri = new Uri(url);
        var pool = new SingleNodeConnectionPool(uri);
        var connectionSettings = new ConnectionSettings(pool)
            .BasicAuthentication(username, password)
            .EnableDebugMode()
            .PrettyJson()
            .CertificateFingerprint(fingerprint)
            .RequestTimeout(TimeSpan.FromSeconds(5));
        _client = new ElasticClient(connectionSettings);
    }

    public async Task<bool> IndexUser(Search.UserSearchModel user)
    {
        var result = await _client.IndexAsync(user, _ => _.Index("users").Id(user.Id));
        return result.Result is Result.Created or Result.Updated;
    }

    public async Task<bool> DeleteUser(long userId)
    {
        var result = await _client.DeleteAsync(new DeleteRequest("users", userId));
        return result.Result is Result.Deleted or Result.NotFound;
    }

    public async Task<IReadOnlyCollection<Search.UserSearchModel>> SearchByUsername(string q)
    {
        var result = await _client.SearchAsync<Search.UserSearchModel>(s =>
            s.Query(q => q.Prefix(c => c
                //.Name("search_by_username")
                .Boost(1.1)
                .Field(p => p.Username)
                .Value(q)
                .Rewrite(MultiTermQueryRewrite.TopTerms(10))
            )).Index("users"));
        return result.Documents;
    }

    public async Task<bool> IndexMessage(MessageSearchModel message)
    {
        var result = await _client.IndexAsync(message, _ => _.Index("messages").Id(message.Id));
        return result.Result is Result.Created or Result.Updated;
    }

    public async Task<bool> DeleteMessage(string id)
    {
        var result = await _client.DeleteAsync(new DeleteRequest("messages", id));
        return result.Result is Result.Deleted or Result.NotFound;
    }

    public async Task<IReadOnlyCollection<MessageSearchModel>> SearchMessages(string q)
    {
        var result = await _client.SearchAsync<MessageSearchModel>(s =>
            s.Query(q => q.Prefix(c => c
                .Boost(1.1)
                .Field(p => p.Message)
                .Value(q)
                .Rewrite(MultiTermQueryRewrite.TopTerms(10))
            )).Index("messages"));
        if (result == null)
        {
            return new List<MessageSearchModel>();
        }
        return result.Documents;
    }
}