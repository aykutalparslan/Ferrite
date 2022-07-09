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

using Cassandra;
using MessagePack;

namespace Ferrite.Data.Repositories;

public class CassandraMessageRepository : IMessageRepository
{
    private readonly CassandraContext _context;
    public CassandraMessageRepository(CassandraContext context)
    {
        _context = context;
    }

    private void CreateSchema()
    {
        var statement = new SimpleStatement(
            "CREATE TABLE IF NOT EXISTS ferrite.messages (" +
            "user_id bigint," +
            "peer_type int," +
            "peer_id long," +
            "outgoing boolean," +
            "message_id int," +
            "message_data blob," +
            "date bigint," +
            "PRIMARY KEY (user_id, peer_type, peer_id, outgoing, message_id));");
        _context.Enqueue(statement);
        _context.ExecuteQueue();
        statement = new SimpleStatement(
            "CREATE TABLE IF NOT EXISTS ferrite.messages_by_id (" +
            "user_id bigint," +
            "message_id int," +
            "peer_type int," +
            "peer_id long," +
            "outgoing boolean," +
            "PRIMARY KEY (user_id, message_id));");
        _context.Enqueue(statement);
        _context.ExecuteQueue();
    }
    
    public bool PutMessage(MessageDTO message)
    {
        var data = MessagePackSerializer.Serialize(message);
        if (message.Out)
        {
            var statement = new SimpleStatement(
                "UPDATE ferrite.messages SET message_data = ?, date = ? " +
                "WHERE user_id = ? AND peer_type = ? AND peer_id = ? AND outgoing = ? AND message_id = ?;",
                data, DateTimeOffset.Now.ToUnixTimeSeconds(),
                message.FromId.PeerId, message.PeerId.PeerType, message.PeerId.PeerId, true, message.Id);
            _context.Enqueue(statement);
            var indexStatement = new SimpleStatement(
                "UPDATE ferrite.messages_by_id SET peer_type = ?, peer_id = ?, outgoing = ? " +
                "WHERE user_id = ? AND message_id = ?;",
                message.PeerId.PeerType, message.PeerId.PeerId, true, 
                message.FromId.PeerId, message.Id);
            _context.Enqueue(indexStatement);
        }
        else
        {
            var statement = new SimpleStatement(
                "UPDATE ferrite.messages SET message_data = ?, date = ? " +
                "WHERE user_id = ? AND peer_type = ? AND peer_id = ? AND outgoing = ? AND message_id = ?;",
                data, DateTimeOffset.Now.ToUnixTimeSeconds(),
                message.PeerId.PeerId, message.FromId.PeerType, message.FromId.PeerId, true, message.Id);
            _context.Enqueue(statement);
            var indexStatement = new SimpleStatement(
                "UPDATE ferrite.messages_by_id SET peer_type = ?, peer_id = ?, outgoing = ? " +
                "WHERE user_id = ? AND message_id = ?;",
                message.FromId.PeerType, message.FromId.PeerId, true, 
                message.PeerId.PeerId, message.Id);
            _context.Enqueue(indexStatement);
        }

        return true;
    }

