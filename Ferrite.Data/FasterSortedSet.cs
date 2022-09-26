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

using FASTER.core;

namespace Ferrite.Data;

public class FasterSortedSet<T> where T: IComparable<T>
{
    private readonly FasterContext<string, SortedSet<T>> _context;
    private readonly string _name;

    public FasterSortedSet(FasterContext<string, SortedSet<T>> context, string name)
    {
        _context = context;
        _name = name;
    }
    
    public IReadOnlySet<T> Get()
    {
        var session = _context.Store.NewSession(new SortedSetFunctions<string, SortedSet<T>, T>(
            (set, l) =>
            {
                return set ??= new();
            }));
        session.Read(_name, out var result);
        return result ?? new SortedSet<T>();
    }

    public async ValueTask Add(T value)
    {
        var session = _context.Store.NewSession(new SortedSetFunctions<string, SortedSet<T>, T>(
            (set, l) =>
            {
                set ??= new();
                set.Add(l);
                return set;
            }));
        session.RMW(_name, value);
        await session.WaitForCommitAsync();
    }
    
    public async ValueTask Remove(T value)
    {
        var session = _context.Store.NewSession(new SortedSetFunctions<string, SortedSet<T>, T>(
            (set, l) =>
            {
                if (set == null)
                {
                    return null;
                }
                set.Remove(value);
                return set;
            }));
        session.RMW(_name, value);
        await session.WaitForCommitAsync();
    }
    
    public async ValueTask RemoveEqualOrLess(T value)
    {
        var session = _context.Store.NewSession(new SortedSetFunctions<string, SortedSet<T>, T>(
            (set, l) =>
            {
                if (set == null)
                {
                    return null;
                }
                while (set.Min.CompareTo(l) <= 0)
                {
                    set.Remove(set.Min);
                }
                return set;
            }));
        session.RMW(_name, value);
        await session.WaitForCommitAsync();
    }
    class SortedSetFunctions<Key, Value, Input> : FunctionsBase<Key, Value, Input, Value, Empty>
    {
        private readonly Func<Value, Input, Value> merger;
        public SortedSetFunctions() => merger = (l, r) => l;
        public SortedSetFunctions(Func<Value, Input, Value> merger) => this.merger = merger;
        /// <inheritdoc/>
        public override bool ConcurrentReader(ref Key key, ref Input input, ref Value value, ref Value dst, ref ReadInfo readInfo)
        {
            dst = value;
            return true;
        }

        /// <inheritdoc/>
        public override bool SingleReader(ref Key key, ref Input input, ref Value value, ref Value dst, ref ReadInfo readInfo)
        {
            dst = value;
            return true;
        }

        /// <inheritdoc/>
        public override bool InitialUpdater(ref Key key, ref Input input, ref Value value, ref Value output, ref RMWInfo rmwInfo){ output = value = merger(value, input); return true; }
        /// <inheritdoc/>
        public override bool CopyUpdater(ref Key key, ref Input input, ref Value oldValue, ref Value newValue, ref Value output, ref RMWInfo rmwInfo) { output = newValue = merger(oldValue, input); return true; }
        /// <inheritdoc/>
        public override bool InPlaceUpdater(ref Key key, ref Input input, ref Value value, ref Value output, ref RMWInfo rmwInfo) { output = value = merger(value, input); return true; }
    }
}