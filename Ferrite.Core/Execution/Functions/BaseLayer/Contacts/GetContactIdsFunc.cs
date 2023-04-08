using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.slim;
using VectorOfLong = Ferrite.TL.slim.VectorOfLong;

namespace Ferrite.Core.Execution.Functions.BaseLayer.Contacts;

public class GetContactIdsFunc : ITLFunction
{
    private readonly IContactsService _contacts;

    public GetContactIdsFunc(IContactsService contacts)
    {
        _contacts = contacts;
    }
    public async ValueTask<TLBytes?> Process(TLBytes q, TLExecutionContext ctx)
    {
        var contacts = await _contacts.GetContactIds(ctx.AuthKeyId, q);
        return RpcResultGenerator.Generate(ToVectorOfLong(contacts), ctx.MessageId);
    }
    
    private static TLBytes ToVectorOfLong(ICollection<long> collection)
    {
        VectorOfLong v = new ();
        foreach (var s in collection)
        {
            v.Append(s);
        }

        var vBytes = v.ToReadOnlySpan().ToArray();
        return new TLBytes(vBytes, 0, vBytes.Length);
    }
}