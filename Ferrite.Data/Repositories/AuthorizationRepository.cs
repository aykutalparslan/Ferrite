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

using MessagePack;

namespace Ferrite.Data.Repositories;

public class AuthorizationRepository : IAuthorizationRepository
{
    private readonly IKVStore _store;
    private readonly IKVStore _storeExported;
    public AuthorizationRepository(IKVStore store, IKVStore storeExported)
    {
        _store = store;
        _store.SetSchema(new TableDefinition("ferrite", "authorizations",
            new KeyDefinition("pk",
                new DataColumn { Name = "auth_key_id", Type = DataType.Long },
                new DataColumn { Name = "phone", Type = DataType.String }),
            new KeyDefinition("by_phone",
                new DataColumn { Name = "phone", Type = DataType.String },
                new DataColumn { Name = "auth_key_id", Type = DataType.Long })));
        _storeExported = storeExported;
        _storeExported.SetSchema(new TableDefinition("ferrite", "exported_authorizations",
            new KeyDefinition("pk",
                new DataColumn { Name = "user_id", Type = DataType.Long },
                new DataColumn { Name = "data", Type = DataType.Bytes })));
    }
    public bool PutAuthorization(AuthInfoDTO info)
    {
        var infoBytes = MessagePackSerializer.Serialize(info);
        return _store.Put(infoBytes, info.AuthKeyId, info.Phone);
    }

    public AuthInfoDTO? GetAuthorization(long authKeyId)
    {
        var infoBytes = _store.Get(authKeyId);
        if (infoBytes == null) return null;
        AuthInfoDTO info = MessagePackSerializer.Deserialize<AuthInfoDTO>(infoBytes);
        return info;
    }

    public ValueTask<AuthInfoDTO?> GetAuthorizationAsync(long authKeyId)
    {
        return new ValueTask<AuthInfoDTO?>(GetAuthorization(authKeyId));
    }

    public IReadOnlyCollection<AuthInfoDTO> GetAuthorizations(string phone)
    {
        List<AuthInfoDTO> infoDTOs = new List<AuthInfoDTO>();
        var authorizations = _store.IterateBySecondaryIndex("by_id", phone);
        foreach (var auth in authorizations)
        {
            var info = MessagePackSerializer.Deserialize<AuthInfoDTO>(auth);
            infoDTOs.Add(info);
        }

        return infoDTOs;
    }

    public async ValueTask<IReadOnlyCollection<AuthInfoDTO>> GetAuthorizationsAsync(string phone)
    {
        List<AuthInfoDTO> infoDTOs = new List<AuthInfoDTO>();
        var authorizations = _store.IterateBySecondaryIndexAsync("by_phone", phone);
        if (authorizations == null) return infoDTOs;
        await foreach (var auth in authorizations)
        {
            if (auth != null)
            {
                var info = MessagePackSerializer.Deserialize<AuthInfoDTO>(auth);
                infoDTOs.Add(info);
            }
        }

        return infoDTOs;
    }

    public bool DeleteAuthorization(long authKeyId)
    {
        return _store.Delete(authKeyId);
    }

    public bool PutExportedAuthorization(ExportedAuthInfoDTO exportedInfo)
    {
        var exportedBytes = MessagePackSerializer.Serialize(exportedInfo);
        return _storeExported.Put(exportedBytes, exportedInfo.UserId, exportedInfo.Data);
    }

    public ExportedAuthInfoDTO? GetExportedAuthorization(long user_id, byte[] data)
    {
        var exportedBytes = _storeExported.Get(user_id, data);
        var exportedInfo = MessagePackSerializer.Deserialize<ExportedAuthInfoDTO>(exportedBytes);
        return exportedInfo;
    }

    public async ValueTask<ExportedAuthInfoDTO?> GetExportedAuthorizationAsync(long user_id, byte[] data)
    {
        var exportedBytes = await _storeExported.GetAsync(user_id, data);
        if (exportedBytes == null) return null;
        var exportedInfo = MessagePackSerializer.Deserialize<ExportedAuthInfoDTO>(exportedBytes);
        return exportedInfo;
    }
}