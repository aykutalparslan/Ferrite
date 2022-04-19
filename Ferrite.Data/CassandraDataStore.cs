//
//  Project Ferrite is an Implementation of the Telegram Server API
//  Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Affero General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Affero General Public License for more details.
//
//  You should have received a copy of the GNU Affero General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
using System;
using Cassandra;

namespace Ferrite.Data
{
    public class CassandraDataStore :IPersistentStore
    {
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
                            "phone text," +
                            "user_id bigint," +
                            "api_layer int," +
                            "PRIMARY KEY (auth_key_id));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.auth_key_details (" +
                            "auth_key_id bigint," +
                            "phone text," +
                            "user_id bigint," +
                            "api_layer int," +
                            "future_auth_token blob," +
                            "logged_in boolean," +
                            "PRIMARY KEY (auth_key_id));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.server_salts (" +
                            "auth_key_id bigint," +
                            "server_salt bigint," +
                            "valid_since bigint," +
                            "PRIMARY KEY (auth_key_id, valid_since)) WITH CLUSTERING ORDER BY (valid_since ASC);");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.users (" +
                            "user_id bigint," +
                            "access_hash bigint," +
                            "first_name text," +
                            "last_name text," +
                            "username text," +
                            "phone text," +
                            "PRIMARY KEY (user_id));");
            session.Execute(statement.SetKeyspace(keySpace));
            //https://docs.datastax.com/en/cql-oss/3.3/cql/cql_using/useWhenIndex.html
            //If you create an index on a high-cardinality column, which has many
            //distinct values, a query between the fields will incur many seeks
            //for very few results.In the table with a billion songs, looking up
            //songs by writer(a value that is typically unique for each song)
            //instead of by their artist, is likely to be very inefficient.It would
            //probably be more efficient to manually maintain the table as a form of
            //an index instead of using the Cassandra built -in index.
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.users_by_phone (" +
                            "phone text," +
                            "user_id bigint," +
                            "PRIMARY KEY (phone));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.users_by_username (" +
                            "username text," +
                            "user_id bigint," +
                            "PRIMARY KEY (username));");
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
                "INSERT INTO ferrite.server_salts(auth_key_id, server_salt, valid_since) VALUES(?,?,?) USING TTL ?;",
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

        public async Task SaveAuthKeyDetailsAsync(AuthKeyDetails details)
        {
            var statement = new SimpleStatement(
                "UPDATE ferrite.auth_key_details SET phone = ?, user_id = ?, " +
                "api_layer = ?, future_auth_token = ?, logged_in = ?  WHERE auth_key_id = ?;",
                details.Phone, details.UserId, details.ApiLayer,
                details.FutureAuthToken, details.LoggedIn, details.AuthKeyId).SetKeyspace(keySpace);

            await session.ExecuteAsync(statement);
        }

        public async Task<AuthKeyDetails?> GetAuthKeyDetailsAsync(long authKeyId)
        {
            AuthKeyDetails? details = null;
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.auth_key_details WHERE auth_key_id = ?;",
                authKeyId);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement.SetKeyspace(keySpace));
            foreach (var row in results)
            {
                details = new AuthKeyDetails()
                {
                    AuthKeyId = row.GetValue<long>("auth_key_id"),
                    Phone = row.GetValue<string>("phone"),
                    UserId = row.GetValue<long>("user_id"),
                    ApiLayer = row.GetValue<int>("api_layer"),
                    FutureAuthToken = row.GetValue<byte[]>("future_auth_token"),
                };
            }
            return details;
        }

        public async Task<bool> SaveUserAsync(User user)
        {
            var statement = new SimpleStatement(
                "INSERT INTO ferrite.users(user_id, access_hash, first_name, " +
                "last_name, username, phone) VALUES(?,?,?,?,?,?);",
                user.Id, user.AccessHash, user.FirstName, user.LastName,
                user.Username, user.Phone).SetKeyspace(keySpace);

            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var statement = new SimpleStatement(
                "UPDATE ferrite.users SET access_hash = =, first_name = ?, " +
                "last_name = ?, username = ?, phone = ?) WHERE user_id = ?;",
                user.AccessHash, user.FirstName, user.LastName,
                user.Username, user.Phone, user.Id).SetKeyspace(keySpace);

            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<User?> GetUserAsync(long userId)
        {
            User? user = null;
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.users WHERE user_id = ?;",
                userId);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement.SetKeyspace(keySpace));
            foreach (var row in results)
            {
                user = new User()
                {
                    Id = row.GetValue<long>("user_id"),
                    AccessHash = row.GetValue<long>("acces_hash"),
                    FirstName = row.GetValue<string>("first_name"),
                    LastName = row.GetValue<string>("last_name"),
                    Phone = row.GetValue<string>("phone"),
                    Username = row.GetValue<string>("username"),
                };
            }
            return user;
        }

        public async Task<User?> GetUserAsync(string phone)
        {
            User? user = null;
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.users_by_phone WHERE phone = ?;",
                phone);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement.SetKeyspace(keySpace));
            long userId = -1;
            foreach (var row in results)
            {
                userId = row.GetValue<long>("user_id");
            }
            statement = new SimpleStatement(
                "SELECT * FROM ferrite.users WHERE user_id = ?;",
                userId);
            statement = statement.SetKeyspace(keySpace);

            results = await session.ExecuteAsync(statement.SetKeyspace(keySpace));
            foreach (var row in results)
            {
                user = new User()
                {
                    Id = row.GetValue<long>("user_id"),
                    AccessHash = row.GetValue<long>("acces_hash"),
                    FirstName = row.GetValue<string>("first_name"),
                    LastName = row.GetValue<string>("last_name"),
                    Phone = row.GetValue<string>("phone"),
                    Username = row.GetValue<string>("username"),
                };
            }
            return user;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            User? user = null;
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.users_by_username WHERE username = ?;",
                username);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement.SetKeyspace(keySpace));
            long userId = -1;
            foreach (var row in results)
            {
                userId = row.GetValue<long>("user_id");
            }
            statement = new SimpleStatement(
                "SELECT * FROM ferrite.users WHERE user_id = ?;",
                userId);
            statement = statement.SetKeyspace(keySpace);

            results = await session.ExecuteAsync(statement.SetKeyspace(keySpace));
            foreach (var row in results)
            {
                user = new User()
                {
                    Id = row.GetValue<long>("user_id"),
                    AccessHash = row.GetValue<long>("acces_hash"),
                    FirstName = row.GetValue<string>("first_name"),
                    LastName = row.GetValue<string>("last_name"),
                    Phone = row.GetValue<string>("phone"),
                    Username = row.GetValue<string>("username"),
                };
            }
            return user;
        }

        public async Task<bool> DeleteAuthKeyAsync(long authKeyId)
        {
            var statement = new SimpleStatement(
                "DELETE FROM ferrite.auth_key_details WHERE auth_key_id = ?;",
                authKeyId);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement.SetKeyspace(keySpace));
            return true;
        }
    }
}