    public IReadOnlyCollection<MessageDTO> GetMessages(long userId, PeerDTO? peerId = null)
    {
        List<MessageDTO> messages = new List<MessageDTO>();
        if (peerId != null)
        {
            var statement = new SimpleStatement(
                "SELECT message_data FROM ferrite.messages " +
                "WHERE user_id = ? AND peer_type = ? AND peer_id = ?;",
                userId, peerId?.PeerType, peerId?.PeerId);
            var results = _context.Execute(statement);
            foreach (var row in results)
            {
                var message = MessagePackSerializer.Deserialize<MessageDTO>(row.GetValue<byte[]>("message_data"));
                messages.Add(message);
            }
        }
        else
        {
            var statement = new SimpleStatement(
                "SELECT message_data FROM ferrite.messages " +
                "WHERE user_id = ?;",
                userId);
            var results = _context.Execute(statement);
            foreach (var row in results)
            {
                var message = MessagePackSerializer.Deserialize<MessageDTO>(row.GetValue<byte[]>("message_data"));
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
            var statement = new SimpleStatement(
                "SELECT message_data FROM ferrite.messages " +
                "WHERE user_id = ? AND peer_type = ? AND peer_id = ?;",
                userId, peerId?.PeerType, peerId?.PeerId);
            var results = await _context.ExecuteAsync(statement);
            foreach (var row in results)
            {
                var message = MessagePackSerializer.Deserialize<MessageDTO>(row.GetValue<byte[]>("message_data"));
                messages.Add(message);
            }
        }
        else
        {
            var statement = new SimpleStatement(
                "SELECT message_data FROM ferrite.messages " +
                "WHERE user_id = ?;",
                userId);
            var results = await _context.ExecuteAsync(statement);
            foreach (var row in results)
            {
                var message = MessagePackSerializer.Deserialize<MessageDTO>(row.GetValue<byte[]>("message_data"));
                messages.Add(message);
            }
        }
        return messages;
    }

    public MessageDTO? GetMessage(long userId, int messageId)
    {
        var statement = new SimpleStatement(
            "SELECT * FROM ferrite.messages_by_id " +
            "WHERE user_id = ? AND message_id = ?;",
            userId, messageId);
        var results = _context.Execute(statement);
        var row = results.FirstOrDefault();
        if (row == null)
        {
            return null;
        }
        var peerType = row.GetValue<int>("peer_type");
        var peerId = row.GetValue<long>("peer_id");
        var outgoing = row.GetValue<bool>("outgoing");
        var messageStatement = new SimpleStatement(
            "SELECT * FROM ferrite.messages " +
            "WHERE user_id = ? AND peer_type = ? AND peer_id = ? AND outgoing = ? AND message_id = ?;",
            userId, peerType, peerId, outgoing, messageId);
        var results2 = _context.Execute(messageStatement);
        var messageRow = results2.FirstOrDefault();
        if (messageRow == null)
        {
            return null;
        }
        var message = MessagePackSerializer.Deserialize<MessageDTO>(messageRow.GetValue<byte[]>("message_data"));
        return message;
    }

    public async ValueTask<MessageDTO> GetMessageAsync(long userId, int messageId)
    {
        var statement = new SimpleStatement(
            "SELECT * FROM ferrite.messages_by_id " +
            "WHERE user_id = ? AND message_id = ?;",
            userId, messageId);
        var results = await _context.ExecuteAsync(statement);
        var row = results.FirstOrDefault();
        if (row == null)
        {
            return null;
        }
        var peerType = row.GetValue<int>("peer_type");
        var peerId = row.GetValue<long>("peer_id");
        var outgoing = row.GetValue<bool>("outgoing");
        var messageStatement = new SimpleStatement(
            "SELECT * FROM ferrite.messages " +
            "WHERE user_id = ? AND peer_type = ? AND peer_id = ? AND outgoing = ? AND message_id = ?;",
            userId, peerType, peerId, outgoing, messageId);
        var results2 = await _context.ExecuteAsync(messageStatement);
        var messageRow = results2.FirstOrDefault();
        if (messageRow == null)
        {
            return null;
        }
        var message = MessagePackSerializer.Deserialize<MessageDTO>(messageRow.GetValue<byte[]>("message_data"));
        return message;
    }

    public bool DeleteMessage(long userId, int id)
    {
        var statement = new SimpleStatement(
            "SELECT * FROM ferrite.messages_by_id " +
            "WHERE user_id = ? AND message_id = ?;",
            userId, id);
        var results = _context.Execute(statement);
        var row = results.FirstOrDefault();
        if (row == null)
        {
            return false;
        }
        var peerType = row.GetValue<int>("peer_type");
        var peerId = row.GetValue<long>("peer_id");
        var outgoing = row.GetValue<bool>("outgoing");
        var messageStatement = new SimpleStatement(
            "DELETE FROM ferrite.messages " +
            "WHERE user_id = ? AND peer_type = ? AND peer_id = ? AND outgoing = ? AND message_id = ?;",
            userId, peerType, peerId, outgoing, id);
        _context.Enqueue(messageStatement);
        var messageByIdStatement = new SimpleStatement(
            "DELETE FROM ferrite.messages_by_id " +
            "WHERE user_id = ? AND message_id = ?;",
            userId, id);
        _context.Enqueue(messageByIdStatement);
        _context.ExecuteQueue();
        return true;
    }

    public async ValueTask<bool> DeleteMessageAsync(long userId, int id)
    {
        var statement = new SimpleStatement(
            "SELECT * FROM ferrite.messages_by_id " +
            "WHERE user_id = ? AND message_id = ?;",
            userId, id);
        var results = await _context.ExecuteAsync(statement);
        var row = results.FirstOrDefault();
        if (row == null)
        {
            return false;
        }
        var peerType = row.GetValue<int>("peer_type");
        var peerId = row.GetValue<long>("peer_id");
        var outgoing = row.GetValue<bool>("outgoing");
        var messageStatement = new SimpleStatement(
            "DELETE FROM ferrite.messages " +
            "WHERE user_id = ? AND peer_type = ? AND peer_id = ? AND outgoing = ? AND message_id = ?;",
            userId, peerType, peerId, outgoing, id);
        _context.Enqueue(messageStatement);
        var messageByIdStatement = new SimpleStatement(
            "DELETE FROM ferrite.messages_by_id " +
            "WHERE user_id = ? AND message_id = ?;",
            userId, id);
        _context.Enqueue(messageByIdStatement);
        await _context.ExecuteQueueAsync();
        return true;
    }
}