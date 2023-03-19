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

using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.slim;
using Ferrite.TL.slim.layer150;

namespace Ferrite.Core.Execution.Functions.BaseLayer.Contacts;

public class GetStatusesFunc : ITLFunction
{
    private readonly IContactsService _contacts;

    public GetStatusesFunc(IContactsService contacts)
    {
        _contacts = contacts;
    }
    public async ValueTask<TLBytes?> Process(TLBytes q, TLExecutionContext ctx)
    {
        var statuses = await _contacts.GetStatuses(ctx.AuthKeyId);
        return RpcResultGenerator.Generate(ToVector(statuses), ctx.MessageId);
    }

    private static TLBytes ToVector(ICollection<TLContactStatus> statuses)
    {
        Vector v = new Vector();
        foreach (var s in statuses)
        {
            v.AppendTLObject(s.AsSpan());
        }

        var vBytes = v.ToReadOnlySpan().ToArray();
        return new TLBytes(vBytes, 0, vBytes.Length);
    }
}