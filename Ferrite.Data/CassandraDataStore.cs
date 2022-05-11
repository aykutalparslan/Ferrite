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
                "CREATE TABLE IF NOT EXISTS ferrite.authorizations (" +
                            "auth_key_id bigint," +
                            "phone text," +
                            "user_id bigint," +
                            "api_layer int," +
                            "future_auth_token blob," +
                            "logged_in boolean," +
                            "PRIMARY KEY (auth_key_id));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.exported_authorizations (" +
                            "user_id bigint," +
                            "auth_key_id bigint," +
                            "phone text," +
                            "previous_dc_id int," +
                            "next_dc_id int," +
                            "data blob," +
                            "PRIMARY KEY (user_id, auth_key_id));");
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
                "about text," +
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
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.devices (" +
                "auth_key_id bigint," +
                "no_muted boolean," +
                "token_type int," +
                "token text," +
                "app_sandbox boolean," +
                "app_version text," +
                "secret blob," +
                "PRIMARY KEY (auth_key_id, token));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.device_other_users (" +
                "auth_key_id bigint," +
                "user_id bigint," +
                "token text," +
                "PRIMARY KEY (auth_key_id, user_id, token));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.notify_settings (" +
                "auth_key_id bigint," +
                "notify_peer_type int," +
                "peer_type int," +
                "peer_id bigint," +
                "show_previews boolean," +
                "silent boolean," +
                "mute_until int," +
                "sound text," +
                "PRIMARY KEY (auth_key_id, notify_peer_type, peer_type, peer_id));");
            session.Execute(statement.SetKeyspace(keySpace));
            statement = new SimpleStatement(
                "CREATE TABLE IF NOT EXISTS ferrite.report_reasons (" +
                "peer_id bigint," +
                "peer_type int," +
                "reported_by_user bigint," +
                "report_reason int," +
                "PRIMARY KEY (peer_id, peer_type, reported_by_user));");
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
                "api_layer = ?, future_auth_token = ?, logged_in = ?  WHERE auth_key_id = ?;",
                info.Phone, info.UserId, info.ApiLayer,
                info.FutureAuthToken, info.LoggedIn, info.AuthKeyId).SetKeyspace(keySpace);
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
                statement = statement.SetKeyspace(keySpace);

                var results2 = await session.ExecuteAsync(statement.SetKeyspace(keySpace));
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
                "last_name, username, phone, about) VALUES(?,?,?,?,?,?);",
                user.Id, user.AccessHash, user.FirstName, user.LastName,
                user.Username, user.Phone, user.About).SetKeyspace(keySpace);
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
                "last_name = ?, username = ?, phone = ?, about = ? WHERE user_id = ?;",
                user.AccessHash, user.FirstName, user.LastName,
                user.Username, user.Phone, user.About, user.Id).SetKeyspace(keySpace);
            
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
                    AccessHash = row.GetValue<long>("access_hash"),
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
                "DELETE FROM ferrite.exported_authorizations WHERE user_id = ? AND auth_key_id = ?;",
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
                "previous_dc_id = ?, next_dc_id = ?, data = ?  WHERE user_id = ? AND auth_key_id = ?;",
                info.Phone, info.UserId, info.ApiLayer,
                info.FutureAuthToken, info.LoggedIn, info.AuthKeyId).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<ExportedAuthInfo?> GetExportedAuthorizationAsync(long user_id, long auth_key_id)
        {
            ExportedAuthInfo? info = null;
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.authorizations WHERE user_id = ? AND auth_key_id = ?;",
                user_id, auth_key_id);
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
            var statement = new SimpleStatement(
                "UPDATE ferrite.app_infos SET api_id = ?, device_model = ?, " +
                "system_version = ?, app_version = ?, " +
                "system_lang_code = ?, lang_pack = ?, " +
                "lang_code = ?, ip_address = ? " +
                "WHERE auth_key_id = ?;",
                appInfo.ApiId, appInfo.DeviceModel, appInfo.SystemVersion,
                appInfo.AppVersion, appInfo.SystemLangCode, appInfo.LangPack,
                appInfo.LangCode, appInfo.IP, appInfo.AuthKeyId).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
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

        public async Task<bool> SaveDeviceInfoAsync(DeviceInfo deviceInfo)
        {
            var statement = new SimpleStatement(
                "UPDATE ferrite.devices SET no_muted = ?, token_type = ?, " +
                "token = ?, app_sandbox = ?, " +
                "secret = ? " +
                "WHERE auth_key_id = ?;",
                deviceInfo.NoMuted, deviceInfo.TokenType, deviceInfo.Token,
                deviceInfo.AppSandbox, deviceInfo.Secret, deviceInfo.AuthKeyId).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            foreach (var userId in deviceInfo.OtherUserIds)
            {
                statement = new SimpleStatement(
                    "UPDATE ferrite.device_other_users SET token = ? " +
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
                    Token = row.GetValue<string>("token"),
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
                "DELETE FROM ferrite.devices WHERE auth_key_id = ? AND token = ?;",
                authKeyId, token);
            statement = statement.SetKeyspace(keySpace);
            await session.ExecuteAsync(statement);
            foreach (var userId in otherUserIds)
            {
                statement = new SimpleStatement(
                    "DELETE FROM ferrite.device_other_users WHERE auth_key_id = ? AND user_id = ?, AND token = ?;",
                    authKeyId, userId, token);
                statement = statement.SetKeyspace(keySpace);
            }
            await session.ExecuteAsync(statement);
            return true;
        }

        public async Task<bool> SaveNotifySettingsAsync(long authKeyId, InputNotifyPeer peer, InputPeerNotifySettings settings)
        {
            long peerId = 0;
            if (peer.Peer.InputPeerType == InputPeerType.User)
            {
                peerId = peer.Peer.UserId;
            } else if (peer.Peer.InputPeerType == InputPeerType.Self)
            {
                peerId = peer.Peer.UserId;
            } else if (peer.Peer.InputPeerType == InputPeerType.Chat)
            {
                peerId = peer.Peer.ChatId;
            }
            var statement = new SimpleStatement(
                "UPDATE ferrite.notify_settings SET peer_type = ?, peer_id = ?, " +
                "show_previews = ?, silent = ?, " +
                "mute_until = ?, sound = ? " +
                "WHERE auth_key_id = ? AND notify_peer_type = ?;",
                (int)peer.Peer.InputPeerType, peerId,settings.ShowPreviews, settings.Silent,
                settings.MuteUntil, settings.Sound, authKeyId, (int)peer.NotifyPeerType).SetKeyspace(keySpace);
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

        public async Task<InputPeerNotifySettings?> GetNotifySettingsAsync(long authKeyId, InputNotifyPeer peer)
        {
            long peerId = 0;
            if (peer.Peer.InputPeerType == InputPeerType.User)
            {
                peerId = peer.Peer.UserId;
            } else if (peer.Peer.InputPeerType == InputPeerType.Self)
            {
                peerId = peer.Peer.UserId;
            } else if (peer.Peer.InputPeerType == InputPeerType.Chat)
            {
                peerId = peer.Peer.ChatId;
            }
            InputPeerNotifySettings? settings = null;
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.app_infos WHERE auth_key_id = ? AND notify_peer_type = ? " +
                "AND peer_type = ? AND peer_id = ?;", authKeyId, (int)peer.NotifyPeerType, 
                (int)peer.Peer.InputPeerType, peerId);
            statement = statement.SetKeyspace(keySpace);

            var results = await session.ExecuteAsync(statement);
            foreach (var row in results)
            {
                settings = new InputPeerNotifySettings()
                {
                    Silent = row.GetValue<bool>("silent"),
                    Sound = row.GetValue<string>("sound"),
                    MuteUntil = row.GetValue<int>("mute_until"),
                    ShowPreviews = row.GetValue<bool>("show_previews")
                };
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
    }
}

