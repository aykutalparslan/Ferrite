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

namespace Ferrite.TL.currentLayer.messages;
public class GetPeerSettings : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IMessagesService _messages;
    private bool serialized = false;
    public GetPeerSettings(ITLObjectFactory objectFactory, IMessagesService messages)
    {
        factory = objectFactory;
        _messages = messages;
    }

    public int Constructor => -270948702;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_peer.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private InputPeer _peer;
    public InputPeer Peer
    {
        get => _peer;
        set
        {
            serialized = false;
            _peer = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var inputPeer = _peer.Constructor switch
        {
            TLConstructor.InputPeerUser => new Data.InputPeerDTO()
            {
                UserId = ((InputPeerUserImpl)_peer).UserId,
                AccessHash = ((InputPeerUserImpl)_peer).UserId,
            },
            TLConstructor.InputPeerChat => new Data.InputPeerDTO()
            {
                ChatId = ((InputPeerChatImpl)_peer).ChatId,
            },
            TLConstructor.InputPeerUserFromMessage => new Data.InputPeerDTO()
            {
                UserId = ((InputPeerUserFromMessageImpl)_peer).UserId,
                MsgId = ((InputPeerUserFromMessageImpl)_peer).MsgId,
                ChatId = ((InputPeerChatImpl)((InputPeerUserFromMessageImpl)_peer).Peer).ChatId
            },
            TLConstructor.InputPeerChannel => new Data.InputPeerDTO()
            {
                ChannelId = ((InputPeerChannelImpl)_peer).ChannelId,
                AccessHash = ((InputPeerChannelImpl)_peer).AccessHash,
            },
            TLConstructor.InputPeerSelf => new Data.InputPeerDTO(){ InputPeerType = InputPeerType.Self},
            TLConstructor.InputPeerChannelFromMessage => new Data.InputPeerDTO()
            {
                ChannelId = ((InputPeerChannelFromMessageImpl)_peer).ChannelId,
                ChatId = ((InputPeerChatImpl)((InputPeerChannelFromMessageImpl)_peer).Peer).ChatId
            },
            _ => new Data.InputPeerDTO(){ InputPeerType = InputPeerType.Empty},
        };
        var serviceResult = await _messages.GetPeerSettings(ctx.CurrentAuthKeyId ,inputPeer);
        var rpcResult = factory.Resolve<RpcResult>();
        rpcResult.ReqMsgId = ctx.MessageId;
        if (!serviceResult.Success)
        {
            var err = factory.Resolve<RpcError>();
            err.ErrorCode = serviceResult.ErrorMessage.Code;
            err.ErrorMessage = serviceResult.ErrorMessage.Message;
            rpcResult.Result = err;
        }
        else
        {
            var peerSettings = factory.Resolve<currentLayer.PeerSettingsImpl>();
            peerSettings.Autoarchived = serviceResult.Result.Settings.AutoArchived;
            peerSettings.AddContact = serviceResult.Result.Settings.AddContact;
            peerSettings.BlockContact = serviceResult.Result.Settings.BlockContact;
            if (serviceResult.Result.Settings.GeoDistance != null)
            {
                peerSettings.GeoDistance = (int)serviceResult.Result.Settings.GeoDistance;
            }
            peerSettings.InviteMembers = serviceResult.Result.Settings.InviteMembers;
            peerSettings.ReportGeo = serviceResult.Result.Settings.ReportGeo;
            peerSettings.ReportSpam = serviceResult.Result.Settings.ReportSpam;
            peerSettings.ShareContact = serviceResult.Result.Settings.ShareContact;
            peerSettings.NeedContactsException = serviceResult.Result.Settings.NeedContactsException;
            peerSettings.RequestChatBroadcast = serviceResult.Result.Settings.RequestChatBroadcast;
            if (serviceResult.Result.Settings.RequestChatDate != null)
            {
                peerSettings.RequestChatDate = (int)serviceResult.Result.Settings.RequestChatDate;
            }
            if (serviceResult.Result.Settings.RequestChatTitle != null && 
                serviceResult.Result.Settings.RequestChatTitle.Length > 0)
            {
                peerSettings.RequestChatTitle = serviceResult.Result.Settings.RequestChatTitle;
            }
            var settings = factory.Resolve<PeerSettingsImpl>();
            settings.Chats = factory.Resolve<Vector<Chat>>();
            settings.Users = factory.Resolve<Vector<User>>();
            foreach (var u in serviceResult.Result.Users)
            {
                var userImpl = factory.Resolve<UserImpl>();
                userImpl.Id = u.Id;
                userImpl.FirstName = u.FirstName;
                userImpl.LastName = u.LastName;
                userImpl.Phone = u.Phone;
                userImpl.Self = u.Self;
                if (u.Username?.Length > 0)
                {
                    userImpl.Username = u.Username;
                }
                if(u.Status == Data.UserStatusDTO.Empty)
                {
                    userImpl.Status = factory.Resolve<UserStatusEmptyImpl>();
                }
                if (u.Photo.Empty)
                {
                    userImpl.Photo = factory.Resolve<UserProfilePhotoEmptyImpl>();
                }
                else
                {
                    var photo = factory.Resolve<UserProfilePhotoImpl>();
                    photo.DcId = u.Photo.DcId;
                    photo.PhotoId = u.Photo.PhotoId;
                    photo.HasVideo = u.Photo.HasVideo;
                    if (u.Photo.StrippedThumb is { Length: > 0 })
                    {
                        photo.StrippedThumb = u.Photo.StrippedThumb;
                    }
                    userImpl.Photo = photo;
                }
                settings.Users.Add(userImpl);
            }
            rpcResult.Result = settings;
        }
        
        return rpcResult;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _peer = (InputPeer)factory.Read(buff.ReadInt32(true), ref buff);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}