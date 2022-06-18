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

namespace Ferrite.TL.currentLayer.account;
public class GetNotifySettings : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IAccountService _account;
    private bool serialized = false;
    public GetNotifySettings(ITLObjectFactory objectFactory, IAccountService account)
    {
        factory = objectFactory;
        _account = account;
    }

    public int Constructor => 313765169;
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

    private InputNotifyPeer _peer;
    public InputNotifyPeer Peer
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
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        Data.InputNotifyPeer? notifyPeer = null;
        if (_peer.Constructor == TLConstructor.InputNotifyPeer)
        {
            var peer = (InputNotifyPeerImpl)_peer;
            notifyPeer = new Data.InputNotifyPeer()
            {
                NotifyPeerType = InputNotifyPeerType.Peer,
                Peer = new Data.InputPeer()
                {
                    InputPeerType = peer.Peer.Constructor switch
                    {
                        TLConstructor.InputPeerChat => InputPeerType.Chat,
                        TLConstructor.InputPeerChannel => InputPeerType.Channel,
                        TLConstructor.InputPeerUserFromMessage => InputPeerType.UserFromMessage,
                        TLConstructor.InputPeerChannelFromMessage => InputPeerType.ChannelFromMessage,
                        _ => InputPeerType.User
                    },
                    UserId = peer.Peer.Constructor switch
                    {
                        TLConstructor.InputPeerUser => ((InputPeerUserImpl)peer.Peer).UserId,
                        TLConstructor.InputPeerUserFromMessage => ((InputPeerUserFromMessageImpl)peer.Peer).UserId,
                        _ => 0
                    },
                    AccessHash = peer.Peer.Constructor switch
                    {
                        TLConstructor.InputPeerUser => ((InputPeerUserImpl)peer.Peer).UserId,
                        TLConstructor.InputPeerUserFromMessage =>
                            ((InputPeerUserImpl)((InputPeerUserFromMessageImpl)peer.Peer).Peer).AccessHash,
                        _ => 0
                    },
                    ChatId = peer.Peer.Constructor == TLConstructor.InputPeerChat
                        ? ((InputPeerChatImpl)peer.Peer).ChatId
                        : 0,
                    ChannelId = peer.Peer.Constructor switch
                    {
                        TLConstructor.InputPeerChannel => ((InputPeerChannelImpl)peer.Peer).ChannelId,
                        TLConstructor.InputPeerChannelFromMessage => ((InputPeerChannelFromMessageImpl)peer.Peer).ChannelId,
                        _ => 0
                    },
                }
            };
        } 
        else if (_peer.Constructor == TLConstructor.InputNotifyChats)
        {
            notifyPeer = new Data.InputNotifyPeer()
            {
                NotifyPeerType = InputNotifyPeerType.Chats
            };
        }
        else if (_peer.Constructor == TLConstructor.InputNotifyUsers)
        {
            notifyPeer = new Data.InputNotifyPeer()
            {
                NotifyPeerType = InputNotifyPeerType.Users
            };
        }
        else if (_peer.Constructor == TLConstructor.InputNotifyBroadcasts)
        {
            notifyPeer = new Data.InputNotifyPeer()
            {
                NotifyPeerType = InputNotifyPeerType.Broadcasts
            };
        }

        if (notifyPeer == null)
        {
            var err = factory.Resolve<RpcError>();
            err.ErrorCode = 400;
            err.ErrorMessage = "";
            result.Result = err;
            return result;
        }
        var settings = await _account.GetNotifySettings(ctx.PermAuthKeyId!=0 ? ctx.PermAuthKeyId : ctx.AuthKeyId, notifyPeer);
        var resp = factory.Resolve<PeerNotifySettingsImpl>();
        resp.ShowPreviews = settings.ShowPreviews;
        resp.Silent = settings.Silent;
        if (settings.MuteUntil > 0)
        {
            resp.MuteUntil = settings.MuteUntil;
        }
        if (settings.Sound.Length>0)
        {
            resp.Sound = settings.Sound;
        }
        result.Result = resp;
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _peer = (InputNotifyPeer)factory.Read(buff.ReadInt32(true), ref buff);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}