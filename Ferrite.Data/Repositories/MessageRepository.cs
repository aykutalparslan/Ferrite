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

using MessagePack;

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
    public bool PutMessage(long userId, MessageDTO message, int pts)
    {
        message.Pts = pts;
        int peerType = message.Out ? (int)message.PeerId.PeerType : (int)message.FromId.PeerType;
        long peerId = message.Out ? message.PeerId.PeerId : message.FromId.PeerId;
        var data = MessagePackSerializer.Serialize(message);
        _store.Put(data, userId, peerType, peerId,
            message.Out, message.Id, pts, DateTimeOffset.Now.ToUnixTimeSeconds());
        return true;
    }

    public IReadOnlyCollection<MessageDTO> GetMessages(long userId, PeerDTO? peerId = null)
    { 
        List<MessageDTO> messages = new List<MessageDTO>();
        if (peerId != null)
        {
            List<object> parameters = new List<object>();
            parameters.Add(userId);
            if (peerId != null)
            {
                parameters.Add((int)peerId.PeerType);
                parameters.Add(peerId.PeerId);
            }
            var results = _store.Iterate(parameters.ToArray());
            foreach (var val in results)
            {
                var message = MessagePackSerializer.Deserialize<MessageDTO>(val);
                messages.Add(message);
            }
        }
        else
        {
            var results = _store.Iterate(userId);
            foreach (var val in results)
            {
                var message = MessagePackSerializer.Deserialize<MessageDTO>(val);
                messages.Add(message);
            }
        }

        return messages;
    }

    public async ValueTask<IReadOnlyCollection<MessageDTO>> GetMessagesAsync(long userId, PeerDTO? peerId = null)
    {
        List<MessageDTO> messages = new List<MessageDTO>();
        if (peerId != null)
        {
            List<object> parameters = new List<object>();
            parameters.Add(userId);
            if (peerId != null)
            {
                parameters.Add((int)peerId.PeerType);
                parameters.Add(peerId.PeerId);
            }
            var results = _store.IterateAsync(parameters.ToArray());
            await foreach (var val in results)
            {
                var message = MessagePackSerializer.Deserialize<MessageDTO>(val);
                messages.Add(message);
            }
        }
        else
        {
            var results = _store.IterateAsync(userId);
            await foreach (var val in results)
            {
                try
                {
                    var message = MessagePackSerializer.Deserialize<MessageDTO>(val);
                    messages.Add(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        return messages;
    }

    public IReadOnlyCollection<MessageDTO> GetMessages(long userId, int pts, int maxPts, DateTimeOffset date)
    {
        List<MessageDTO> messages = new List<MessageDTO>();
        var results = _store.Iterate(userId);
        foreach (var val in results)
        {
            var message = MessagePackSerializer.Deserialize<MessageDTO>(val);
            if (message.Pts >= pts && message.Pts <= pts && message.Date <= date.ToUnixTimeSeconds())
            {
                messages.Add(message);
            }
        }
        return messages;
    }

    public async ValueTask<IReadOnlyCollection<MessageDTO>> GetMessagesAsync(long userId, int pts, int maxPts, DateTimeOffset date)
    {
        List<MessageDTO> messages = new List<MessageDTO>();
        var results = _store.IterateAsync(userId);
        await foreach (var val in results)
        {
            var message = MessagePackSerializer.Deserialize<MessageDTO>(val);
            if (message.Pts >= pts && message.Pts <= maxPts && message.Date <= date.ToUnixTimeSeconds())
            {
                messages.Add(message);
            }
        }
        return messages;
    }

    public MessageDTO? GetMessage(long userId, int messageId)
    {
        var data = _store.GetBySecondaryIndex("by_id", userId, messageId);
        if (data == null)
        {
            return null;
        }
        var message = MessagePackSerializer.Deserialize<MessageDTO>(data);
        return message;
    }

    public async ValueTask<MessageDTO> GetMessageAsync(long userId, int messageId)
    {
        var data = await _store.GetBySecondaryIndexAsync("by_id", userId, messageId);
        if (data == null)
        {
            return null;
        }
        var message = MessagePackSerializer.Deserialize<MessageDTO>(data);
        return message;
    }

    public bool DeleteMessage(long userId, int id)
    {
        return _store.DeleteBySecondaryIndex("by_id", userId, id);
    }

    public async ValueTask<bool> DeleteMessageAsync(long userId, int id)
    {
        return await _store.DeleteBySecondaryIndexAsync("by_id", userId, id);
    }
}