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

using Ferrite.TL.slim;
using Ferrite.TL.slim.baseLayer;
using Ferrite.TL.slim.baseLayer.contacts;

namespace Ferrite.Services;

public interface IContactsService
{
    Task<ICollection<long>> GetContactIds(long authKeyId, TLBytes q);
    Task<ICollection<TLContactStatus>> GetStatuses(long authKeyId);
    Task<TLContacts> GetContacts(long authKeyId, TLBytes q);
    Task<TLImportedContacts> ImportContacts(long authKeyId, TLBytes q);
    Task<TLUpdates> DeleteContacts(long authKeyId, TLBytes q);
    Task<TLBool> DeleteByPhones(long authKeyId, TLBytes q);
    Task<TLBool> Block(long authKeyId, TLBytes q);
    Task<TLBool> Unblock(long authKeyId, TLBytes q);
    Task<TLBlocked> GetBlocked(long authKeyId, TLBytes q);
    Task<TLFound> Search(long authKeyId, TLBytes q);
    Task<TLResolvedPeer> ResolveUsername(long authKeyId, TLBytes q);
    Task<TLTopPeers> GetTopPeers(long authKeyId, TLBytes q);
    Task<TLBool> ResetTopPeerRating(long authKeyId, TLBytes q);
    Task<TLBool> ResetSaved(long authKeyId);
    Task<ServiceResult<ICollection<TLSavedContact>>> GetSaved(long authKeyId);
    Task<TLBool> ToggleTopPeers(long authKeyId, TLBytes q);
    Task<TLUpdates> AddContact(long authKeyId, TLBytes q);
    Task<TLUpdates> AcceptContact(long authKeyId, TLBytes q);
    Task<TLUpdates> GetLocated(long authKeyId, TLBytes q);
    Task<TLUpdates> BlockFromReplies(long authKeyId, TLBytes q);
    Task<TLResolvedPeer> ResolvePhone(long authKeyId, TLBytes q);
}