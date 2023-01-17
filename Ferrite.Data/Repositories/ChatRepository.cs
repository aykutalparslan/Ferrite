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
using Ferrite.TL.slim.layer150;
using MessagePack;

namespace Ferrite.Data.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly IKVStore _store;
    public ChatRepository(IKVStore store)
    {
        _store = store;
        store.SetSchema(new TableDefinition("ferrite", "chats",
            new KeyDefinition("pk",
                new DataColumn { Name = "chat_id", Type = DataType.Long })));
    }
    public bool PutChat(TLBytes chat)
    {
        long chatId = chat.Constructor switch
        {
            Constructors.layer150_Chat => ((Chat)chat).Id,
            Constructors.layer150_ChatForbidden => ((ChatForbidden)chat).Id,
            Constructors.layer150_Channel => ((Channel)chat).Id,
            _ => 0
        };
        return _store.Put(chat.AsSpan().ToArray(), chatId);
    }

    public async ValueTask<TLBytes?> GetChatAsync(long chatId)
    {
        var chatBytes = await _store.GetAsync(chatId);
        if (chatBytes is { Length: > 0 })
        {
            return new TLBytes(chatBytes, 0, chatBytes.Length);
        }

        return null;
    }
}