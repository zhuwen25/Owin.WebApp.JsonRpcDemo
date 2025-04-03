﻿using StreamJsonRpc;
using StreamJsonRpc.Protocol;
using StreamJsonRpc.Reflection;

namespace JsonRpcServer.CommonJsonRpc;

public class HclJsonRpc : JsonRpc, IJsonRpcTracingCallbacks
{
    public HclJsonRpc(IJsonRpcMessageHandler messageHandler, object? target = null) : base(messageHandler, target)
    {
    }

    void IJsonRpcTracingCallbacks.OnMessageDeserialized(JsonRpcMessage message, object encodedMessage)
    {
        Console.WriteLine($"Server: OnMessageDeserialized message:{encodedMessage}");
    }

    void IJsonRpcTracingCallbacks.OnMessageSerialized(JsonRpcMessage message, object encodedMessage)
    {
        Console.WriteLine($"Server OnMessageSerialized message:{message}");
    }

    protected override ValueTask SendAsync(JsonRpcMessage message, CancellationToken cancellationToken)
    {
        return base.SendAsync(message, cancellationToken);
    }

    protected override ValueTask<JsonRpcMessage> DispatchRequestAsync(JsonRpcRequest request, TargetMethod targetMethod,
        CancellationToken cancellationToken)
    {
        return base.DispatchRequestAsync(request, targetMethod, cancellationToken);
    }

    protected override Type? GetErrorDetailsDataType(JsonRpcError error)
    {
        var ret = base.GetErrorDetailsDataType(error);
        if (ret != typeof(Exception))
        {
            return ret;
        }
        // if (error.Error?.Data is not null && error.Error?.Data?.ToString().IndexOf(Exception., StringComparison.Ordinal) > -1)
        // {
        //     return typeof(FaultException);
        // }

        return ret;
    }
}
