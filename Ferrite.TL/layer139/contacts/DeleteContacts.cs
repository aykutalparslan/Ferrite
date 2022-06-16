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
using Ferrite.Utils;

namespace Ferrite.TL.layer139.contacts;
public class DeleteContacts : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IContactsService _contacts;
    private bool serialized = false;
    public DeleteContacts(ITLObjectFactory objectFactory, IContactsService contacts)
    {
        factory = objectFactory;
        _contacts = contacts;
    }

    public int Constructor => 157945344;
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

    private Vector<InputUser> _id;
    public Vector<InputUser> Id
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
        List<Data.InputUser> users = new();
        foreach (var u in _id)
        {
            if (u is InputUserImpl inputUser)
            {
                users.Add(new Data.InputUser()
                {
                    InputUserType = InputUserType.User,
                    UserId = inputUser.UserId,
                    AccessHash = inputUser.AccessHash
                });
            }
            else if (u is InputUserFromMessageImpl inputUserFromMessage)
            {
                users.Add(new Data.InputUser()
                {
                    InputUserType = InputUserType.UserFromMessage,
                    UserId = inputUserFromMessage.UserId,
                    MsgId = inputUserFromMessage.MsgId,
                    Peer = new Data.InputPeer()
                    {
                        InputPeerType = inputUserFromMessage.Peer.Constructor switch
                        {
                            TLConstructor.InputPeerChat => InputPeerType.Chat,
                            TLConstructor.InputPeerChannel => InputPeerType.Channel,
                            TLConstructor.InputPeerUserFromMessage => InputPeerType.UserFromMessage,
                            TLConstructor.InputPeerChannelFromMessage => InputPeerType.ChannelFromMessage,
                            _ => InputPeerType.User
                        },
                        UserId = inputUserFromMessage.Peer.Constructor switch
                        {
                            TLConstructor.InputPeerUser => ((InputPeerUserImpl)inputUserFromMessage.Peer).UserId,
                            TLConstructor.InputPeerUserFromMessage => ((InputPeerUserFromMessageImpl)inputUserFromMessage.Peer).UserId,
                            _ => 0
                        },
                        AccessHash = inputUserFromMessage.Peer.Constructor switch
                        {
                            TLConstructor.InputPeerUser => ((InputPeerUserImpl)inputUserFromMessage.Peer).UserId,
                            TLConstructor.InputPeerUserFromMessage =>
                                ((InputPeerUserImpl)((InputPeerUserFromMessageImpl)inputUserFromMessage.Peer).Peer).AccessHash,
                            _ => 0
                        },
                        ChatId = inputUserFromMessage.Peer.Constructor == TLConstructor.InputPeerChat
                            ? ((InputPeerChatImpl)inputUserFromMessage.Peer).ChatId
                            : 0,
                        ChannelId = inputUserFromMessage.Peer.Constructor switch
                        {
                            TLConstructor.InputPeerChannel => ((InputPeerChannelImpl)inputUserFromMessage.Peer).ChannelId,
                            TLConstructor.InputPeerChannelFromMessage => ((InputPeerChannelFromMessageImpl)inputUserFromMessage.Peer).ChannelId,
                            _ => 0
                        },
                    }
                });
            }
        }

        await _contacts.DeleteContacts(ctx.AuthKeyId, users);
        return null;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        buff.Skip(4); _id  =  factory . Read < Vector < InputUser > > ( ref  buff ) ; 
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}