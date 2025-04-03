using Newtonsoft.Json;
using StreamJsonRpc;
using System.Net.WebSockets;

namespace JsonRpcServer.CommonJsonRpc;

/// <summary>
///     JsonRpc base web socket
/// </summary>
public class WsJsonRpc
{
    private readonly ILogger _logger;
    private CancellationToken _cancellationToken;
    private IJsonRpcMessageHandler _handler;
    private HclJsonRpc _hclJsonRpc;
    private WebSocket _webSocket;

    public WsJsonRpc(ILogger logger = null, CancellationToken cancellation = default)
    {
        _logger = logger;
        _cancellationToken = cancellation;
    }

    public Task Completion
    {
        get
        {
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
            return _hclJsonRpc.Completion;
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
        }
    }


    public async Task<T> AttachService<T>(WebSocket wss, IEnumerable<object>? targets = null) where T : class
    {
        _webSocket = wss;
        var messageHandler = new WebSocketMessageHandler(wss, new JsonMessageFormatter
        {
            JsonSerializer =
            {
                TypeNameHandling = TypeNameHandling.All,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
                // Converters = { new () } // Add custom converters
            }
        });
        _hclJsonRpc = new HclJsonRpc(messageHandler);
        foreach (var target in targets ?? [])
        {
            _hclJsonRpc.AddLocalRpcTarget(target);
        }

        var service = _hclJsonRpc.Attach<T>();
        _hclJsonRpc.StartListening();
        return await Task.FromResult(service);

        // _hclJsonRpc.StartListening();

        // using (var jsonRpc = new JsonRpc( new WebSocketMessageHandler(socket , messageFormatter ), _greeterServer ))
        // {
        //
        //     jsonRpc.CancelLocallyInvokedMethodsWhenConnectionIsClosed = true;
        //     jsonRpc.StartListening();
        //     await jsonRpc.Completion;
        //     _logger.LogInformation("Greeter client disconnected");
        // }
        //
        // return new EmptyResult();
    }
}
