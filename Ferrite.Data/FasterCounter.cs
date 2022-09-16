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

public class FasterCounter : IAtomicCounter
{
    private readonly FasterContext<string, long> _context;
    private string _name;
    private long _inc = 1;
    private bool _disposed = false;
    private readonly ClientSession<string, long, long, long, Empty, IFunctions<string, long, long, long, Empty>> _session;
    
    public FasterCounter(FasterContext<string, long> context, string name)
    {
        _context = context;
        _name = name;
        _session = _context.Store.NewSession(new RMWSimpleFunctions<string, long>(
            (a, b) => a + b));
        
    }
    
    public ValueTask<long> Get()
    {
        if (_disposed) throw new ObjectDisposedException(_name);
        long value = 0;
        _session.Read(ref _name, ref value);
        return ValueTask.FromResult(value);
    }

    public async ValueTask<long> IncrementAndGet()
    {
        if (_disposed) throw new ObjectDisposedException(_name);
        long value = 0;
        _session.RMW(ref _name, ref _inc, ref value);
        await _session.WaitForCommitAsync();
        return value;
    }

    public async ValueTask<long> IncrementByAndGet(long inc)
    {
        if (_disposed) throw new ObjectDisposedException(_name);
        long value = 0;
        _session.RMW(ref _name, ref inc, ref value);
        await _session.WaitForCommitAsync();
        return value;
    }

    
    class RMWSimpleFunctions<Key, Value> : SimpleFunctions<Key, Value>
    {
        public RMWSimpleFunctions(Func<Value, Value, Value> merger) : base(merger) { }

        public override bool InitialUpdater(ref Key key, ref Value input, ref Value value, ref Value output, ref RMWInfo rmwInfo)
        {
            base.InitialUpdater(ref key, ref input, ref value, ref output, ref rmwInfo);
            output = input;
            return true;
        }

        /// <inheritdoc/>
        public override bool CopyUpdater(ref Key key, ref Value input, ref Value oldValue, ref Value newValue, ref Value output, ref RMWInfo rmwInfo)
        {
            base.CopyUpdater(ref key, ref input, ref oldValue, ref newValue, ref output, ref rmwInfo);
            output = newValue;
            return true;
        }

        /// <inheritdoc/>
        public override bool InPlaceUpdater(ref Key key, ref Value input, ref Value value, ref Value output, ref RMWInfo rmwInfo)
        {
            base.InPlaceUpdater(ref key, ref input, ref value, ref output, ref rmwInfo);
            output = value; 
            return true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        _disposed = true;
    }
}