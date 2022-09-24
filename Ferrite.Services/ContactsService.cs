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
using Ferrite.Data.Contacts;
using Ferrite.Data.Repositories;

namespace Ferrite.Services;

public class ContactsService : IContactsService
{
    private readonly IUnitOfWork _unitOfWork;
    public ContactsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ICollection<long>> GetContactIds(long authKeyId, long hash)
    {
        return new List<long>();
    }

    public async Task<ICollection<ContactStatusDTO>> GetStatuses(long authKeyId)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var contactList = _unitOfWork.ContactsRepository.GetContacts(auth.UserId);
        var result =  new List<ContactStatusDTO>();
        foreach (var c in contactList)
        {
            var status = _unitOfWork.UserStatusRepository.GetUserStatus(c.UserId);
            var contactStatus = new ContactStatusDTO(c.UserId, status);
            result.Add(contactStatus);
        }
        return result;
    }

    public async Task<ContactsDTO> GetContacts(long authKeyId, long hash)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var contactList = _unitOfWork.ContactsRepository.GetContacts(auth.UserId);
        List<UserDTO> userList = new List<UserDTO>();
        foreach (var c in contactList)
        {
            userList.Add(_unitOfWork.UserRepository.GetUser(c.UserId));
        }

        return new ContactsDTO(contactList, contactList.Count, userList);
    }

    public async Task<ImportedContactsDTO> ImportContacts(long authKeyId, ICollection<InputContactDTO> contacts)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        List<ImportedContactDTO> importedContacts = new();
        List<UserDTO> users = new();
        foreach (var c in contacts)
        {
            var user = _unitOfWork.UserRepository.GetUser(c.Phone);
            var imported = _unitOfWork.ContactsRepository.PutContact(auth.UserId, user.Id, c);
            users.Add(_unitOfWork.UserRepository.GetUser(imported.UserId));
            importedContacts.Add(imported);
        }

        return new ImportedContactsDTO(importedContacts, new List<PopularContactDTO>(), 
            new List<long>(), users);
    }

    public async Task<UpdatesBase?> DeleteContacts(long authKeyId, ICollection<InputUserDTO> id)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        foreach (var c in id)
        {
            _unitOfWork.ContactsRepository.DeleteContact(auth.UserId, c.UserId);
        }

        await _unitOfWork.SaveAsync();
        return null;
    }

    public async Task<bool> DeleteByPhones(long authKeyId, ICollection<string> phones)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        foreach (var p in phones)
        {
            var userId = _unitOfWork.UserRepository.GetUserId(p);
            if (userId != null)
            {
                _unitOfWork.ContactsRepository.DeleteContact(auth.UserId, (long)userId);
                await _unitOfWork.SaveAsync();
            }
        }

        return true;
    }

    public async Task<bool> Block(long authKeyId, InputUserDTO id)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> Block(long authKeyId, InputPeerDTO id)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (id.InputPeerType is InputPeerType.Channel or InputPeerType.ChannelFromMessage)
        {
            _unitOfWork.BlockedPeersRepository.PutBlockedPeer(auth.UserId, id.ChannelId, PeerType.Channel);
        }
        if (id.InputPeerType is InputPeerType.User or InputPeerType.UserFromMessage)
        {
            _unitOfWork.BlockedPeersRepository.PutBlockedPeer(auth.UserId, id.UserId, PeerType.User);
        }
        else
        {
            _unitOfWork.BlockedPeersRepository.PutBlockedPeer(auth.UserId, id.ChatId, PeerType.Chat);
        }

        return await _unitOfWork.SaveAsync();
    }

    public async Task<bool> Unblock(long authKeyId, InputPeerDTO id)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (id.InputPeerType is InputPeerType.Channel or InputPeerType.ChannelFromMessage)
        {
            _unitOfWork.BlockedPeersRepository.DeleteBlockedPeer(auth.UserId, id.ChannelId, PeerType.Channel);
        }
        if (id.InputPeerType is InputPeerType.User or InputPeerType.UserFromMessage)
        {
            _unitOfWork.BlockedPeersRepository.DeleteBlockedPeer(auth.UserId, id.UserId, PeerType.User);
        }
        else
        {
            _unitOfWork.BlockedPeersRepository.DeleteBlockedPeer(auth.UserId, id.ChatId, PeerType.Chat);
        }

        return await _unitOfWork.SaveAsync();
    }

    public async Task<BlockedDTO> GetBlocked(long authKeyId, int offset, int limit)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var blockedPeers = _unitOfWork.BlockedPeersRepository.GetBlockedPeers(auth.UserId);
        List<UserDTO> users= new();
        foreach (var p in blockedPeers)
        {
            if (p.PeerId.PeerType == PeerType.User)
            {
                users.Add(_unitOfWork.UserRepository.GetUser(p.PeerId.PeerId));
            }
        }
        //TODO: also fetch the chats from the db
        return new BlockedDTO(blockedPeers.Count, blockedPeers,new List<ChatDTO>(), users);
    }

    public async Task<FoundDTO> Search(long authKeyId, string q, int limit)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<ResolvedPeerDTO>> ResolveUsername(long authKeyId, string username)
    {
        throw new NotImplementedException();
    }

    public async Task<TopPeersDTO> GetTopPeers(long authKeyId, bool correspondents, bool botsPm, bool botsInline, bool phoneCalls, bool forwardUsers,
        bool forwardChats, bool groups, bool channels, int offset, int limit, long hash)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<bool>> ResetTopPeerRating(long authKeyId, TopPeerCategory category, PeerDTO peer)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ResetSaved(long authKeyId)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        _unitOfWork.ContactsRepository.DeleteContacts(auth.UserId);
        return await _unitOfWork.SaveAsync();
    }

    public async Task<ServiceResult<ICollection<SavedContactDTO>>> GetSaved(long authKeyId)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        return new ServiceResult<ICollection<SavedContactDTO>>(_unitOfWork.ContactsRepository.GetSavedContacts(auth.UserId),
                true, ErrorMessages.None);
    }

    public async Task<bool> ToggleTopPeers(long authKeyId, bool enabled)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<UpdatesBase>> AddContact(long authKeyId, bool AddPhonePrivacyException, InputUserDTO id, string firstname, string lastname,
        string phone)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<UpdatesBase>> AcceptContact(long authKeyId, InputUserDTO id)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<UpdatesBase>> GetLocated(long authKeyId, bool background, InputGeoPointDTO geoPoint, int? selfExpires)
    {
        throw new NotImplementedException();
    }

    public async Task<UpdatesBase?> BlockFromReplies(long authKeyId, bool deleteMessage, bool deleteHistory, bool reportSpam, int messageId)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<ResolvedPeerDTO>> ResolvePhone(long authKeyId, string phone)
    {
        throw new NotImplementedException();
    }
}