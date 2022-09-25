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

namespace Ferrite.Data;

public class FasterUpdatesContextFactory : IUpdatesContextFactory
{
    private readonly FasterContext<string, long> _counterContext;
    private readonly FasterContext<string, SortedSet<long>> _unreadContext;
    private readonly FasterContext<string, SortedSet<string>> _dialogContex;
    public FasterUpdatesContextFactory(string path)
    {
        _counterContext = new FasterContext<string, long>(path + "-counter");
        _unreadContext = new FasterContext<string, SortedSet<long>>(path + "-unread");
        _dialogContex = new FasterContext<string, SortedSet<string>>(path + "-dialog");
    }
    public IUpdatesContext GetUpdatesContext(long? authKeyId, long userId)
    {
        return new FasterUpdatesContext(_counterContext, _unreadContext, _dialogContex,
            authKeyId, userId);
    }
}