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
using Ferrite.TL.currentLayer.updates;
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer.messages;
public class GetPeerDialogs : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public GetPeerDialogs(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -462373635;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_peers.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private Vector<InputDialogPeer> _peers;
    public Vector<InputDialogPeer> Peers
    {
        get => _peers;
        set
        {
            serialized = false;
            _peers = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var dialogs = factory.Resolve<PeerDialogsImpl>();
        dialogs.Chats = factory.Resolve<Vector<Chat>>();
        dialogs.Dialogs = factory.Resolve<Vector<Dialog>>();
        foreach (InputDialogPeerImpl p in _peers)
        {
            if (p.Peer is InputPeerUserImpl inputPeer)
            {
                var dialog = factory.Resolve<DialogImpl>();
                var peerUser = factory.Resolve<PeerUserImpl>();
                peerUser.UserId = inputPeer.UserId;
                dialog.Peer = peerUser;
                dialog.NotifySettings = factory.Resolve<PeerNotifySettingsImpl>();
                dialogs.Dialogs.Add(dialog);
            }
        }
        
        dialogs.Messages = factory.Resolve<Vector<Message>>();
        dialogs.Users = factory.Resolve<Vector<User>>();
        var state = factory.Resolve<StateImpl>();
        state.Date = (int)DateTimeOffset.Now.ToUnixTimeSeconds();
        state.Pts = 0;
        state.Qts = 0;
        state.Seq = 0;
        state.UnreadCount = 0;
        dialogs.State = state;
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        result.Result = dialogs;
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        buff.Skip(4); _peers  =  factory . Read < Vector < InputDialogPeer > > ( ref  buff ) ; 
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}