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
using PeerSettings = Ferrite.Data.Messages.PeerSettings;

namespace Ferrite.Services;

public class MessagesService : IMessagesService
{
    private readonly IPersistentStore _store;
    public MessagesService(IPersistentStore store)
    {
        _store = store;
    }
    public async Task<ServiceResult<Data.Messages.PeerSettings>> GetPeerSettings(long authKeyId, InputPeer peer)
    {
        if (peer.InputPeerType == InputPeerType.Self)
        {
            var settings = new Data.PeerSettings(false, false, false, 
                false, false, false, 
                false, false, false, 
                null, null, null);
            return new ServiceResult<PeerSettings>(new PeerSettings(settings, new List<Chat>(), new List<User>())
                , true, ErrorMessages.None);
        }
        else if (peer.InputPeerType == InputPeerType.User)
        {
            var settings = new Data.PeerSettings(true, true, true, 
                false, false,  false, 
                false, false, false, 
                null, null, null);
            var users = new List<User>();
            var user = await _store.GetUserAsync(peer.UserId);
            users.Add(user);
            return new ServiceResult<PeerSettings>(new PeerSettings(settings, new List<Chat>(), users)
                , true, ErrorMessages.None);
        }
        return new ServiceResult<PeerSettings>(null, false, ErrorMessages.PeerIdInvalid);
    }
}