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

using System.Text;
using Ferrite.TL.slim;
using Ferrite.TL.slim.layer150.dto;
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
    public bool PutAuthorization(TLAuthInfo info)
    {
        var authInfo = info.AsAuthInfo();
        return _store.Put(info.AsSpan().ToArray(), authInfo.AuthKeyId, Encoding.UTF8.GetString(authInfo.Phone));
    }

    public TLAuthInfo? GetAuthorization(long authKeyId)
    {
        var infoBytes = _store.Get(authKeyId);
        if (infoBytes == null) return null;
        return new TLAuthInfo(infoBytes, 0, infoBytes.Length);
    }

    public async ValueTask<TLAuthInfo?> GetAuthorizationAsync(long authKeyId)
    {
        var infoBytes =  await _store.GetAsync(authKeyId);
        if (infoBytes == null) return null;
        return new TLAuthInfo(infoBytes, 0, infoBytes.Length);
    }

    public IReadOnlyList<TLAuthInfo> GetAuthorizations(string phone)
    {
        List<TLAuthInfo> infos = new();
        var authorizations = _store.IterateBySecondaryIndex("by_phone", phone);
        foreach (var auth in authorizations)
        {
            infos.Add(new TLAuthInfo(auth, 0, auth.Length));
        }

        return infos;
    }

    public async ValueTask<IReadOnlyList<TLAuthInfo>> GetAuthorizationsAsync(string phone)
    {
        List<TLAuthInfo> infos = new();
        var authorizations = _store.IterateBySecondaryIndexAsync("by_phone", phone);
        await foreach (var auth in authorizations)
        {
            infos.Add(new TLAuthInfo(auth, 0, auth.Length));
        }

        return infos;
    }

    public bool DeleteAuthorization(long authKeyId)
    {
        return _store.Delete(authKeyId);
    }

    public bool PutExportedAuthorization(TLExportedAuthInfo exportedInfo)
    {
        var exported = exportedInfo.AsExportedAuthInfo();
        return _storeExported.Put(exportedInfo.AsSpan().ToArray(), exported.UserId, exported.Data.ToArray());
    }

    public TLExportedAuthInfo? GetExportedAuthorization(long userId, byte[] data)
    {
        var exportedBytes = _storeExported.Get(userId, data);
        if (exportedBytes == null) return null;
        return new TLExportedAuthInfo(exportedBytes, 0 , exportedBytes.Length);
    }

    public async ValueTask<TLExportedAuthInfo?> GetExportedAuthorizationAsync(long userId, byte[] data)
    {
        var exportedBytes = await _storeExported.GetAsync(userId, data);
        if (exportedBytes == null) return null;
        return new TLExportedAuthInfo(exportedBytes, 0 , exportedBytes.Length);
    }
}