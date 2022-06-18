/*
 *   Project Ferrite is an Implementation Telegram Server API
 *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Affero General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Affero General Public License for more details.
 *
 *   You should have received a copy of the GNU Affero General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Buffers;
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer.contacts;
public class GetBlocked : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IContactsService _contacts;
    private bool serialized = false;
    public GetBlocked(ITLObjectFactory objectFactory, IContactsService contacts)
    {
        factory = objectFactory;
        _contacts = contacts;
    }

    public int Constructor => -176409329;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt32(_offset, true);
            writer.WriteInt32(_limit, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private int _offset;
    public int Offset
    {
        get => _offset;
        set
        {
            serialized = false;
            _offset = value;
        }
    }

    private int _limit;
    public int Limit
    {
        get => _limit;
        set
        {
            serialized = false;
            _limit = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var serviceResult = await _contacts.GetBlocked(ctx.PermAuthKeyId != 0 ? ctx.PermAuthKeyId : ctx.AuthKeyId,
            _offset, _limit);
        var blockedList = factory.Resolve<Vector<PeerBlocked>>();
        var usersList = factory.Resolve<Vector<User>>();
        foreach (var p in serviceResult.BlockedPeers)
        { 
            var peerBlocked = factory.Resolve<PeerBlockedImpl>();
            peerBlocked.Date = p.Date;
            if (p.PeerId.PeerType == PeerType.User)
            {
                var peerId = factory.Resolve<PeerUserImpl>();
                peerId.UserId = p.PeerId.PeerId;
                peerBlocked.PeerId = peerId;
            }
            else if (p.PeerId.PeerType == PeerType.Chat)
            {
                var peerId = factory.Resolve<PeerChatImpl>();
                peerId.ChatId = p.PeerId.PeerId;
                peerBlocked.PeerId = peerId;
            }
            else if (p.PeerId.PeerType == PeerType.Channel)
            {
                var peerId = factory.Resolve<PeerChannelImpl>();
                peerId.ChannelId = p.PeerId.PeerId;
                peerBlocked.PeerId = peerId;
            }
            blockedList.Add(peerBlocked);
        }
        foreach (var u in serviceResult.Users)
        {
            var userImpl = factory.Resolve<UserImpl>();
            userImpl.Id = u.Id;
            userImpl.FirstName = u.FirstName;
            userImpl.LastName = u.LastName;
            userImpl.Phone = u.Phone;
            userImpl.Self = u.Self;
            if(u.Status == Data.UserStatus.Empty)
            {
                userImpl.Status = factory.Resolve<UserStatusEmptyImpl>();
            }
            if (u.Photo.Empty)
            {
                userImpl.Photo = factory.Resolve<UserProfilePhotoEmptyImpl>();
            }
            usersList.Add(userImpl);
        }

        var blocked = factory.Resolve<BlockedImpl>();
        blocked.Blocked = blockedList;
        blocked.Chats = factory.Resolve<Vector<Chat>>();
        blocked.Users = usersList;
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        result.Result = blocked;
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _offset = buff.ReadInt32(true);
        _limit = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}