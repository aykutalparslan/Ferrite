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

using System.Collections;
using System.Text;
using Ferrite.Data;
using Ferrite.Data.Contacts;
using Ferrite.Data.Repositories;
using Ferrite.TL.slim;
using Ferrite.TL.slim.layer150;
using Ferrite.TL.slim.layer150.contacts;
using Ferrite.TL.slim.layer150.dto;
using Org.BouncyCastle.Utilities;

namespace Ferrite.Services;

public class ContactsService : IContactsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISearchEngine _search;
    private readonly IUpdatesContextFactory _updatesContextFactory;
    public ContactsService(IUnitOfWork unitOfWork, ISearchEngine search,
        IUpdatesContextFactory updatesContextFactory)
    {
        _unitOfWork = unitOfWork;
        _search = search;
        _updatesContextFactory = updatesContextFactory;
    }

    public async Task<ICollection<long>> GetContactIds(long authKeyId, TLBytes q)
    {
        return new List<long>();
    }

    public async Task<ICollection<TLContactStatus>> GetStatuses(long authKeyId)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var result =  new List<TLContactStatus>();
        if (auth == null) return result;
        var contactList = _unitOfWork.ContactsRepository.GetContacts(auth.Value.AsAuthInfo().UserId);
        
        foreach (var c in contactList)
        {
            var userId = c.AsContact().UserId;
            var status = await _unitOfWork.UserStatusRepository.GetUserStatusAsync(userId);
            TLContactStatus contactStatus = ContactStatus.Builder()
                .UserId(userId)
                .Status(status.AsSpan())
                .Build();
            result.Add(contactStatus);
        }
        return result;
    }

    public async Task<TLContacts> GetContacts(long authKeyId, TLBytes q)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return Contacts.Builder()
                .ContactsProperty(new Vector())
                .Users(new Vector())
                .SavedCount(0)
                .Build();
        }

        var contactList = _unitOfWork.ContactsRepository.GetContacts(auth.Value.AsAuthInfo().UserId);
        
        List<TLUser> userList = new ();
        foreach (var c in contactList)
        {
            var user = _unitOfWork.UserRepository.GetUser(c.AsContact().UserId);
            if(user != null) userList.Add(user.Value);
        }

        return Contacts.Builder()
            .ContactsProperty(ToContactVector(contactList.ToList()))
            .Users(ToUserVector(userList))
            .SavedCount(contactList.Count)
            .Build();
    }
    
    private static Vector ToUserVector(ICollection<TLUser> users)
    {
        Vector v = new Vector();
        foreach (var s in users)
        {
            v.AppendTLObject(s.AsSpan());
            s.Dispose();
        }

        return v;
    }
    
    private static Vector ToContactVector(ICollection<TLContact> users)
    {
        Vector v = new Vector();
        foreach (var s in users)
        {
            v.AppendTLObject(s.AsSpan());
            s.Dispose();
        }

        return v;
    }

    public async Task<TLImportedContacts> ImportContacts(long authKeyId, TLBytes q)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        List<TLImportedContact> importedContacts = new();
        List<TLUser> users = new();
        var contacts = ToInputContactList(new ImportContacts(q.AsSpan()).Contacts);
        foreach (var c in contacts)
        {
            var user = _unitOfWork.UserRepository.GetUser(Encoding.UTF8.GetString(c.AsInputPhoneContact().Phone));
            if (user == null || auth == null) continue;
            TLContactInfo contactInfo = ContactInfo.Builder()
                .UserId(user.Value.AsUser().Id)
                .Phone(c.AsInputPhoneContact().Phone)
                .ClientId(c.AsInputPhoneContact().ClientId)
                .FirstName(c.AsInputPhoneContact().FirstName)
                .LastName(c.AsInputPhoneContact().LastName)
                .Date((int)DateTimeOffset.Now.ToUnixTimeSeconds())
                .Build();
            var imported = _unitOfWork.ContactsRepository.PutContact(auth.Value.AsAuthInfo().UserId, 
                user.Value.AsUser().Id, contactInfo);
            var contactUser = _unitOfWork.UserRepository.GetUser(imported.AsImportedContact().UserId);
            if(contactUser == null) continue;
            users.Add(contactUser.Value);
            importedContacts.Add(imported);
        }

        return ImportedContacts.Builder()
            .Users(ToUserVector(users))
            .Imported(ToImportedContactVector(importedContacts))
            .PopularInvites(new Vector())
            .RetryContacts(new VectorOfLong())
            .Build();
    }
    
    private static Vector ToImportedContactVector(ICollection<TLImportedContact> contacts)
    {
        Vector v = new Vector();
        foreach (var c in contacts)
        {
            v.AppendTLObject(c.AsSpan());
            c.Dispose();
        }

        return v;
    }

    private static List<TLInputContact> ToInputContactList(Vector inputContacts)
    {
        List<TLInputContact> result = new();
        for (int i = 0; i < inputContacts.Count; i++)
        {
            var inputContactBytes = inputContacts.ReadTLObject().ToArray();
            result.Add(new TLInputContact(inputContactBytes, 0, inputContactBytes.Length));
        }

        return result;
    }

    public async Task<TLUpdates> DeleteContacts(long authKeyId, TLBytes q)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var userId = auth.Value.AsAuthInfo().UserId;
        var id = ToInputUserList(new DeleteContacts(q.AsSpan()).Id);
        List<TLBytes> userList = new();
        List<TLUpdate> updateList = new();
        foreach (var c in id)
        {
            var contactUserId = c.AsInputUser().UserId;
            var contactUser = await GetUserInternal(contactUserId);
            if (contactUser != null) userList.Add(contactUser.Value);
            _unitOfWork.ContactsRepository.DeleteContact(userId, contactUserId);
            using TLPeer peer = new PeerUser(contactUserId);
            using TLPeerSettings settings = PeerSettings.Builder()
                .AddContact(true)
                .Build();
            TLUpdate update = UpdatePeerSettings.Builder()
                .Peer(peer.AsSpan())
                .Settings(settings.AsSpan())
                .Build();
            updateList.Add(update);
            c.Dispose();
        }

        await _unitOfWork.SaveAsync();

        var updatesCtx = _updatesContextFactory.GetUpdatesContext(authKeyId, userId);
        var seq = await updatesCtx.IncrementSeq();

        TLUpdates res = Updates.Builder()
            .Users(userList.ToVector())
            .UpdatesProperty(ToUpdateVector(updateList))
            .Chats(new Vector())
            .Seq(seq)
            .Date((int)DateTimeOffset.Now.ToUnixTimeSeconds())
            .Build();
        return res;
    }
    
    private async ValueTask<TLUser?> GetUserInternal(long userId)
    {
        using var user = _unitOfWork.UserRepository.GetUser(userId);
        if (user == null) return null;
        var status = await _unitOfWork.UserStatusRepository.GetUserStatusAsync(user.Value.AsUser().Id);
        return user.Value.AsUser().Clone().Status(status.AsSpan()).Build();
    }
    
    private static List<TLInputUser> ToInputUserList(Vector inputUsers)
    {
        List<TLInputUser> result = new();
        for (int i = 0; i < inputUsers.Count; i++)
        {
            var inputUserBytes = inputUsers.ReadTLObject().ToArray();
            result.Add(new TLInputUser(inputUserBytes, 0, inputUserBytes.Length));
        }

        return result;
    }
    
    private static Vector ToUpdateVector(ICollection<TLUpdate> updates)
    {
        Vector v = new Vector();
        foreach (var s in updates)
        {
            v.AppendTLObject(s.AsSpan());
            s.Dispose();
        }

        return v;
    }

    public async Task<TLBool> DeleteByPhones(long authKeyId, TLBytes q)
    {
        var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        var phones = ToStringList(new DeleteByPhones(q.AsSpan()).Phones);
        foreach (var p in phones)
        {
            var userId = _unitOfWork.UserRepository.GetUserId(p);
            if (userId != null)
            {
                _unitOfWork.ContactsRepository.DeleteContact(auth.Value.AsAuthInfo().UserId, (long)userId);
            }
        }
        await _unitOfWork.SaveAsync();

        return new BoolTrue();
    }
    
    private static List<string> ToStringList(VectorOfString v)
    {
        List<string> result = new();
        for (int i = 0; i < v.Count; i++)
        {
            var s = Encoding.UTF8.GetString(v.ReadTLBytes());
        }

        return result;
    }

    public async Task<TLBool> Block(long authKeyId, TLBytes q)
    {
        /*var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
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

        return await _unitOfWork.SaveAsync();*/
        throw new NotImplementedException();
    }

    public async Task<TLBool> Unblock(long authKeyId, TLBytes q)
    {
        /*var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
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

        return await _unitOfWork.SaveAsync();*/
        throw new NotImplementedException();
    }

    public async Task<TLBlocked> GetBlocked(long authKeyId, TLBytes q)
    {
        /*var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
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
        return new BlockedDTO(blockedPeers.Count, blockedPeers,new List<ChatDTO>(), users);*/
        throw new NotImplementedException();
    }

    public async Task<TLFound> Search(long authKeyId, TLBytes q)
    {
        /*var searchResults = await _search.SearchUser(q, limit);
        List<PeerDTO> peers = new();
        List<UserDTO> users = new();
        foreach (var u in searchResults)
        {
            var user = _users.GetUser(u.Id);
            if (user != null)
            {
                peers.Add(new PeerDTO(PeerType.User, u.Id));
                users.Add(user);
            }
        }

        return new FoundDTO(Array.Empty<PeerDTO>(), peers,
            Array.Empty<ChatDTO>(), users);*/
        throw new NotImplementedException();
    }

    public async Task<TLResolvedPeer> ResolveUsername(long authKeyId, TLBytes q)
    {
        /*var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        if (auth == null)
        {
            return new ServiceResult<ResolvedPeerDTO>(null, false, ErrorMessages.InvalidAuthKey);
        }
        var peerUser = _unitOfWork.UserRepository.GetUserByUsername(username);
        if (peerUser == null)
        {
            return new ServiceResult<ResolvedPeerDTO>(null, false, ErrorMessages.UsernameInvalid);
        }

        var peer = new PeerDTO(PeerType.User, peerUser.Id);
        var resolved = new ResolvedPeerDTO(peer, Array.Empty<ChatDTO>(), 
            new List<UserDTO>() { peerUser });
        return new ServiceResult<ResolvedPeerDTO>(resolved, true, ErrorMessages.None);*/
        throw new NotImplementedException();
    }

    public async Task<TLTopPeers> GetTopPeers(long authKeyId, TLBytes q)
    {
        throw new NotImplementedException();
    }

    public async Task<TLBool> ResetTopPeerRating(long authKeyId, TLBytes q)
    {
        throw new NotImplementedException();
    }

    public async Task<TLBool> ResetSaved(long authKeyId)
    {
        /*var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        _unitOfWork.ContactsRepository.DeleteContacts(auth.UserId);
        return await _unitOfWork.SaveAsync();*/
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<ICollection<TLSavedContact>>> GetSaved(long authKeyId)
    {
        /*var auth = await _unitOfWork.AuthorizationRepository.GetAuthorizationAsync(authKeyId);
        return new ServiceResult<ICollection<SavedContactDTO>>(_unitOfWork.ContactsRepository.GetSavedContacts(auth.UserId),
                true, ErrorMessages.None);*/
        throw new NotImplementedException();
    }

    public async Task<TLBool> ToggleTopPeers(long authKeyId, TLBytes q)
    {
        throw new NotImplementedException();
    }

    public async Task<TLUpdates> AddContact(long authKeyId, TLBytes q)
    {
        throw new NotImplementedException();
    }

    public async Task<TLUpdates> AcceptContact(long authKeyId, TLBytes q)
    {
        throw new NotImplementedException();
    }

    public async Task<TLUpdates> GetLocated(long authKeyId, TLBytes q)
    {
        throw new NotImplementedException();
    }

    public async Task<TLUpdates> BlockFromReplies(long authKeyId, TLBytes q)
    {
        throw new NotImplementedException();
    }

    public async Task<TLResolvedPeer> ResolvePhone(long authKeyId, TLBytes q)
    {
        throw new NotImplementedException();
    }
}