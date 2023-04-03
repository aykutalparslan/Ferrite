using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.slim;

namespace Ferrite.Core.Execution.Functions.BaseLayer.Contacts;

public class SearchFunc : ITLFunction
{
    private readonly IContactsService _contacts;

    public SearchFunc(IContactsService contacts)
    {
        _contacts = contacts;
    }
    public async ValueTask<TLBytes?> Process(TLBytes q, TLExecutionContext ctx)
    {
        var result = await _contacts.Search(ctx.AuthKeyId, q);
        return RpcResultGenerator.Generate(result, ctx.MessageId);
    }
}