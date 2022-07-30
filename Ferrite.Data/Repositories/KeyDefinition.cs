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

using System.Collections.Immutable;
using Nest;

namespace Ferrite.Data.Repositories;

public class KeyDefinition
{
    public readonly string Name;
    public readonly ImmutableList<DataColumn> Columns;
    private readonly ImmutableDictionary<string, int> _colsIndex;

    public DataColumn this[int index] => Columns[index];
    public DataColumn this[string name] => Columns[_colsIndex[name]];
    public int GetOrdinal(string name) => _colsIndex[name];

    public KeyDefinition(string name, params DataColumn[] args)
    {
        Name = name;
        Columns = ImmutableList.Create(args);
        var bld = ImmutableDictionary.CreateBuilder<string, int>();
        for (int i = 0; i < args.Length; i++)
        {
            bld.Add(args[i].Name, i);
        }

        _colsIndex = bld.ToImmutable();
    }
}