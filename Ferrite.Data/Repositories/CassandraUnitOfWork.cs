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

using Ferrite.Utils;

namespace Ferrite.Data.Repositories;

public class CassandraUnitOfWork : IUnitOfWork
{
    private readonly CassandraContext _cassandra;
    private readonly ILogger _log;
    public CassandraUnitOfWork(ILogger log, string cassandraKeyspace, params string[] cassandraHosts)
    {
        _cassandra = new CassandraContext(cassandraKeyspace, cassandraHosts);
        _log = log;
        MessageRepository = new MessageRepository(new CassandraKVStore(_cassandra));
    }
    public IAuthKeyRepository AuthKeyRepository { get; }
    public IAuthorizationRepository AuthorizationRepository { get; }
    public IServerSaltRepository ServerSaltRepository { get; }
    public IMessageRepository MessageRepository { get; }
    public bool Save()
    {
        try
        {
            _cassandra.ExecuteQueue();
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to save changes to Cassandra");
            return false;
        }
        return true;
    }

    public async ValueTask<bool> SaveAsync()
    {
        try
        {
            await _cassandra.ExecuteQueueAsync();
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to save changes to Cassandra");
            return false;
        }
        return true;
    }
}