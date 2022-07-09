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

using Ferrite.Data;
using Ferrite.TL.currentLayer;

namespace Ferrite.TL.ObjectMapper;

public class PeerSettingsMapper : ITLObjectMapper<PeerSettings, PeerSettingsDTO>
{
    private readonly ITLObjectFactory _factory;
    public PeerSettingsMapper(ITLObjectFactory factory)
    {
        _factory = factory;
    }
    public PeerSettingsDTO MapToDTO(PeerSettings obj)
    {
        throw new NotImplementedException();
    }

    public PeerSettings MapToTLObject(PeerSettingsDTO obj)
    {
        var peerSettings = _factory.Resolve<PeerSettingsImpl>();
        peerSettings.Autoarchived = obj.AutoArchived;
        peerSettings.AddContact = obj.AddContact;
        peerSettings.BlockContact = obj.BlockContact;
        if (obj.GeoDistance != null)
        {
            peerSettings.GeoDistance = (int)obj.GeoDistance;
        }
        peerSettings.InviteMembers = obj.InviteMembers;
        peerSettings.ReportGeo = obj.ReportGeo;
        peerSettings.ReportSpam = obj.ReportSpam;
        peerSettings.ShareContact = obj.ShareContact;
        peerSettings.NeedContactsException = obj.NeedContactsException;
        peerSettings.RequestChatBroadcast = obj.RequestChatBroadcast;
        if (obj.RequestChatDate != null)
        {
            peerSettings.RequestChatDate = (int)obj.RequestChatDate;
        }
        if (obj.RequestChatTitle is { Length: > 0 })
        {
            peerSettings.RequestChatTitle = obj.RequestChatTitle;
        }

        return peerSettings;
    }
}