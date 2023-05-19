using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.slim;

namespace Ferrite.Core.Execution.Functions.BaseLayer.Messages;

public class SetTypingFunc : ITLFunction
{
    private readonly IMessagesService _messages;

    public SetTypingFunc(IMessagesService messages)
    {
        _messages = messages;
    }
    public async ValueTask<TLBytes?> Process(TLBytes q, TLExecutionContext ctx)
    {
        var result = await _messages.SetTyping(ctx.AuthKeyId, q);
        return RpcResultGenerator.Generate(result, ctx.MessageId);
    }
}