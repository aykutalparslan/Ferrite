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

using Cassandra;

namespace Ferrite.Data.Repositories;

public class CassandraContext 
{
    private readonly Cluster _cluster;
    private readonly ISession _session;
    private readonly string _keySpace;
    private readonly Queue<Statement> _executionQueue = new Queue<Statement>();
    private readonly SemaphoreSlim _executionSemaphore = new SemaphoreSlim(1, 1);

    public CassandraContext(string keyspace, params string[] hosts)
    {
        _cluster = Cluster.Builder()
            .AddContactPoints(hosts)
            .Build();

        _keySpace = keyspace;
        _session = _cluster.Connect();
        Dictionary<string, string> replication = new Dictionary<string, string>();
        replication.Add("class", "SimpleStrategy");
        replication.Add("replication_factor", "1");
        _session.CreateKeyspaceIfNotExists(_keySpace, replication);
    }
    
    public void Enqueue(Statement statement)
    {
        _executionSemaphore.Wait();
        _executionQueue.Enqueue(statement);
        _executionSemaphore.Release();
    }
    public RowSet Execute(Statement statement)
    {
        return _session.Execute(statement);
    }
    public async Task<RowSet> ExecuteAsync(Statement statement)
    {
        return await _session.ExecuteAsync(statement);
    }
    public RowSet ExecuteQueue()
    {
        _executionSemaphore.Wait();
        if (_executionQueue.Count == 1)
        {
            var statement = _executionQueue.Dequeue();
            var result = _session.Execute(statement);
            _executionSemaphore.Release();
            return result;
        }
        else
        {
            var batch = new BatchStatement();
            while (_executionQueue.Count > 0)
            {
                var statement = _executionQueue.Dequeue();
                batch = batch.Add(statement);
            }
            var result = _session.Execute(batch.SetKeyspace(_keySpace));
            _executionSemaphore.Release();
            return result;
        }
    }
    public async Task<RowSet> ExecuteQueueAsync()
    {
        await _executionSemaphore.WaitAsync();
        if (_executionQueue.Count == 1)
        {
            var statement = _executionQueue.Dequeue();
            var result = await _session.ExecuteAsync(statement);
            _executionSemaphore.Release();
            return result;
        }
        else
        {
            var batch = new BatchStatement();
            while (_executionQueue.Count > 0)
            {
                var statement = _executionQueue.Dequeue();
                batch = batch.Add(statement);
            }
            var result = await _session.ExecuteAsync(batch.SetKeyspace(_keySpace));
            _executionSemaphore.Release();
            return result;
        }
    }
}