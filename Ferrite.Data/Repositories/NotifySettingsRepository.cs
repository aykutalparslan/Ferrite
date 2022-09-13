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

public class NotifySettingsRepository : INotifySettingsRepository
{
    private readonly IKVStore _store;
    public NotifySettingsRepository(IKVStore store, IKVStore storeUsers)
    {
        _store = store;
        _store.SetSchema(new TableDefinition("ferrite", "devices",
            new KeyDefinition("pk",
                new DataColumn { Name = "auth_key_id", Type = DataType.Long },
                new DataColumn { Name = "notify_peer_type", Type = DataType.Int }, 
                new DataColumn { Name = "peer_type", Type = DataType.Int },
                new DataColumn { Name = "peer_id", Type = DataType.Long },
                new DataColumn { Name = "device_type", Type = DataType.Int })));
    }
    public bool PutNotifySettings(long authKeyId, InputNotifyPeerDTO peer, PeerNotifySettingsDTO settings)
    {
        var settingBytes = MessagePackSerializer.Serialize(settings);
        long peerId = 0;
        if (peer.Peer.InputPeerType is InputPeerType.User or InputPeerType.UserFromMessage)
        {
            peerId = peer.Peer.UserId;
        }
        else if (peer.Peer.InputPeerType == InputPeerType.Chat)
        {
            peerId = peer.Peer.ChatId;
        }
        else if (peer.Peer.InputPeerType is InputPeerType.Channel or InputPeerType.ChannelFromMessage)
        {
            peerId = peer.Peer.ChannelId;
        }
        return _store.Put(settingBytes, authKeyId,
            (int)peer.NotifyPeerType, (int)peer.Peer.InputPeerType,
            peerId, (int)settings.DeviceType);
    }

    public IReadOnlyCollection<PeerNotifySettingsDTO> GetNotifySettings(long authKeyId, InputNotifyPeerDTO peer)
    {
        List<PeerNotifySettingsDTO> results = new();
        long peerId = 0;
        if (peer.Peer.InputPeerType is InputPeerType.User or InputPeerType.UserFromMessage)
        {
            peerId = peer.Peer.UserId;
        }
        else if (peer.Peer.InputPeerType == InputPeerType.Chat)
        {
            peerId = peer.Peer.ChatId;
        }
        else if (peer.Peer.InputPeerType is InputPeerType.Channel or InputPeerType.ChannelFromMessage)
        {
            peerId = peer.Peer.ChannelId;
        }

        var iter = _store.Iterate(authKeyId,
            (int)peer.NotifyPeerType, (int)peer.Peer.InputPeerType,
            peerId);
        foreach (var settingBytes in iter)
        {
            var settings = MessagePackSerializer.Deserialize<PeerNotifySettingsDTO>(settingBytes);
            results.Add(settings);
        }

        return results;
    }

    public bool DeleteNotifySettings(long authKeyId)
    {
        return _store.Delete(authKeyId);
    }
}