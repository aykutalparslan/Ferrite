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
    public class CassandraDataStore : IPersistentStore
    {
        private readonly Cluster cluster;
        private readonly ISession session;
        private readonly string keySpace;
        private IPersistentStore _persistentStoreImplementation;

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
            //statement = new SimpleStatement(
            //    "DROP TABLE IF EXISTS ferrite.authorizations;");
            //session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.authorizations (" +
                            "auth_key_id bigint," +
                            "phone text," +
                            "user_id bigint," +
                            "api_layer int," +
                            "future_auth_token blob," +
                            "logged_in boolean," +
                            "logged_in_at timestamp," +
                            "PRIMARY KEY (auth_key_id));");
            session.Execute(statement.SetKeyspace(keySpace));
            //statement = new SimpleStatement(
            //    "DROP TABLE IF EXISTS ferrite.exported_authorizations;");
            //session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.exported_authorizations (" +
                            "user_id bigint," +
                            "data blob," +
                            "auth_key_id bigint," +
                            "phone text," +
                            "previous_dc_id int," +
                            "next_dc_id int," +
                "PRIMARY KEY (user_id, data));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.server_salts (" +
                            "auth_key_id bigint," +
                            "server_salt bigint," +
                            "valid_since bigint," +
                            "PRIMARY KEY (auth_key_id, valid_since)) WITH CLUSTERING ORDER BY (valid_since ASC);");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "DROP TABLE IF EXISTS ferrite.users;");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.users (" +
                "user_id bigint," +
                "access_hash bigint," +
                "first_name text," +
                "last_name text," +
                "username text," +
                "phone text," +
                "about text," +
                "profile_photo bigint,"+
                "account_days_TTL int," +
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
                            "PRIMARY KEY (phone, user_id));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.users_by_username (" +
                            "username text," +
                            "user_id bigint," +
                            "PRIMARY KEY (username, user_id));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.authorizations_by_phone (" +
                            "phone text," +
                            "auth_key_id bigint," +
                            "PRIMARY KEY (phone, auth_key_id));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.app_infos (" +
                "auth_key_id bigint," +
                "hash bigint," +
                "api_id int," +
                "device_model text," +
                "system_version text," +
                "app_version text," +
                "system_lang_code text," +
                "lang_pack text," +
                "lang_code text," +
                "ip_address text," +
                "PRIMARY KEY (auth_key_id));");
            session.Execute(statement.SetKeyspace(keySpace));
            //statement = new SimpleStatement(
            //  "DROP TABLE IF EXISTS ferrite.app_infos_by_hash;");
            //session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.app_infos_by_hash (" +
                "hash bigint," +
                "auth_key_id bigint," +
                "PRIMARY KEY (hash));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.devices (" +
                "auth_key_id bigint," +
                "no_muted boolean," +
                "token_type int," +
                "app_token text," +
                "app_sandbox boolean," +
                "app_version text," +
                "secret blob," +
                "PRIMARY KEY (auth_key_id, app_token));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.device_other_users (" +
                "auth_key_id bigint," +
                "user_id bigint," +
                "app_token text," +
                "PRIMARY KEY (auth_key_id, user_id, app_token));");
            session.Execute(statement.SetKeyspace(keySpace));
            //statement = new SimpleStatement(
            //  "DROP TABLE IF EXISTS ferrite.notify_settings;");
            //session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.notify_settings (" +
                "auth_key_id bigint," +
                "notify_peer_type int," +
                "peer_type int," +
                "peer_id bigint," +
                "device_type int," +
                "show_previews boolean," +
                "silent boolean," +
                "mute_until int," +
                "sound_type int," +
                "sound_title text," +
                "sound_data text," +
                "sound_id bigint," +
                "PRIMARY KEY (auth_key_id, notify_peer_type, peer_type, peer_id, device_type));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.report_reasons (" +
                "peer_id bigint," +
                "peer_type int," +
                "reported_by_user bigint," +
                "report_reason int," +
                "PRIMARY KEY (peer_id, peer_type, reported_by_user));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.privacy_rules (" +
                "user_id bigint," +
                "privacy_key int," +
                "rule_type int," +
                "peer_ids set<bigint>," +
                "PRIMARY KEY (user_id, privacy_key, rule_type));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.contacts (" +
                "user_id bigint," +
                "contact_user_id bigint," +
                "client_id bigint," +
                "firstname text," +
                "lastname text," +
                "added_on timestamp," +
                "PRIMARY KEY (user_id, contact_user_id));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.blocked_peers (" +
                "user_id bigint," +
                "peer_type int," +
                "peer_id bigint, " +
                "blocked_on timestamp," +
                "PRIMARY KEY (user_id, peer_type, peer_id));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.files (" +
                "file_id bigint," +
                "access_hash bigint," +
                "part_size int," +
                "parts int, " +
                "file_name text," +
                "md5_checksum text," +
                "saved_on timestamp," +
                "PRIMARY KEY (file_id));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.file_parts (" +
                "file_id bigint," +
                "part_num int," +
                "part_size int, " +
                "saved_on timestamp," +
                "PRIMARY KEY (file_id, part_num))" +
                "WITH CLUSTERING ORDER BY (part_num ASC);");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.big_files (" +
                "file_id bigint," +
                "access_hash bigint," +
                "part_size int," +
                "parts int, " +
                "file_name text," +
                "saved_on timestamp," +
                "PRIMARY KEY (file_id));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.big_file_parts (" +
                "file_id bigint," +
                "part_num int," +
                "part_size int, " +
                "saved_on timestamp," +
                "PRIMARY KEY (file_id, part_num))" +
                "WITH CLUSTERING ORDER BY (part_num ASC);");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.file_references (" +
                "file_reference blob," +
                "file_id bigint," +
                "is_big_file boolean, " +
                "PRIMARY KEY (file_reference));");
            session.Execute(statement.SetKeyspace(keySpace));
            //statement = new SimpleStatement(
            //    "DROP TABLE IF EXISTS ferrite.profile_photos;");
            //session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.profile_photos (" +
                "user_id blob," +
                "file_id bigint," +
                "file_reference blob," +
                "access_hash bigint," +
                //"added_on timestamp," +
                "PRIMARY KEY (user_id, file_id));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.thumbnails (" +
                "file_id bigint," +
                "thumb_file_id bigint," +
                "thumb_type text," +
                "thumb_size int," +
                "width int," +
                "height int," +
                "bytes blob," +
                "sizes set<int>," +
                "PRIMARY KEY (file_id, thumb_file_id, thumb_type));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.signup_notifications (" +
                "user_id bigint," +
                "silent boolean," +
                "PRIMARY KEY (user_id));");
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

        public byte[]? GetAuthKey(long authKeyId)
        {
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.auth_keys WHERE auth_key_id = ?;",
                authKeyId);
            statement = statement.SetKeyspace(keySpace);

            var results = session.Execute(statement.SetKeyspace(keySpace));
            foreach (var row in results)
            {
                var authKey = row.GetValue<byte[]>("auth_key");
                return authKey;
            }

            return null;
        }

        public async Task<bool> SaveAuthKeyAsync(long authKeyId, byte[] authKey)
        {
            var statement = new SimpleStatement(
                "INSERT INTO ferrite.auth_keys(auth_key_id, auth_key) VALUES(?,?);",
                authKeyId, authKey).SetKeyspace(keySpace);

            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<bool> SaveServerSaltAsync(long authKeyId, long serverSalt, long validSince, int TTL)
        {
            var statement = new SimpleStatement(
                "INSERT INTO ferrite.server_salts(auth_key_id, server_salt, valid_since) VALUES(?,?,?) USING TTL ?;",
                authKeyId, serverSalt, validSince, TTL).SetKeyspace(keySpace);

            var set = await session.ExecuteAsync(statement);
            return true;
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

        public async Task<bool> SaveAuthorizationAsync(AuthInfo info)
        {
            var oldAuth = await GetAuthorizationAsync(info.AuthKeyId);
            var statement = new SimpleStatement(
                "UPDATE ferrite.authorizations SET phone = ?, user_id = ?, " +
                "api_layer = ?, future_auth_token = ?, logged_in = ?, logged_in_at = ?  WHERE auth_key_id = ?;",
                info.Phone, info.UserId, info.ApiLayer,
                info.FutureAuthToken, info.LoggedIn, DateTime.Now, info.AuthKeyId).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            if((oldAuth?.Phone.Length ?? 0) > 0)
            {
                statement = new SimpleStatement(
                "DELETE FROM ferrite.authorizations_by_phone WHERE phone = ? AND auth_key_id = ?;",
                oldAuth?.Phone, oldAuth?.AuthKeyId);
                statement = statement.SetKeyspace(keySpace);
                await session.ExecuteAsync(statement.SetKeyspace(keySpace));
            }
            if (info.Phone.Length > 0)
            {
                statement = new SimpleStatement(
                "INSERT INTO ferrite.authorizations_by_phone (phone, auth_key_id) VALUES (?,?);",
                info.Phone, info.AuthKeyId).SetKeyspace(keySpace);
                await session.ExecuteAsync(statement);
            }
            return true;
        }

        public async Task<AuthInfo?> GetAuthorizationAsync(long authKeyId)
        {
            AuthInfo? info = null;
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.authorizations WHERE auth_key_id = ?;",
                authKeyId);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement.SetKeyspace(keySpace));
            foreach (var row in results)
            {
                info = new AuthInfo()
                {
                    AuthKeyId = row.GetValue<long>("auth_key_id"),
                    Phone = row.GetValue<string>("phone"),
                    UserId = row.GetValue<long>("user_id"),
                    ApiLayer = row.GetValue<int>("api_layer"),
                    FutureAuthToken = row.GetValue<byte[]>("future_auth_token"),
                    LoggedIn = row.GetValue<bool>("logged_in"),
                    LoggedInAt = row.GetValue<DateTime>("logged_in_at"),
                };
            }
            return info;
        }

        public async Task<ICollection<AuthInfo>> GetAuthorizationsAsync(string phone)
        {
            List<AuthInfo> result = new List<AuthInfo>();
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.authorizations_by_phone WHERE phone = ?;",
                phone);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement.SetKeyspace(keySpace));
            foreach (var row in results)
            {
                var authKeyId = row.GetValue<long>("auth_key_id");
                var statement2 = new SimpleStatement(
                    "SELECT * FROM ferrite.authorizations WHERE auth_key_id = ?;",
                    authKeyId);
                statement2 = statement2.SetKeyspace(keySpace);

                var results2 = await session.ExecuteAsync(statement2);
                foreach (var row2 in results2)
                {
                    AuthInfo info = new AuthInfo()
                    {
                        AuthKeyId = row2.GetValue<long>("auth_key_id"),
                        Phone = row2.GetValue<string>("phone"),
                        UserId = row2.GetValue<long>("user_id"),
                        ApiLayer = row2.GetValue<int>("api_layer"),
                        FutureAuthToken = row2.GetValue<byte[]>("future_auth_token"),
                    };
                    result.Add(info);
                }
            }
            return result;
        }

        public async Task<bool> SaveUserAsync(User user)
        {
            var statement = new SimpleStatement(
                "INSERT INTO ferrite.users(user_id, access_hash, first_name, " +
                "last_name, username, phone, about, profile_photo, account_days_TTL) VALUES(?,?,?,?,?,?,?,?,0);",
                user.Id, user.AccessHash, user.FirstName, user.LastName,
                user.Username, user.Phone, user.About, user.Photo.PhotoId).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            if ((user.Phone?.Length ?? 0) > 0)
            {
                statement = new SimpleStatement(
                "INSERT INTO ferrite.users_by_phone (phone, user_id) VALUES (?,?);",
                user.Phone, user.Id).SetKeyspace(keySpace);
                await session.ExecuteAsync(statement);
            }
            if ((user.Username?.Length ?? 0)> 0)
            {
                statement = new SimpleStatement(
                "INSERT INTO ferrite.users_by_username (username, user_id) VALUES (?,?);",
                user.Username, user.Id).SetKeyspace(keySpace);
                await session.ExecuteAsync(statement);
            }
            return true;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var oldUser = await GetUserAsync(user.Id);
            if ((oldUser?.Phone.Length ?? 0) > 0)
            {
                var stmt = new SimpleStatement(
                "DELETE FROM ferrite.users_by_phone WHERE phone = ? AND user_id = ?;",
                oldUser.Phone, oldUser.Id);
                stmt = stmt.SetKeyspace(keySpace);
                await session.ExecuteAsync(stmt);
            }
            if ((oldUser?.Username?.Length ?? 0) > 0)
            {
                var stmt = new SimpleStatement(
                "DELETE FROM ferrite.users_by_username WHERE username = ? AND user_id = ?;",
                oldUser.Username, oldUser.Id);
                stmt = stmt.SetKeyspace(keySpace);
                await session.ExecuteAsync(stmt);
            }
            var statement = new SimpleStatement(
                "UPDATE ferrite.users SET access_hash = =, first_name = ?, " +
                "last_name = ?, username = ?, phone = ?, about = ?, profile_photo = ? WHERE user_id = ?;",
                user.AccessHash, user.FirstName, user.LastName,
                user.Username, user.Phone, user.About, user.Photo.Empty ? 0 : user.Photo.PhotoId, 
                user.Id).SetKeyspace(keySpace);
            
            await session.ExecuteAsync(statement);
            if ((user.Phone?.Length ?? 0) > 0)
            {
                statement = new SimpleStatement(
                "INSERT INTO ferrite.users_by_phone (phone, user_id) VALUES (?,?);",
                user.Phone, user.Id).SetKeyspace(keySpace);
                await session.ExecuteAsync(statement);
            }
            if ((user.Username?.Length ?? 0) > 0)
            {
                statement = new SimpleStatement(
                "INSERT INTO ferrite.users_by_username (username, user_id) VALUES (?,?);",
                user.Username, user.Id).SetKeyspace(keySpace);
                await session.ExecuteAsync(statement);
            }
            return true;
        }

        public async Task<bool> UpdateUsernameAsync(long userId, string username)
        {
            var oldUser = await GetUserAsync(userId);
            if ((oldUser?.Username?.Length ?? 0) > 0)
            {
                var stmt = new SimpleStatement(
                    "DELETE FROM ferrite.users_by_username WHERE username = ? AND user_id = ?;",
                    oldUser?.Username, oldUser?.Id);
                stmt = stmt.SetKeyspace(keySpace);
                await session.ExecuteAsync(stmt);
            }
            var statement = new SimpleStatement(
                "UPDATE ferrite.users SET username = ? WHERE user_id = ?;",
                username, userId).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            statement = new SimpleStatement(
                    "INSERT INTO ferrite.users_by_username (username, user_id) VALUES (?,?);",
                    username, userId).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<bool> UpdateUserPhoneAsync(long userId, string phone)
        {
            var oldUser = await GetUserAsync(userId);
            if ((oldUser?.Phone.Length ?? 0) > 0)
            {
                var stmt = new SimpleStatement(
                    "DELETE FROM ferrite.users_by_phone WHERE phone = ? AND user_id = ?;",
                    oldUser?.Phone, oldUser?.Id);
                stmt = stmt.SetKeyspace(keySpace);
                await session.ExecuteAsync(stmt);
            }
            var statement = new SimpleStatement(
                "UPDATE ferrite.users SET phone = ? WHERE user_id = ?;",
                phone, userId).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            statement = new SimpleStatement(
                "INSERT INTO ferrite.users_by_phone (username, user_id) VALUES (?,?);",
                phone, userId).SetKeyspace(keySpace);
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
                var photoId = row.GetValue<long>("profile_photo");
                user = new User()
                {
                    Id = row.GetValue<long>("user_id"),
                    AccessHash = row.GetValue<long>("access_hash"),
                    FirstName = row.GetValue<string>("first_name"),
                    LastName = row.GetValue<string>("last_name"),
                    Phone = row.GetValue<string>("phone"),
                    Username = row.GetValue<string>("username"),
                    About = row.GetValue<string>("about"),
                    Photo = new UserProfilePhoto()
                    {
                        DcId = 2,
                        PhotoId = photoId,
                        Empty = photoId == 0
                    }
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
                var photoId = row.GetValue<long>("profile_photo");
                user = new User()
                {
                    Id = row.GetValue<long>("user_id"),
                    AccessHash = row.GetValue<long>("access_hash"),
                    FirstName = row.GetValue<string>("first_name"),
                    LastName = row.GetValue<string>("last_name"),
                    Phone = row.GetValue<string>("phone"),
                    Username = row.GetValue<string>("username"),
                    Photo = new UserProfilePhoto()
                    {
                        DcId = 2,
                        PhotoId = photoId,
                        Empty = photoId == 0
                    }
                };
            }
            return user;
        }

        public async Task<long> GetUserIdAsync(string phone)
        {
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.users_by_phone WHERE phone = ?;",
                phone);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement.SetKeyspace(keySpace));
            long userId = -1;
            foreach (var row in results)
            {
                return row.GetValue<long>("user_id");
            }

            return 0;
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
                var photoId = row.GetValue<long>("profile_photo");
                user = new User()
                {
                    Id = row.GetValue<long>("user_id"),
                    AccessHash = row.GetValue<long>("access_hash"),
                    FirstName = row.GetValue<string>("first_name"),
                    LastName = row.GetValue<string>("last_name"),
                    Phone = row.GetValue<string>("phone"),
                    Username = row.GetValue<string>("username"),
                    Photo = new UserProfilePhoto()
                    {
                        DcId = 2,
                        PhotoId = photoId,
                        Empty = photoId == 0
                    }
                };
            }
            return user;
        }

        public async Task<bool> DeleteUserAsync(User user)
        {
            if (user.Phone.Length  > 0)
            {
                var stmt = new SimpleStatement(
                    "DELETE FROM ferrite.users_by_phone WHERE phone = ? AND user_id = ?;",
                    user.Phone, user.Id);
                stmt = stmt.SetKeyspace(keySpace);
                await session.ExecuteAsync(stmt);
            }
            if (user.Username?.Length > 0)
            {
                var stmt = new SimpleStatement(
                    "DELETE FROM ferrite.users_by_username WHERE username = ? AND user_id = ?;",
                    user.Username, user.Id);
                stmt = stmt.SetKeyspace(keySpace);
                await session.ExecuteAsync(stmt);
            }
            var statement = new SimpleStatement(
                "DELETE FROM ferrite.users WHERE user_id = ?;",
                user.Id);
            statement = statement.SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteAuthKeyAsync(long authKeyId)
        {
            var statement = new SimpleStatement(
                "DELETE FROM ferrite.auth_keys WHERE auth_key_id = ?;",
                authKeyId);
            statement = statement.SetKeyspace(keySpace);
            await session.ExecuteAsync(statement.SetKeyspace(keySpace));
            var oldAuthorization = await GetAuthorizationAsync(authKeyId);
            statement = new SimpleStatement(
                "DELETE FROM ferrite.authorizations WHERE auth_key_id = ?;",
                authKeyId);
            statement = statement.SetKeyspace(keySpace);
            await session.ExecuteAsync(statement.SetKeyspace(keySpace));
            if((oldAuthorization?.Phone.Length ?? 0) > 0)
            {
                statement = new SimpleStatement(
                "DELETE FROM ferrite.authorizations_by_phone WHERE phone = ? AND auth_key_id = ?;",
                oldAuthorization?.Phone, authKeyId);
                statement = statement.SetKeyspace(keySpace);
                await session.ExecuteAsync(statement.SetKeyspace(keySpace));
            }
            
            return true;
        }

        public async Task<bool> DeleteAuthorizationAsync(long authKeyId)
        {
            var oldAuthorization = await GetAuthorizationAsync(authKeyId);
            var statement = new SimpleStatement(
                "DELETE FROM ferrite.authorizations WHERE auth_key_id = ?;",
                authKeyId);
            statement = statement.SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            if (oldAuthorization != null)
            {
                statement = new SimpleStatement(
                "DELETE FROM ferrite.exported_authorizations WHERE user_id = ?;",
                oldAuthorization.UserId, authKeyId);
                statement = statement.SetKeyspace(keySpace);
                await session.ExecuteAsync(statement);
            }
            if ((oldAuthorization?.Phone.Length ?? 0) > 0)
            {
                statement = new SimpleStatement(
                "DELETE FROM ferrite.authorizations_by_phone WHERE phone = ? AND auth_key_id = ?;",
                oldAuthorization?.Phone, authKeyId);
                statement = statement.SetKeyspace(keySpace);
                await session.ExecuteAsync(statement);
            }
            return true;
        }

        public async Task<bool> SaveExportedAuthorizationAsync(AuthInfo info, int previousDc, int nextDc, byte[] data)
        {
            var statement = new SimpleStatement(
                "UPDATE ferrite.exported_authorizations SET phone = ?, " +
                "previous_dc_id = ?, next_dc_id = ?, data = ?  WHERE user_id = ? AND data = ?;",
                info.Phone, previousDc, nextDc, info.UserId, data).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<ExportedAuthInfo?> GetExportedAuthorizationAsync(long user_id, byte[] data)
        {
            ExportedAuthInfo? info = null;
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.exported_authorizations WHERE user_id = ? AND data = ?;",
                user_id, data);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement);
            foreach (var row in results)
            {
                info = new ExportedAuthInfo()
                {
                    AuthKeyId = row.GetValue<long>("auth_key_id"),
                    Phone = row.GetValue<string>("phone"),
                    UserId = row.GetValue<long>("user_id"),
                    PreviousDcId = row.GetValue<int>("previous_dc_id"),
                    NextDcId = row.GetValue<int>("next_dc_id"),
                    Data = row.GetValue<byte[]>("data"),
                };
            }
            return info;
        }

        public async Task<bool> SaveAppInfoAsync(AppInfo appInfo)
        {
            BatchStatement batchStatement = new BatchStatement();
            var statement = new SimpleStatement(
                "UPDATE ferrite.app_infos SET hash = ?, api_id = ?, device_model = ?, " +
                "system_version = ?, app_version = ?, " +
                "system_lang_code = ?, lang_pack = ?, " +
                "lang_code = ?, ip_address = ? " +
                "WHERE auth_key_id = ?;",
                appInfo.Hash, appInfo.ApiId, appInfo.DeviceModel, appInfo.SystemVersion,
                appInfo.AppVersion, appInfo.SystemLangCode, appInfo.LangPack,
                appInfo.LangCode, appInfo.IP, appInfo.AuthKeyId).SetKeyspace(keySpace);
            batchStatement = batchStatement.Add(statement);
            var statement2 = new SimpleStatement(
                "UPDATE ferrite.app_infos_by_hash SET auth_key_id = ? " +
                "WHERE hash = ?;",
                appInfo.AuthKeyId, appInfo.Hash).SetKeyspace(keySpace);
            batchStatement = batchStatement.Add(statement2);
            var result = await session.ExecuteAsync(batchStatement);
            return true;
        }
        
        public async Task<AppInfo?> GetAppInfoAsync(long authKeyId)
        {
            AppInfo? info = null;
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.app_infos WHERE auth_key_id = ?;", authKeyId);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement);
            foreach (var row in results)
            {
                info = new AppInfo()
                {
                    Hash = row.GetValue<long>("hash"),
                    AuthKeyId = row.GetValue<long>("auth_key_id"),
                    ApiId = row.GetValue<int>("api_id"),
                    DeviceModel = row.GetValue<string>("device_model"),
                    SystemVersion = row.GetValue<string>("system_version"),
                    AppVersion = row.GetValue<string>("app_version"),
                    SystemLangCode = row.GetValue<string>("system_lang_code"),
                    LangPack = row.GetValue<string>("lang_pack"),
                    LangCode = row.GetValue<string>("lang_code"),
                    IP = row.GetValue<string>("ip_address"),
                };
            }
            return info;
        }

        public async Task<long?> GetAuthKeyIdByAppHashAsync(long hash)
        {
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.app_infos_by_hash WHERE hash = ?;", hash);
            statement = statement.SetKeyspace(keySpace);
            var results = await session.ExecuteAsync(statement);
            foreach (var row in results)
            {
                return row.GetValue<long>("auth_key_id");
            }

            return null;
        }

        public async Task<bool> SaveDeviceInfoAsync(DeviceInfo deviceInfo)
        {
            var statement = new SimpleStatement(
                "UPDATE ferrite.devices SET no_muted = ?, token_type = ?, " +
                "app_sandbox = ?, secret = ? " +
                "WHERE auth_key_id = ? AND app_token = ?;",
                deviceInfo.NoMuted, deviceInfo.TokenType,
                deviceInfo.AppSandbox, deviceInfo.Secret, deviceInfo.AuthKeyId, deviceInfo.Token).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            foreach (var userId in deviceInfo.OtherUserIds)
            {
                statement = new SimpleStatement(
                    "UPDATE ferrite.device_other_users SET app_token = ? " +
                    "WHERE auth_key_id = ? AND user_id = ?;",
                    deviceInfo.Token, deviceInfo.AuthKeyId, userId).SetKeyspace(keySpace);
                await session.ExecuteAsync(statement);
            }
            return true;
        }

        public async Task<DeviceInfo?> GetDeviceInfoAsync(long authKeyId)
        {
            DeviceInfo? info = null;
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.devices WHERE auth_key_id = ?;", authKeyId);
            statement = statement.SetKeyspace(keySpace);
            var results = await session.ExecuteAsync(statement);
            foreach (var row in results)
            {
                var statement2 = new SimpleStatement(
                    "SELECT * FROM ferrite.device_other_users WHERE auth_key_id = ?;", authKeyId);
                statement2 = statement2.SetKeyspace(keySpace);
                var results2 = await session.ExecuteAsync(statement2);
                List<long> userIds = new();
                foreach (var row2 in results2)
                {
                    userIds.Add(row2.GetValue<long>("user_id"));
                }
                info = new DeviceInfo()
                {
                    AuthKeyId = row.GetValue<long>("auth_key_id"),
                    TokenType = row.GetValue<int>("token_type"),
                    Token = row.GetValue<string>("app_token"),
                    NoMuted = row.GetValue<bool>("no_muted"),
                    Secret = row.GetValue<byte[]>("secret"),
                    AppSandbox = row.GetValue<bool>("app_sandbox"),
                    OtherUserIds = userIds,
                };
            }
            return info;
        }

        public async Task<bool> DeleteDeviceInfoAsync(long authKeyId, string token, ICollection<long> otherUserIds)
        {
            var statement = new SimpleStatement(
                "DELETE FROM ferrite.devices WHERE auth_key_id = ? AND app_token = ?;",
                authKeyId, token);
            statement = statement.SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            foreach (var userId in otherUserIds)
            {
                statement = new SimpleStatement(
                    "DELETE FROM ferrite.device_other_users WHERE auth_key_id = ? AND user_id = ?, AND app_token = ?;",
                    authKeyId, userId, token);
                statement = statement.SetKeyspace(keySpace);
            }
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<bool> SaveNotifySettingsAsync(long authKeyId, InputNotifyPeer peer, PeerNotifySettings settings)
        {
            long peerId = 0;
            int peerType = 0;
            if (peer.NotifyPeerType == InputNotifyPeerType.Peer)
            {
                peerType = (int)peer.Peer.InputPeerType;
                if (peer.Peer.InputPeerType == InputPeerType.User)
                {
                    peerId = peer.Peer.UserId;
                }
                else if (peer.Peer.InputPeerType == InputPeerType.UserFromMessage)
                {
                    peerId = peer.Peer.UserId;
                }
                else if (peer.Peer.InputPeerType == InputPeerType.Chat)
                {
                    peerId = peer.Peer.ChatId;
                }
                else if (peer.Peer.InputPeerType == InputPeerType.Channel)
                {
                    peerId = peer.Peer.ChannelId;
                }
                else if (peer.Peer.InputPeerType == InputPeerType.ChannelFromMessage)
                {
                    peerId = peer.Peer.ChannelId;
                }
            }
            
            var statement = new SimpleStatement(
                "UPDATE ferrite.notify_settings SET show_previews = ?, silent = ?, " +
                "mute_until = ?, sound_type = ?, sound_title = ?, sound_data = ?, sound_id = ? " +
                "WHERE auth_key_id = ? AND notify_peer_type = ? AND peer_type = ? AND peer_id = ? AND device_type = ?;",
                settings.ShowPreviews, settings.Silent, settings.MuteUntil,
                (int)settings.NotifySoundType, settings.Title,settings.Data, settings.Id,
                    authKeyId, (int)peer.NotifyPeerType, peerType, peerId, (int)settings.DeviceType).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<bool> SavePeerReportReasonAsync(long reportedByUser, InputPeer peer, ReportReason reason)
        {
            long peerId = 0;
            if (peer.InputPeerType == InputPeerType.User)
            {
                peerId = peer.UserId;
            } else if (peer.InputPeerType == InputPeerType.Chat)
            {
                peerId = peer.ChatId;
            } else if (peer.InputPeerType == InputPeerType.Channel)
            {
                peerId = peer.ChannelId;
            } else if (peer.InputPeerType == InputPeerType.UserFromMessage)
            {
                peerId = peer.UserId;
            } else if (peer.InputPeerType == InputPeerType.ChannelFromMessage)
            {
                peerId = peer.ChannelId;
            }
            var statement = new SimpleStatement(
                "UPDATE ferrite.report_reasons SET report_reason = ? " +
                "WHERE peer_id = ? AND peer_type = ? AND reported_by_user = ?;",
                (int)reason, peerId, (int)peer.InputPeerType, reportedByUser).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<IReadOnlyCollection<PeerNotifySettings>> GetNotifySettingsAsync(long authKeyId, InputNotifyPeer peer)
        {
            long peerId = 0;
            int peerType = 0;
            if (peer.NotifyPeerType == InputNotifyPeerType.Peer)
            {
                peerType = (int)peer.Peer.InputPeerType;
                if (peer.Peer.InputPeerType == InputPeerType.User)
                {
                    peerId = peer.Peer.UserId;
                }
                else if (peer.Peer.InputPeerType == InputPeerType.UserFromMessage)
                {
                    peerId = peer.Peer.UserId;
                }
                else if (peer.Peer.InputPeerType == InputPeerType.Chat)
                {
                    peerId = peer.Peer.ChatId;
                }
                else if (peer.Peer.InputPeerType == InputPeerType.Channel)
                {
                    peerId = peer.Peer.ChannelId;
                }
                else if (peer.Peer.InputPeerType == InputPeerType.ChannelFromMessage)
                {
                    peerId = peer.Peer.ChannelId;
                }
            }
            List<PeerNotifySettings> settings = new List<PeerNotifySettings>();
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.notify_settings WHERE auth_key_id = ? AND notify_peer_type = ? " +
                "AND peer_type = ? AND peer_id = ?;", authKeyId, (int)peer.NotifyPeerType, 
                peerType, peerId);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement);
            foreach (var row in results)
            {
                var notifySettings = new PeerNotifySettings()
                {
                    DeviceType = (DeviceType)row.GetValue<int>("device_type"),
                    NotifySoundType = (NotifySoundType)row.GetValue<int>("sound_type"),
                    Silent = row.GetValue<bool>("silent"),
                    Title = row.GetValue<string>("sound") ?? "",
                    Data = row.GetValue<string>("sound") ?? "",
                    Id = row.GetValue<long>("sound_id"),
                    MuteUntil = row.GetValue<int>("mute_until"),
                    ShowPreviews = row.GetValue<bool>("show_previews"),
                };
                settings.Add(notifySettings);
            }
            return settings;
        }

        public async Task<bool> DeleteNotifySettingsAsync(long authKeyId)
        {
            var statement = new SimpleStatement(
                "DELETE FROM ferrite.notify_settings WHERE auth_key_id = ?;", authKeyId).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<bool> SavePrivacyRulesAsync(long userId, InputPrivacyKey key, ICollection<PrivacyRule> rules)
        {
            foreach (var rule in rules)
            {
                var statement = new SimpleStatement(
                    "UPDATE ferrite.privacy_rules SET peer_ids = ? " +
                    "WHERE user_id = ? AND privacy_key = ? AND rule_type = ?;",
                    rule.Peers, userId, (int)key, (int)rule.PrivacyRuleType).SetKeyspace(keySpace);
                await session.ExecuteAsync(statement);
            }
            return true;
        }

        public async Task<bool> DeletePrivacyRulesAsync(long userId)
        {
            var statement = new SimpleStatement(
                "DELETE FROM ferrite.privacy_rules " +
                "WHERE user_id = ?;",
                userId).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<ICollection<PrivacyRule>> GetPrivacyRulesAsync(long userId, InputPrivacyKey key)
        {
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.privacy_rules WHERE user_id = ? AND privacy_key = ?;", 
                userId, (int)key);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement);
            List<PrivacyRule> result = new();
            foreach (var row in results)
            {
                result.Add(new PrivacyRule()
                {
                    PrivacyRuleType = (PrivacyRuleType)row.GetValue<int>("rule_type"),
                    Peers = row.GetValue<List<long>>("peer_ids"),
                });
            }
            return result;
        }

        public async Task<bool> SaveChatAsync(Chat chat)
        {
            throw new NotImplementedException();
        }

        public async Task<Chat?> GetChatAsync(long chatId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateAccountTTLAsync(long userId, int accountDaysTTL)
        {
            var statement = new SimpleStatement(
                "UPDATE ferrite.users SET account_days_TTL = ? WHERE user_id = ?;",
                accountDaysTTL, userId).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<int> GetAccountTTLAsync(long userId)
        {
            var statement = new SimpleStatement(
                "SELECT account_days_TTL FROM ferrite.users WHERE user_id = ?;", 
                userId);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement);
            foreach (var row in results)
            {
                return row.GetValue<int>(0);
            }

            return 0;
        }

        public async Task<ImportedContact?> SaveContactAsync(long userId, InputContact contact)
        {
            var contactUser = await GetUserAsync(contact.Phone);
            var statement = new SimpleStatement(
                "UPDATE ferrite.contacts SET client_id = ?, firstname = ?, lastname = ?, added_on = ? " +
                "WHERE user_id = ? AND contact_user_id = ?;",
                contact.ClientId, contact.FirstName, contact.LastName, DateTime.Now, userId, contactUser.Id).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);

            return new ImportedContact(contactUser.Id, contact.ClientId);
        }

        public async Task<bool> DeleteContactAsync(long userId, long contactUserId)
        {
            var statement = new SimpleStatement(
                "DELETE ferrite.contacts WHERE user_id = ? AND contact_user_id = ?;",
                userId, contactUserId).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<bool> DeleteContactsAsync(long userId)
        {
            var statement = new SimpleStatement(
                "DELETE ferrite.contacts WHERE user_id = ?;",
                userId).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<ICollection<SavedContact>> GetSavedContactsAsync(long userId)
        {
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.contacts WHERE user_id = ?;", 
                userId);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement);
            List<SavedContact> result = new();
            foreach (var row in results)
            {
                var contactUserId = row.GetValue<long>("contact_user_id");
                var contactUser = await GetUserAsync(contactUserId);
                var added = ((DateTimeOffset)row.GetValue<DateTime>("added_on")).ToUnixTimeSeconds();
                result.Add(new SavedContact(contactUser.Phone,
                    row.GetValue<string>("firstname"),
                    row.GetValue<string>("lastname"), 
                    (int)added));
            }
            return result;
        }

        public async Task<ICollection<Contact>> GetContactsAsync(long userId)
        {
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.contacts WHERE user_id = ?;", 
                userId);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement);
            List<Contact> result = new();
            foreach (var row in results)
            {
                var contactUserId = row.GetValue<long>("contact_user_id");
                var statement2 = new SimpleStatement(
                    "SELECT * FROM ferrite.contacts WHERE user_id = ? AND contact_user_id = ?;", 
                    contactUserId, userId);
                statement = statement.SetKeyspace(keySpace);
                var resutlsInner = await session.ExecuteAsync(statement2);
                bool mutual = false;
                foreach (var rowInner in resutlsInner)
                {
                    mutual = true;
                    break;
                }
                result.Add(new Contact(contactUserId, mutual));
            }
            return result;
        }

        public async Task<bool> SaveBlockedUserAsync(long userId, long peerId, PeerType peerType)
        {
            var statement = new SimpleStatement(
                "INSERT INTO ferrite.blocked_peers (user_id, peer_type, peer_id, blocked_on) " +
                "VALUES (?,?,?,?) IF NOT EXISTS;",
                userId, (int)peerType, peerId, DateTime.Now) .SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<bool> DeleteBlockedUserAsync(long userId, long peerId, PeerType peerType)
        {
            var statement = new SimpleStatement(
                "DELETE FROM ferrite.blocked_users WHERE user_id = ? AND peer_type = ? AND peer_id = ?;",
                userId, (int)peerType, peerId).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<ICollection<PeerBlocked>> GetBlockedPeersAsync(long userId)
        {
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.blocked_users WHERE user_id = ?;", 
                userId);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement);
            List<PeerBlocked> result = new();
            foreach (var row in results)
            {
                result.Add(new PeerBlocked(new Peer((PeerType)row.GetValue<int>("peer_type"),
                    row.GetValue<long>("blocked_user_id")), 
                    (int)((DateTimeOffset)row.GetValue<DateTime>("blocked_on")).ToUnixTimeSeconds()));
            }
            return result;
        }

        public async Task<bool> SaveFileInfoAsync(UploadedFileInfo uploadedFile)
        {
            var statement = new SimpleStatement(
                "UPDATE ferrite.files SET part_size = ?, parts = ?, access_hash = ?, " +
                "file_name = ?, md5_checksum = ?, saved_on = ? " +
                "WHERE file_id = ?;",
                uploadedFile.PartSize, uploadedFile.Parts, uploadedFile.AccessHash, 
                uploadedFile.Name, uploadedFile.MD5Checksum, uploadedFile.SavedOn, uploadedFile.Id) .SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<UploadedFileInfo?> GetFileInfoAsync(long fileId)
        {
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.files WHERE file_id = ?;", 
                fileId);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement);
            foreach (var row in results)
            {
                return new UploadedFileInfo(fileId, row.GetValue<int>("part_size"),
                    row.GetValue<int>("parts"), row.GetValue<long>("access_hash"),
                    row.GetValue<string>("file_name"), row.GetValue<string>("md5_checksum"),
                row.GetValue<DateTime>("saved_on"), false);
            }

            return null;
        }

        public async Task<bool> SaveBigFileInfoAsync(UploadedFileInfo uploadedFile)
        {
            var statement = new SimpleStatement(
                "UPDATE ferrite.big_files SET part_size = ?, parts = ?, access_hash = ?, " +
                "file_name = ?, saved_on = ? " +
                "WHERE file_id = ?;",
                uploadedFile.PartSize, uploadedFile.Parts, uploadedFile.AccessHash, 
                uploadedFile.Name, uploadedFile.SavedOn ,uploadedFile.Id) .SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<UploadedFileInfo?> GetBigFileInfoAsync(long fileId)
        {
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.big_files WHERE file_id = ?;", 
                fileId);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement);
            foreach (var row in results)
            {
                return new UploadedFileInfo(fileId, row.GetValue<int>("part_size"),
                    row.GetValue<int>("parts"), row.GetValue<long>("access_hash"),
                    row.GetValue<string>("file_name"), null,
                    row.GetValue<DateTime>("saved_on"), true);
            }

            return null;
        }

        public async Task<bool> SaveFilePartAsync(FilePart part)
        {
            var statement = new SimpleStatement(
                "UPDATE ferrite.file_parts SET part_size = ?, saved_on = ? " +
                "WHERE file_id = ? AND part_num = ?;",
                part.PartSize, DateTime.Now, part.FileId, part.PartNum) .SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<IReadOnlyCollection<FilePart>> GetFilePartsAsync(long fileId)
        {
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.file_parts WHERE file_id = ?;", 
                fileId);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement);
            List<FilePart> parts = new();
            foreach (var row in results)
            {
                parts.Add(new FilePart(fileId, row.GetValue<int>("part_num"),
                    row.GetValue<int>("part_size")));
            }

            return parts;
        }

        public async Task<bool> SaveBigFilePartAsync(FilePart part)
        {
            var statement = new SimpleStatement(
                "UPDATE ferrite.big_file_parts SET part_size = ?, saved_on = ? " +
                "WHERE file_id = ? AND part_num = ?;",
                part.PartSize, DateTime.Now, part.FileId, part.PartNum) .SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<IReadOnlyCollection<FilePart>> GetBigFilePartsAsync(long fileId)
        {
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.big_file_parts WHERE file_id = ?;", 
                fileId);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement);
            List<FilePart> parts = new();
            foreach (var row in results)
            {
                parts.Add(new FilePart(fileId, row.GetValue<int>("part_num"),
                    row.GetValue<int>("part_size")));
            }

            return parts;
        }

        public async Task<bool> SaveFileReferenceAsync(FileReference reference)
        {
            var statement = new SimpleStatement(
                "UPDATE ferrite.file_references SET file_id = ?, is_big_file = ? " +
                "WHERE file_reference = ?;",
                reference.FileId, reference.IsBigfile, reference.ReferenceBytes) .SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<FileReference?> GetFileReferenceAsync(byte[] referenceBytes)
        {
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.file_references WHERE file_reference = ?;", 
                referenceBytes);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement);
            foreach (var row in results)
            {
                return new FileReference(row.GetValue<byte[]>("file_reference"),
                    row.GetValue<long>("file_id"), row.GetValue<bool>("is_big_file"));
            }

            return null;
        }

        public async Task<bool> SaveProfilePhotoAsync(long userId, long fileId, long accessHash, 
            byte[] referenceBytes, DateTime date)
        {
            var statement = new SimpleStatement(
                "UPDATE ferrite.profile_photos SET file_reference = ?, access_hash = ? " +
                "WHERE user_id = ? AND file_id = ?;",
                 referenceBytes, accessHash, userId, fileId) .SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            statement = new SimpleStatement(
                "UPDATE ferrite.users SET profile_photo = ? WHERE user_id = ?;",
                fileId, userId).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<bool> DeleteProfilePhotoAsync(long userId, long fileId)
        {
            var statement = new SimpleStatement(
                "DELETE FROM ferrite.profile_photos " +
                "WHERE user_id = ? AND file_id = ?;",
                userId, fileId) .SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            statement = new SimpleStatement(
                "UPDATE ferrite.users SET profile_photo = ? WHERE user_id = ?;",
                0, userId).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<IReadOnlyCollection<Photo>> GetProfilePhotosAsync(long userId)
        {
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.profile_photos WHERE user_id = ?;", 
                userId);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement);
            List<Photo> photos = new();
            foreach (var row in results)
            {
                var fileId = row.GetValue<long>("file_id");
                var statementInner = new SimpleStatement(
                    "SELECT * FROM ferrite.thumbnails WHERE file_id = ?;", 
                    fileId);
                statementInner = statementInner.SetKeyspace(keySpace);

                var results2 = await session.ExecuteAsync(statementInner);
                List<PhotoSize> photoSizes = new List<PhotoSize>();
                foreach (var row2 in results2)
                {
                    photoSizes.Add(new PhotoSize(PhotoSizeType.Default,
                        row2.GetValue<string>("thumb_type"),
                        row2.GetValue<int>("width"),
                        row2.GetValue<int>("height"),
                        row2.GetValue<int>("thumb_size"),
                        row2.GetValue<byte[]>("bytes"),
                        row2.GetValue<List<int>>("sizes")));
                }
                photos.Add(new Photo(false, fileId,
                    row.GetValue<long>("access_hash"),
                    row.GetValue<byte[]>("file_reference"),
                    (int)DateTimeOffset.Now.ToUnixTimeSeconds(),
                    photoSizes, null, 2));
            }

            return photos;
        }

        public async Task<Photo?> GetProfilePhotoAsync(long userId, long fileId)
        {
            Photo? photo = null;
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.profile_photos WHERE user_id = ? AND file_id = ?;", 
                userId, fileId);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement);
            List<Photo> photos = new();
            foreach (var row in results)
            {
                var statementInner = new SimpleStatement(
                    "SELECT * FROM ferrite.thumbnails WHERE file_id = ?;", 
                    fileId);
                statementInner = statementInner.SetKeyspace(keySpace);

                var results2 = await session.ExecuteAsync(statementInner);
                List<PhotoSize> photoSizes = new List<PhotoSize>();
                foreach (var row2 in results2)
                {
                    photoSizes.Add(new PhotoSize(PhotoSizeType.Default,
                        row2.GetValue<string>("thumb_type"),
                        row2.GetValue<int>("width"),
                        row2.GetValue<int>("height"),
                        row2.GetValue<int>("thumb_size"),
                        row2.GetValue<byte[]>("bytes"),
                        row2.GetValue<List<int>>("sizes")));
                }
                photo =new Photo(false, fileId,
                    row.GetValue<long>("access_hash"),
                    row.GetValue<byte[]>("file_reference"),
                    (int)DateTimeOffset.Now.ToUnixTimeSeconds(),
                    photoSizes, null, 2);
            }
            return photo;
        }

        public async Task<bool> SaveThumbnailAsync(Thumbnail thumbnail)
        {
            var statement = new SimpleStatement(
                "UPDATE ferrite.thumbnails SET thumb_size = ?, width = ?, height = ?, " +
                "bytes = ?, sizes = ? "+
                "WHERE file_id = ? AND thumb_file_id = ? AND thumb_type = ?;",
                thumbnail.Size, thumbnail.Width, thumbnail.Height,thumbnail.Bytes, 
                thumbnail.Sizes, thumbnail.FileId, thumbnail.ThumbnailFileId, thumbnail.Type).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<IReadOnlyCollection<Thumbnail>> GetThumbnailsAsync(long photoId)
        {
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.thumbnails WHERE file_id = ?;", 
                photoId);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement);
            List<Thumbnail> thumbs = new();
            foreach (var row in results)
            {
                thumbs.Add(new Thumbnail(photoId, 
                    row.GetValue<long>("thumb_file_id"),
                    row.GetValue<string>("thumb_type"),
                    row.GetValue<int>("thumb_size"),
                    row.GetValue<int>("width"),
                    row.GetValue<int>("height"),
                    row.GetValue<byte[]>("bytes"),
                    row.GetValue<List<int>>("sizes")));
            }

            return thumbs;
        }

        public async Task<bool> SaveSignUoNotificationAsync(long userId, bool silent)
        {
            var statement = new SimpleStatement(
                "UPDATE ferrite.signup_notifications SET silent = ? "+
                "WHERE user_id = ?;",
                silent, userId).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<bool> GetSignUoNotificationAsync(long userId)
        {
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.signup_notifications WHERE user_id = ?;", 
                userId);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement);
            foreach (var row in results)
            {
                return row.GetValue<bool>("silent");
            }

            return false;
        }
    }
}

