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

using System.Security.Cryptography;
using Autofac.Extras.Moq;
using Cassandra;
using Ferrite.Data;
using Ferrite.Data.Repositories;
using Moq;
using Xunit;

namespace Ferrite.Tests.Data;

public class CassandraKVStoreTests
{
    [Fact]
    public void Constructor_Should_Generate_CreateSchemaQuery()
    {
        List<string> queriesGenerated = new List<string>();
        var ctx = new Mock<ICassandraContext>();
        ctx.Setup(x => x.Execute(It.IsAny<Statement>())).Callback((Statement s) =>
        {
            if (s is SimpleStatement simpleStatement)
            {
                queriesGenerated.Add(simpleStatement.QueryString);
            }
        });

        CassandraKVStore store = new CassandraKVStore(ctx.Object,
            new TableDefinition("ferrite","messages",
            new KeyDefinition("pk", 
                new DataColumn{Name = "user_id", Type = DataType.Long},
                new DataColumn{Name = "peer_type", Type = DataType.Int},
                new DataColumn{Name = "peer_id", Type = DataType.Long},
                new DataColumn{Name = "outgoing", Type = DataType.Bool},
                new DataColumn{Name = "message_id", Type = DataType.Int},
                new DataColumn{Name = "pts", Type = DataType.Int},
                new DataColumn{Name = "date", Type = DataType.Long}),
            new KeyDefinition("id", 
                new DataColumn{Name = "user_id", Type = DataType.Long},
                new DataColumn{Name = "message_id", Type = DataType.Int})));
        Assert.Equal(2, queriesGenerated.Count);
        Assert.Equal("CREATE TABLE IF NOT EXISTS ferrite.messages (user_id bigint, peer_type int, " +
                     "peer_id bigint, outgoing boolean, message_id int, pts int, date bigint, messages_data blob, " +
                     "PRIMARY KEY (user_id, peer_type, peer_id, outgoing, message_id, pts, date));",
            queriesGenerated[0]);
        Assert.Equal("CREATE TABLE IF NOT EXISTS ferrite.messages_id (user_id bigint, message_id int, " +
                     "pk_user_id bigint, pk_peer_type int, pk_peer_id bigint, pk_outgoing boolean, pk_message_id int, " +
                     "pk_pts int, pk_date bigint, PRIMARY KEY (user_id, message_id));",
            queriesGenerated[1]);
    }
    [Fact]
    public void Put_Should_Generate_UpdateQuery()
    {
        List<string> queriesGenerated = new List<string>();
        var ctx = new Mock<ICassandraContext>();
        ctx.Setup(x => x.Enqueue(It.IsAny<Statement>())).Callback((Statement s) =>
        {
            if (s is SimpleStatement simpleStatement)
            {
                queriesGenerated.Add(simpleStatement.QueryString);
            }
        });

        CassandraKVStore store = new CassandraKVStore(ctx.Object,
            new TableDefinition("ferrite","messages",
            new KeyDefinition("pk", 
                new DataColumn{Name = "user_id", Type = DataType.Long},
                new DataColumn{Name = "peer_type", Type = DataType.Int},
                new DataColumn{Name = "peer_id", Type = DataType.Long},
                new DataColumn{Name = "outgoing", Type = DataType.Bool},
                new DataColumn{Name = "message_id", Type = DataType.Int},
                new DataColumn{Name = "pts", Type = DataType.Int},
                new DataColumn{Name = "date", Type = DataType.Long}),
            new KeyDefinition("id", 
                new DataColumn{Name = "user_id", Type = DataType.Long},
                new DataColumn{Name = "message_id", Type = DataType.Int})));
        store.Put(RandomNumberGenerator.GetBytes(16), 123L, 1, 222L, true, 55, 112, 0L);
        Assert.Equal(2, queriesGenerated.Count);
        Assert.Equal("UPDATE ferrite.messages SET messages_data = ? WHERE user_id = ? AND peer_type = ? " +
                     "AND peer_id = ? AND outgoing = ? AND message_id = ? AND pts = ? AND date = ?",
            queriesGenerated[0]);
        Assert.Equal("UPDATE ferrite.messages_id SET pk_user_id = ?, pk_peer_type = ?, pk_peer_id = ?, " +
                     "pk_outgoing = ?, pk_message_id = ?, pk_pts = ?, pk_date = ? WHERE user_id = ? AND message_id = ?",
            queriesGenerated[1]);
    }

    [Fact]
    public void Delete_Should_Generate_Query()
    {
        List<string> queriesGenerated = new List<string>();
        var ctx = new Mock<ICassandraContext>();
        ctx.Setup(x => x.Enqueue(It.IsAny<Statement>())).Callback((Statement s) =>
        {
            if (s is SimpleStatement simpleStatement)
            {
                queriesGenerated.Add(simpleStatement.QueryString);
            }
        });

        CassandraKVStore store = new CassandraKVStore(ctx.Object,
            new TableDefinition("ferrite", "messages",
                new KeyDefinition("pk",
                    new DataColumn { Name = "user_id", Type = DataType.Long },
                    new DataColumn { Name = "peer_type", Type = DataType.Int },
                    new DataColumn { Name = "peer_id", Type = DataType.Long },
                    new DataColumn { Name = "outgoing", Type = DataType.Bool },
                    new DataColumn { Name = "message_id", Type = DataType.Int },
                    new DataColumn { Name = "pts", Type = DataType.Int },
                    new DataColumn { Name = "date", Type = DataType.Long }),
                new KeyDefinition("id",
                    new DataColumn { Name = "user_id", Type = DataType.Long },
                    new DataColumn { Name = "message_id", Type = DataType.Int })));
        store.Delete(123L, 1, 222L, true, 55, 112, 0L);
        Assert.Equal(1, queriesGenerated.Count);
        Assert.Equal("DELETE FROM ferrite.messages WHERE user_id = ? AND peer_type = ? AND peer_id = ? " +
                     "AND outgoing = ? AND message_id = ? AND pts = ? AND date = ?",
            queriesGenerated[0]);
    }
}