using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.slim;

namespace Ferrite.Core.Execution.Functions.BaseLayer.Contacts;

public class DeleteContactsFunc : ITLFunction
{
    private readonly IContactsService _contacts;

    public DeleteContactsFunc(IContactsService contacts)
    {
        _contacts = contacts;
    }
    public async ValueTask<TLBytes?> Process(TLBytes q, TLExecutionContext ctx)
    {
        var result = await _contacts.DeleteContacts(ctx.AuthKeyId, q);
        return RpcResultGenerator.Generate(result, ctx.MessageId);
    }
}