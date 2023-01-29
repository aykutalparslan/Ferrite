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

using System.Runtime.InteropServices;
using Ferrite.TL.slim;
using Ferrite.TL.slim.layer150;
using MessagePack;

namespace Ferrite.Data.Repositories;

public class NotifySettingsRepository : INotifySettingsRepository
{
    private readonly IKVStore _store;
    public NotifySettingsRepository(IKVStore store)
    {
        _store = store;
        _store.SetSchema(new TableDefinition("ferrite", "notify_settings",
            new KeyDefinition("pk",
                new DataColumn { Name = "auth_key_id", Type = DataType.Long },
                new DataColumn { Name = "notify_peer_type", Type = DataType.Int }, 
                new DataColumn { Name = "peer_type", Type = DataType.Int },
                new DataColumn { Name = "peer_id", Type = DataType.Long },
                new DataColumn { Name = "device_type", Type = DataType.Int })));
    }
    public bool PutNotifySettings(long authKeyId, int notifyPeerType, int peerType, long peerId, int deviceType, TLPeerNotifySettings settings)
    {
        var settingBytes = settings.AsSpan().ToArray();

        return _store.Put(settingBytes, authKeyId, notifyPeerType, peerType, peerId, deviceType);
    }

    public IReadOnlyCollection<TLPeerNotifySettings> GetNotifySettings(long authKeyId, int notifyPeerType, int peerType, long peerId, int deviceType)
    {
        List<TLPeerNotifySettings> results = new();
       
        var iter = _store.Iterate(authKeyId,
            notifyPeerType, peerType, peerId, deviceType);
        foreach (var settingBytes in iter)
        {
            results.Add(new TLPeerNotifySettings(settingBytes.AsMemory(), 0, settingBytes.Length));
        }

        return results;
    }

    public bool DeleteNotifySettings(long authKeyId)
    {
        return _store.Delete(authKeyId);
    }
}