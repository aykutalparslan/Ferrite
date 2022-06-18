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

namespace Ferrite.TL.currentLayer.users;
public class GetFullUser : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IUsersService _users;
    private bool serialized = false;
    public GetFullUser(ITLObjectFactory objectFactory, IUsersService users)
    {
        factory = objectFactory;
        _users = users;
    }

    public int Constructor => -1240508136;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_id.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private InputUser _id;
    public InputUser Id
    {
        get => _id;
        set
        {
            serialized = false;
            _id = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var result = factory.Resolve<RpcResult>();
        Data.InputUser inputUser = null;
        if (_id is InputUserImpl user)
        {
            inputUser = new Data.InputUser()
            {
                InputUserType = InputUserType.User,
                UserId = user.UserId,
                AccessHash = user.AccessHash
            };
        }
        else if (_id is InputUserFromMessageImpl userFromMessage)
        {
            //TODO: handle this case
        }
        else if (_id is InputUserSelfImpl userSelf)
        {
            inputUser = new Data.InputUser()
            {
                InputUserType = InputUserType.Self
            };
        }
        var serviceResult = await _users.GetFullUser(ctx.PermAuthKeyId != 0 ? ctx.PermAuthKeyId : ctx.AuthKeyId,
            inputUser);
        if (serviceResult.Success)
        {
            var peerSettings = factory.Resolve<PeerSettingsImpl>();
            peerSettings.Autoarchived = serviceResult.Result.FullUser.Settings.AutoArchived;
            peerSettings.AddContact = serviceResult.Result.FullUser.Settings.AddContact;
            peerSettings.BlockContact = serviceResult.Result.FullUser.Settings.BlockContact;
            if (serviceResult.Result.FullUser.Settings.GeoDistance != null)
            {
                peerSettings.GeoDistance = (int)serviceResult.Result.FullUser.Settings.GeoDistance;
            }
            peerSettings.InviteMembers = serviceResult.Result.FullUser.Settings.InviteMembers;
            peerSettings.ReportGeo = serviceResult.Result.FullUser.Settings.ReportGeo;
            peerSettings.ReportSpam = serviceResult.Result.FullUser.Settings.ReportSpam;
            peerSettings.ShareContact = serviceResult.Result.FullUser.Settings.ShareContact;
            peerSettings.NeedContactsException = serviceResult.Result.FullUser.Settings.NeedContactsException;
            peerSettings.RequestChatBroadcast = serviceResult.Result.FullUser.Settings.RequestChatBroadcast;
            if (serviceResult.Result.FullUser.Settings.RequestChatDate != null)
            {
                peerSettings.RequestChatDate = (int)serviceResult.Result.FullUser.Settings.RequestChatDate;
            }
            if (serviceResult.Result.FullUser.Settings.RequestChatTitle != null && 
                serviceResult.Result.FullUser.Settings.RequestChatTitle.Length > 0)
            {
                peerSettings.RequestChatTitle = serviceResult.Result.FullUser.Settings.RequestChatTitle;
            }
            var notifySettings = factory.Resolve<PeerNotifySettingsImpl>();
            notifySettings.ShowPreviews = serviceResult.Result.FullUser.NotifySettings.ShowPreviews;
            notifySettings.Silent = serviceResult.Result.FullUser.NotifySettings.Silent;
            if (serviceResult.Result.FullUser.NotifySettings.MuteUntil > 0)
            {
                notifySettings.MuteUntil = serviceResult.Result.FullUser.NotifySettings.MuteUntil;
            }
            if (serviceResult.Result.FullUser.NotifySettings.Sound.Length>0)
            {
                notifySettings.Sound = serviceResult.Result.FullUser.NotifySettings.Sound;
            }
            var fullUser = factory.Resolve<currentLayer.UserFullImpl>();
            if (serviceResult.Result.FullUser.About != null &&
                serviceResult.Result.FullUser.About.Length > 0)
            {
                fullUser.About = serviceResult.Result.FullUser.About;
            }
            fullUser.Blocked = serviceResult.Result.FullUser.Blocked;
            fullUser.Id = serviceResult.Result.FullUser.Id;
            fullUser.Settings = peerSettings;
            fullUser.NotifySettings = notifySettings;
            fullUser.Blocked = serviceResult.Result.FullUser.Blocked;
            fullUser.PhoneCallsAvailable = serviceResult.Result.FullUser.PhoneCallsAvailable;
            fullUser.PhoneCallsPrivate = serviceResult.Result.FullUser.PhoneCallsPrivate;
            fullUser.CommonChatsCount = serviceResult.Result.FullUser.CommonChatsCount;
            
            var userFull = factory.Resolve<UserFullImpl>();
            userFull.Chats = factory.Resolve<Vector<Chat>>();
            userFull.Users = factory.Resolve<Vector<User>>();
            userFull.FullUser = fullUser;
            result.Result = userFull;
            //TODO: implemnt this properly
        }
        else
        {
            var err = factory.Resolve<RpcError>();
            err.ErrorCode = serviceResult.ErrorMessage.Code;
            err.ErrorMessage = serviceResult.ErrorMessage.Message;
            result.Result = err;
        }
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _id = (InputUser)factory.Read(buff.ReadInt32(true), ref buff);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}