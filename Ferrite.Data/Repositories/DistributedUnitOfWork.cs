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

public class DistributedUnitOfWork : IUnitOfWork
{
    private readonly CassandraContext _cassandra;
    private readonly ILogger _log;
    public DistributedUnitOfWork(ILogger log, string redisConfig, string cassandraKeyspace, params string[] cassandraHosts)
    {
        _cassandra = new CassandraContext(cassandraKeyspace, cassandraHosts);
        _log = log;
        AuthKeyRepository = new AuthKeyRepository(new CassandraKVStore(_cassandra), new RedisDataStore(redisConfig));
        AuthorizationRepository =
            new AuthorizationRepository(new CassandraKVStore(_cassandra), new CassandraKVStore(_cassandra));
        TempAuthKeyRepository = new TempAuthKeyRepository(new RedisDataStore(redisConfig));
        BoundAuthKeyRepository = new BoundAuthKeyRepository(new RedisDataStore(redisConfig),
            new RedisDataStore(redisConfig), new RedisDataStore(redisConfig));
        MessageRepository = new MessageRepository(new CassandraKVStore(_cassandra));
        UserStatusRepository = new UserStatusRepository(new CassandraKVStore(_cassandra));
        SessionRepository = new SessionRepository(new RedisDataStore(redisConfig));
        AuthSessionRepository = new AuthSessionRepository(new RedisDataStore(redisConfig));
        PhoneCodeRepository = new PhoneCodeRepository(new RedisDataStore(redisConfig));
        SignInRepository = new SignInRepository(new RedisDataStore(redisConfig));
        ServerSaltRepository =
            new ServerSaltRepository(new RedisDataStore(redisConfig), new RedisDataStore(redisConfig));
        LoginTokenRepository = new LoginTokenRepository(new RedisDataStore(redisConfig));
        DeviceLockedRepository = new DeviceLockedRepository(new RedisDataStore(redisConfig));
        UserRepository = new UserRepository(new CassandraKVStore(_cassandra), new CassandraKVStore(_cassandra));
        AppInfoRepository = new AppInfoRepository(new CassandraKVStore(_cassandra));
        DeviceInfoRepository =
            new DeviceInfoRepository(new CassandraKVStore(_cassandra), new CassandraKVStore(_cassandra));
        NotifySettingsRepository =
            new NotifySettingsRepository(new CassandraKVStore(_cassandra));
        ReportReasonRepository = new ReportReasonRepository(new CassandraKVStore(_cassandra));
        PrivacyRulesRepository = new PrivacyRulesRepository(new CassandraKVStore(_cassandra));
        ChatRepository = new ChatRepository(new CassandraKVStore(_cassandra));
        ContactsRepository = new ContactsRepository(new CassandraKVStore(_cassandra));
        BlockedPeersRepository = new BlockedPeersRepository(new CassandraKVStore(_cassandra));
        FileInfoRepository = new FileInfoRepository(new CassandraKVStore(_cassandra),
            new CassandraKVStore(_cassandra), new CassandraKVStore(_cassandra),
            new CassandraKVStore(_cassandra), new CassandraKVStore(_cassandra));
        PhotoRepository = new PhotoRepository(new CassandraKVStore(_cassandra), new CassandraKVStore(_cassandra));
        LangPackRepository = new LangPackRepository(new CassandraKVStore(_cassandra), new CassandraKVStore(_cassandra));
        SignUpNotificationRepository = new SignUpNotificationRepository(new CassandraKVStore(_cassandra));
    }
    public IAuthKeyRepository AuthKeyRepository { get; }
    public ITempAuthKeyRepository TempAuthKeyRepository { get; }
    public IBoundAuthKeyRepository BoundAuthKeyRepository { get; }
    public IAuthorizationRepository AuthorizationRepository { get; }
    public IServerSaltRepository ServerSaltRepository { get; }
    public IMessageRepository MessageRepository { get; }
    public IUserStatusRepository UserStatusRepository { get; }
    public ISessionRepository SessionRepository { get; }
    public IAuthSessionRepository AuthSessionRepository { get; }
    public IPhoneCodeRepository PhoneCodeRepository { get; }
    public ISignInRepository SignInRepository { get; }
    public ILoginTokenRepository LoginTokenRepository { get; }
    public IDeviceLockedRepository DeviceLockedRepository { get; }
    public IUserRepository UserRepository { get; }
    public IAppInfoRepository AppInfoRepository { get; }
    public IDeviceInfoRepository DeviceInfoRepository { get; }
    public INotifySettingsRepository NotifySettingsRepository { get; }
    public IReportReasonRepository ReportReasonRepository { get; }
    public IPrivacyRulesRepository PrivacyRulesRepository { get; }
    public IChatRepository ChatRepository { get; }
    public IContactsRepository ContactsRepository { get; }
    public IBlockedPeersRepository BlockedPeersRepository { get; }
    public ISignUpNotificationRepository SignUpNotificationRepository { get; }
    public IFileInfoRepository FileInfoRepository { get; }
    public IPhotoRepository PhotoRepository { get; }
    public ILangPackRepository LangPackRepository { get; }

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