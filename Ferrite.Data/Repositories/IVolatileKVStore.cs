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

namespace Ferrite.Data.Repositories;

/// <summary>
/// Keeps the data in RAM only and never saves it in persistent storage.
/// </summary>
public interface IVolatileKVStore
{
    /// <summary>
    /// Sets the schema.
    /// </summary>
    /// <param name="table"></param>
    void SetSchema(TableDefinition table);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="Ttl">Time-to-live in milliseconds. Zero and negative values are treated as infinity.</param>
    /// <param name="keys"></param>
    public void Put(byte[] value, int Ttl, params object[] keys);
    public void Delete(params object[] keys);
    public bool Exists(params object[] keys);
    public byte[]? Get(params object[] keys);
}