//
//    Project Ferrite is an Implementation Telegram Server API
//    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU Affero General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
using System;
using Cassandra;

namespace Ferrite.Data
{
    public class CassandraDataStore :IPersistentStore
    {
        // Configure the builder with your cluster's contact points
        private readonly Cluster cluster;
        private readonly ISession session;
        private readonly string keySpace;

        public CassandraDataStore(string keyspace, params string[] hosts)
        {
            cluster = Cluster.Builder()
                             .AddContactPoints(hosts)
                             .Build();

            keySpace = keyspace;
            session = cluster.Connect();
            CreateSchema();
        }

        private void CreateSchema()
        {
            Dictionary<string, string> replication = new Dictionary<string, string>();
            replication.Add("class", "SimpleStrategy");
            replication.Add("replication_factor", "1");
            session.CreateKeyspaceIfNotExists(keySpace, replication);
            var statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.auth_keys (" +
                            "auth_key_id bigint," +
                            "auth_key blob," +
                            "PRIMARY KEY (auth_key_id));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.server_salts (" +
                            "auth_key_id bigint," +
                            "server_salt bigint," +
                            "valid_since timestamp," +
                            "PRIMARY KEY (auth_key_id, valid_since)) WITH CLUSTERING ORDER BY (valid_since ASC));");
            session.Execute(statement.SetKeyspace(keySpace));
        }

        public async Task<byte[]?> GetAuthKeyAsync(long authKeyId)
        {
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.auth_keys WHERE auth_key_id = ?;",
                authKeyId);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement.SetKeyspace(keySpace));
            foreach (var row in results)
            {
                var authKey = row.GetValue<byte[]>("auth_key");
                return authKey;
            }

            return null;
        }

        public async Task SaveAuthKeyAsync(long authKeyId, byte[] authKey)
        {
            var statement = new SimpleStatement(
                "INSERT INTO ferrite.auth_keys(auth_key_id, auth_key) VALUES(?,?);",
                authKeyId, authKey).SetKeyspace(keySpace);

            await session.ExecuteAsync(statement);
        }

        public async Task SaveServerSaltAsync(long authKeyId, long serverSalt, long validSince, int TTL)
        {
            var statement = new SimpleStatement(
                $"INSERT INTO ferrite.server_salts(auth_key_id, server_salt, valid_since) VALUES(?,?,?) USING TTL ?;",
                authKeyId, serverSalt, validSince, TTL).SetKeyspace(keySpace);

            await session.ExecuteAsync(statement);
        }

        public async Task<ICollection<ServerSalt>> GetServerSaltsAsync(long authKeyId, int count)
        {
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.server_salts WHERE auth_key_id = ? LIMIT ?;",
                authKeyId, count);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement.SetKeyspace(keySpace));
            List<ServerSalt> serverSalts = new();
            foreach (var row in results)
            {
                var serverSalt = row.GetValue<long>("server_salt");
                var validSince = row.GetValue<long>("valid_since");
                serverSalts.Add(new ServerSalt()
                {
                    Salt = serverSalt,
                    ValidSince = validSince
                });
            }
            return serverSalts;
        }
    }
}

