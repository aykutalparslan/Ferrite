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

public class FasterContext<Tkey, TValue> : IAsyncDisposable
{
    public FasterKV<Tkey, TValue> Store { get; }
    private bool _disposed = false;
    private readonly Task? _checkpointHybrid;
    private readonly Task? _checkpointFull;
    
    public FasterContext()
    {
        Store = new FasterKV<Tkey, TValue>(new FasterKVSettings<Tkey, TValue>(null)
        {
            TryRecoverLatest = true,
            RemoveOutdatedCheckpoints = true,
        });
        _checkpointHybrid = IssueHybridLogCheckpoints();
        _checkpointFull = IssueFullCheckpoints();
    }
    public FasterContext(string path)
    {
        Store = new FasterKV<Tkey, TValue>(new FasterKVSettings<Tkey, TValue>(path, deleteDirOnDispose: false)
        {
            TryRecoverLatest = true,
            RemoveOutdatedCheckpoints = true,
        });
        _checkpointHybrid = IssueHybridLogCheckpoints();
        _checkpointFull = IssueFullCheckpoints();
    }
    
    private async Task IssueHybridLogCheckpoints()
    {
        while (!_disposed)
        {
            await Task.Delay(100);
            (_, _) = Store.TakeHybridLogCheckpointAsync(CheckpointType.FoldOver).GetAwaiter().GetResult();
        }
    }
    private async Task IssueFullCheckpoints()
    {
        while (!_disposed)
        {
            await Task.Delay(1000);
            (_, _) = Store.TakeHybridLogCheckpointAsync(CheckpointType.FoldOver).GetAwaiter().GetResult();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        await _checkpointHybrid;
        await _checkpointFull;
    }
}