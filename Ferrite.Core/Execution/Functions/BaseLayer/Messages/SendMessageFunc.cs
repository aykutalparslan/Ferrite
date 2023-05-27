using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.slim;

namespace Ferrite.Core.Execution.Functions.BaseLayer.Messages;

public class SendMessageFunc : ITLFunction
{
    private readonly IMessagesService _messages;

    public SendMessageFunc(IMessagesService messages)
    {
        _messages = messages;
    }
    public async ValueTask<TLBytes?> Process(TLBytes q, TLExecutionContext ctx)
    {
        var result = await _messages.SendMessage(ctx.AuthKeyId, q);
        return RpcResultGenerator.Generate(result, ctx.MessageId);
    }
}