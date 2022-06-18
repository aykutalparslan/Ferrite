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
public class Block : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IContactsService _contacts;
    private bool serialized = false;
    public Block(ITLObjectFactory objectFactory, IContactsService contacts)
    {
        factory = objectFactory;
        _contacts = contacts;
    }

    public int Constructor => 1758204945;
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

    private InputPeer _id;
    public InputPeer Id
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
        var success =
            await _contacts.Block(ctx.PermAuthKeyId != 0 ? ctx.PermAuthKeyId : ctx.AuthKeyId,
                new Data.InputPeer()
                    {
                        InputPeerType = _id.Constructor switch
                        {
                            TLConstructor.InputPeerChat => InputPeerType.Chat,
                            TLConstructor.InputPeerChannel => InputPeerType.Channel,
                            TLConstructor.InputPeerUserFromMessage => InputPeerType.UserFromMessage,
                            TLConstructor.InputPeerChannelFromMessage => InputPeerType.ChannelFromMessage,
                            _ => InputPeerType.User
                        },
                        UserId = _id.Constructor switch
                        {
                            TLConstructor.InputPeerUser => ((InputPeerUserImpl)_id).UserId,
                            TLConstructor.InputPeerUserFromMessage => ((InputPeerUserFromMessageImpl)_id).UserId,
                            _ => 0
                        },
                        AccessHash = _id.Constructor switch
                        {
                            TLConstructor.InputPeerUser => ((InputPeerUserImpl)_id).UserId,
                            TLConstructor.InputPeerUserFromMessage =>
                                ((InputPeerUserImpl)((InputPeerUserFromMessageImpl)_id).Peer).AccessHash,
                            _ => 0
                        },
                        ChatId = _id.Constructor == TLConstructor.InputPeerChat
                            ? ((InputPeerChatImpl)_id).ChatId
                            : 0,
                        ChannelId = _id.Constructor switch
                        {
                            TLConstructor.InputPeerChannel => ((InputPeerChannelImpl)_id).ChannelId,
                            TLConstructor.InputPeerChannelFromMessage => ((InputPeerChannelFromMessageImpl)_id).ChannelId,
                            _ => 0
                        },
                    });
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        result.Result = success ? new BoolTrue() : new BoolFalse();
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _id = (InputPeer)factory.Read(buff.ReadInt32(true), ref buff);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}