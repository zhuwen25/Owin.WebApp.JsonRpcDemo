using StreamJsonRpc.Protocol;

namespace JsonRpcServer;

public class HypervisorRpcDispatcher
{
    public async Task HandleRpcCallAsync(JsonRpcRequest request)
    {
        var method = request.Method;
        var parameters = request.Arguments;
    }
}
