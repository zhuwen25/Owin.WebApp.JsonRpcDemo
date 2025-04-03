using Newtonsoft.Json;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;
using StreamJsonRpc.Reflection;
using System.IO.Pipelines;
using System.Net.WebSockets;

namespace JsonRpcConsole;

public class HclWssJsonRpc : JsonRpc, IJsonRpcTracingCallbacks
{
    public HclWssJsonRpc(WebSocket ws, object? target = null) : base(
        new WebSocketMessageHandler(ws, new JsonMessageFormatter
        {
            JsonSerializer =
            {
                TypeNameHandling = TypeNameHandling.All,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
                // Converters = { new () } // Add custom converters
            }
        }), target)
    {
    }


    public HclWssJsonRpc(IDuplexPipe duplexPipe, object? target = null) : base(
        new LengthHeaderMessageHandler(duplexPipe, new JsonMessageFormatter
        {
            JsonSerializer =
            {
                TypeNameHandling = TypeNameHandling.All,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
                // Converters = { new () } // Add custom converters
            }
        }), target)
    {
    }


    public HclWssJsonRpc(IJsonRpcMessageHandler handler, object? target = null) : base(handler, target)
    {
    }

    void IJsonRpcTracingCallbacks.OnMessageDeserialized(JsonRpcMessage message, object encodedMessage)
    {
        Console.WriteLine($"{this.GetType().FullName}: Server: OnMessageDeserialized message:{encodedMessage}");
    }

    void IJsonRpcTracingCallbacks.OnMessageSerialized(JsonRpcMessage message, object encodedMessage)
    {
        Console.WriteLine($"{this.GetType().FullName}: Server OnMessageSerialized message:{message}");
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
