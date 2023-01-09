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

namespace Ferrite.Data.Repositories;

public class LocalUnitOfWork : IUnitOfWork
{
    private readonly RocksDBContext _rocksdb;

    public LocalUnitOfWork(string path = "rocksdb-data")
    {
        _rocksdb = new RocksDBContext(path);
        AuthKeyRepository = new AuthKeyRepository(new RocksDBKVStore(_rocksdb), new InMemoryStore());
        AuthorizationRepository =
            new AuthorizationRepository(new RocksDBKVStore(_rocksdb), new RocksDBKVStore(_rocksdb));
        TempAuthKeyRepository = new TempAuthKeyRepository(new InMemoryStore());
        BoundAuthKeyRepository = new BoundAuthKeyRepository(new InMemoryStore(),
            new InMemoryStore(), new InMemoryStore());
        MessageRepository = new MessageRepository(new RocksDBKVStore(_rocksdb));
        UserStatusRepository = new UserStatusRepository(new RocksDBKVStore(_rocksdb));
        SessionRepository = new SessionRepository(new InMemoryStore(), new InMemoryStore());
        AuthSessionRepository = new AuthSessionRepository(new InMemoryStore());
        PhoneCodeRepository = new PhoneCodeRepository(new InMemoryStore());
        SignInRepository = new SignInRepository(new InMemoryStore());
        ServerSaltRepository =
            new ServerSaltRepository(new InMemoryStore(), new InMemoryStore());
        LoginTokenRepository = new LoginTokenRepository(new InMemoryStore());
        DeviceLockedRepository = new DeviceLockedRepository(new InMemoryStore());
        UserRepository = new UserRepository(new RocksDBKVStore(_rocksdb), new RocksDBKVStore(_rocksdb), new RocksDBKVStore(_rocksdb));
        AppInfoRepository = new AppInfoRepository(new RocksDBKVStore(_rocksdb));
        DeviceInfoRepository =
            new DeviceInfoRepository(new RocksDBKVStore(_rocksdb), new RocksDBKVStore(_rocksdb));
        NotifySettingsRepository =
            new NotifySettingsRepository(new RocksDBKVStore(_rocksdb));
        ReportReasonRepository = new ReportReasonRepository(new RocksDBKVStore(_rocksdb));
        PrivacyRulesRepository = new PrivacyRulesRepository(new RocksDBKVStore(_rocksdb));
        ChatRepository = new ChatRepository(new RocksDBKVStore(_rocksdb));
        ContactsRepository = new ContactsRepository(new RocksDBKVStore(_rocksdb));
        BlockedPeersRepository = new BlockedPeersRepository(new RocksDBKVStore(_rocksdb));
        FileInfoRepository = new FileInfoRepository(new RocksDBKVStore(_rocksdb),
            new RocksDBKVStore(_rocksdb), new RocksDBKVStore(_rocksdb),
            new RocksDBKVStore(_rocksdb), new RocksDBKVStore(_rocksdb));
        PhotoRepository = new PhotoRepository(new RocksDBKVStore(_rocksdb), new RocksDBKVStore(_rocksdb));
        LangPackRepository = new LangPackRepository(new RocksDBKVStore(_rocksdb), new RocksDBKVStore(_rocksdb));
        SignUpNotificationRepository = new SignUpNotificationRepository(new RocksDBKVStore(_rocksdb));
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
        return true;
    }

    public ValueTask<bool> SaveAsync()
    {
        return ValueTask.FromResult(true);
    }
}