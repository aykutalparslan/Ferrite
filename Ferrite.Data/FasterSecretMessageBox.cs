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

public class FasterSecretMessageBox : ISecretMessageBox
{
    private readonly IAtomicCounter _counter;
    private readonly long _authKeyId;
    public FasterSecretMessageBox(FasterContext<string, long> counterContext, long authKeyId)
    {
        _authKeyId = authKeyId;
        _counter = new FasterCounter(counterContext , $"seq:qts:{authKeyId}");
    }
    public async ValueTask<int> Qts()
    {
        return (int)await _counter.Get();
    }

    public async ValueTask<int> IncrementQts()
    {
        int qts = (int)await _counter.IncrementAndGet();
        if (qts == 0)
        {
            qts = (int)await _counter.IncrementAndGet();
        }
        return qts;
    }
}