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
using Ferrite.TL.slim.baseLayer.dto;

namespace Ferrite.Data.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly IKVStore _store;
    public MessageRepository(IKVStore store)
    {
        _store = store;
        store.SetSchema(new TableDefinition("ferrite", "messages",
            new KeyDefinition("pk",
                new DataColumn { Name = "user_id", Type = DataType.Long },
                new DataColumn { Name = "peer_type", Type = DataType.Int },
                new DataColumn { Name = "peer_id", Type = DataType.Long },
                new DataColumn { Name = "outgoing", Type = DataType.Bool },
                new DataColumn { Name = "message_id", Type = DataType.Int },
                new DataColumn { Name = "pts", Type = DataType.Int },
                new DataColumn { Name = "date", Type = DataType.Long }),
            new KeyDefinition("by_id",
                new DataColumn { Name = "user_id", Type = DataType.Long },
                new DataColumn { Name = "message_id", Type = DataType.Int })));
    }
    public bool PutMessage(long userId, TLMessage message, int pts)
    {
        bool outgoing = message.AsMessage().OutProperty;
        int peerType = outgoing 
            ? (int)message.AsMessage().Get_PeerId().Type 
            : (int)message.AsMessage().Get_FromId().Type;
        
        long peerId = outgoing 
            ? GetPeerId(message.AsMessage().Get_PeerId()) 
            : GetPeerId(message.AsMessage().Get_FromId());
        
        using TLSavedMessage savedMessage = SavedMessage.Builder()
            .Pts(pts)
            .OriginalMessage(message.AsSpan())
            .Build();
        _store.Put(savedMessage.AsSpan().ToArray(), userId, peerType, peerId,
            outgoing, message.AsMessage().Id, pts, DateTimeOffset.Now.ToUnixTimeSeconds());
        return true;
    }

    private static long GetPeerId(TLPeer peer) => peer.Type switch
    {
        TLPeer.PeerType.PeerUser => peer.AsPeerUser().UserId,
        TLPeer.PeerType.PeerChat => peer.AsPeerChat().ChatId,
        TLPeer.PeerType.PeerChannel => peer.AsPeerChannel().ChannelId,
        _ => 0
    };

    public IReadOnlyCollection<TLSavedMessage> GetMessages(long userId, TLInputPeer? peerId = null)
    { 
        List<TLSavedMessage> messages = new();
        if (peerId != null)
        {
            List<object> parameters = new List<object>();
            parameters.Add(userId);
            parameters.Add((int)peerId.Value.Type);
            parameters.Add(GetPeerId(peerId.Value));
            
            var results = _store.Iterate(parameters.ToArray());
            foreach (var val in results)
            {
                messages.Add(new TLSavedMessage(val, 0 ,val.Length));
            }
        }
        else
        {
            var results = _store.Iterate(userId);
            foreach (var val in results)
            {
                messages.Add(new TLSavedMessage(val, 0 ,val.Length));
            }
        }
        messages = messages.OrderByDescending(m => 
            m.AsSavedMessage().Get_OriginalMessage().AsMessage().Id).ToList();
        return messages;
    }

    public async ValueTask<IReadOnlyCollection<TLSavedMessage>> GetMessagesAsync(long userId, TLInputPeer? peerId = null)
    {
        List<TLSavedMessage> messages = new();
        if (peerId != null)
        {
            List<object> parameters = new List<object>();
            parameters.Add(userId);
            parameters.Add((int)peerId.Value.Type);
            parameters.Add(GetPeerId(peerId.Value));
            var results = _store.IterateAsync(parameters.ToArray());
            await foreach (var val in results)
            {
                messages.Add(new TLSavedMessage(val, 0 ,val.Length));
            }
        }
        else
        {
            var results = _store.IterateAsync(userId);
            await foreach (var val in results)
            {
                try
                {
                    messages.Add(new TLSavedMessage(val, 0 ,val.Length));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
        messages = messages.OrderByDescending(m => 
            m.AsSavedMessage().Get_OriginalMessage().AsMessage().Id).ToList();
        return messages;
    }

    public IReadOnlyCollection<TLSavedMessage> GetMessages(long userId, int pts, int maxPts, DateTimeOffset date)
    {
        List<TLSavedMessage> messages = new();
        var results = _store.Iterate(userId);
        foreach (var val in results)
        {
            var message = new TLSavedMessage(val, 0, val.Length);
            var messagePts = message.AsSavedMessage().Pts;
            if (messagePts >= pts && messagePts <= maxPts &&
                message.AsSavedMessage().Get_OriginalMessage().AsMessage().Date <= date.ToUnixTimeSeconds())
            {
                messages.Add(message);
            }
        }
        messages = messages.OrderByDescending(m => 
            m.AsSavedMessage().Get_OriginalMessage().AsMessage().Id).ToList();
        return messages;
    }

    public async ValueTask<IReadOnlyCollection<TLSavedMessage>> GetMessagesAsync(long userId, int pts, int maxPts, DateTimeOffset date)
    {
        List<TLSavedMessage> messages = new();
        var results = _store.IterateAsync(userId);
        await foreach (var val in results)
        {
            var message = new TLSavedMessage(val, 0, val.Length);
            var messagePts = message.AsSavedMessage().Pts;
            if (messagePts >= pts && messagePts <= maxPts && 
                message.AsSavedMessage().Get_OriginalMessage().AsMessage().Date<= date.ToUnixTimeSeconds())
            {
                messages.Add(message);
            }
        }
        messages = messages.OrderByDescending(m => 
            m.AsSavedMessage().Get_OriginalMessage().AsMessage().Id).ToList();
        return messages;
    }

    public TLSavedMessage? GetMessage(long userId, int messageId)
    {
        var data = _store.GetBySecondaryIndex("by_id", userId, messageId);
        if (data == null)
        {
            return null;
        }
        return new TLSavedMessage(data, 0, data.Length);
    }

    public async ValueTask<TLSavedMessage?> GetMessageAsync(long userId, int messageId)
    {
        var data = await _store.GetBySecondaryIndexAsync("by_id", userId, messageId);
        if (data == null)
        {
            return null;
        }
        return new TLSavedMessage(data, 0, data.Length);
    }

    public bool DeleteMessage(long userId, int id)
    {
        return _store.DeleteBySecondaryIndex("by_id", userId, id);
    }

    public async ValueTask<bool> DeleteMessageAsync(long userId, int id)
    {
        return await _store.DeleteBySecondaryIndexAsync("by_id", userId, id);
    }
    
    private static long GetPeerId(TLInputPeer p) => p.Type switch
    {
        TLInputPeer.InputPeerType.InputPeerChat => p.AsInputPeerChat().ChatId,
        TLInputPeer.InputPeerType.InputPeerUser => p.AsInputPeerUser().UserId,
        TLInputPeer.InputPeerType.InputPeerChannel => p.AsInputPeerChannel().ChannelId,
        TLInputPeer.InputPeerType.InputPeerUserFromMessage => p.AsInputPeerUserFromMessage().UserId,
        TLInputPeer.InputPeerType.InputPeerChannelFromMessage => p.AsInputPeerChannelFromMessage().ChannelId,
        _ => 0
    };
}