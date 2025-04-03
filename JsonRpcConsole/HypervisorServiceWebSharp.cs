using HypervisorCreator;
using JsonRpcContract;
using JsonRpcContract.Contracts;
using JsonRpcServer;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Channels;
using WebSocketSharp;
using WebSocketSharp.Server;
using Yz.AzureHypervisor;
using WebSocket = WebSocketSharp.WebSocket;
using WebSocketState = WebSocketSharp.WebSocketState;


namespace JsonRpcConsole;

// public class WebSocketStream : Stream
// {
//     private readonly WebSocket _webSocket;
//     private readonly MemoryStream _receiveStream = new();
//
//     public WebSocketStream(WebSocket webSocket)
//     {
//         _webSocket = webSocket;
//     }
//
//     public override bool CanRead => true;
//     public override bool CanSeek => false;
//     public override bool CanWrite => true;
//     public override long Length => throw new NotSupportedException();
//
//     public override long Position
//     {
//         get => throw new NotSupportedException();
//         set => throw new NotSupportedException();
//     }
//
//     public override void Flush() => _receiveStream.Flush();
//     public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
//     public override void SetLength(long value) => throw new NotSupportedException();
//
//     // 🔹 Read from the stream (for JSON-RPC messages)
//     public override int Read(byte[] buffer, int offset, int count)
//     {
//         return _receiveStream.Read(buffer, offset, count);
//     }
//
//     // 🔹 Write JSON-RPC messages to WebSocket
//     public override void Write(byte[] buffer, int offset, int count)
//     {
//         throw new NotImplementedException();
//     }
//
//     public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
//     {
//         if (_webSocket.State == WebSocketState.Open)
//         {
//             _webSocket.SendAsync(new ArraySegment<byte>(buffer, offset, count), WebSocketMessageType.Text, true,
//                 cancellationToken);
//         }
//     }
//
//     // 🔹 Handle incoming WebSocket messages
//     public void OnMessage(byte[] data)
//     {
//         _receiveStream.Write(data, 0, data.Length);
//         _receiveStream.Position = 0; // Reset position for reading
//     }
// }

public class WebsharpWebSocketMessageHandler : IJsonRpcMessageHandler
{
    private readonly WebSocket _webSocket;

    private readonly Channel<JsonRpcMessage> _messageQueue = Channel.CreateUnbounded<JsonRpcMessage>();
    private readonly TaskCompletionSource<object?> _closedTcs = new();

    public WebsharpWebSocketMessageHandler(WebSocket webSocket)
    {
        _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
        _webSocket.OnMessage += (sender, args) =>
        {
            if (args.IsText)
            {
                try
                {
                    var byteArray = Encoding.UTF8.GetBytes(args.Data);
                    ReadOnlySequence<byte> buffer = new ReadOnlySequence<byte>(byteArray);
                    var jsonRpcMessage = Formatter.Deserialize(buffer);
                    _messageQueue.Writer.TryWrite(jsonRpcMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        };
        _webSocket.OnClose += (sender, e) => _messageQueue.Writer.Complete();
    }

    public async ValueTask<JsonRpcMessage?> ReadAsync(CancellationToken cancellationToken)
    {
        //throw new NotImplementedException();
        if (await _messageQueue.Reader.WaitToReadAsync(cancellationToken))
        {
            if (_messageQueue.Reader.TryRead(out var message))
            {
                return message;
            }
        }

        return null;
    }


    public async ValueTask WriteAsync(JsonRpcMessage jsonRpcMessage, CancellationToken cancellationToken)
    {
        // throw new NotImplementedException();
        if (_webSocket.ReadyState == WebSocketState.Open)
        {
             IBufferWriter<byte> bufferWriter = new ArrayBufferWriter<byte>();
             Formatter.Serialize(bufferWriter, jsonRpcMessage);
             byte[] byteArray=  ((ArrayBufferWriter<byte>)bufferWriter).WrittenSpan.ToArray();
             var msg = Encoding.UTF8.GetString(byteArray);
            // var msg =  JsonConvert.SerializeObject(jsonRpcMessage, new  { NullValueHandling = NullValueHandling.Ignore });
            _webSocket.SendAsync(msg, (b =>
            {
                Console.WriteLine($"send message: {b}");
            }));
        }

        await Task.CompletedTask;
    }


    public bool CanRead => true;
    public bool CanWrite => true;

    private static readonly Lazy<IJsonRpcMessageFormatter> LazyFormatter = new(() => new JsonMessageFormatter()
    {
        JsonSerializer =
        {
            TypeNameHandling = TypeNameHandling.All,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
        }
    });

    public IJsonRpcMessageFormatter Formatter => LazyFormatter.Value;
}

public class WebSharpServer : IDisposable
{
    private readonly WebSocketServer _server;
    public WebSharpServer(string uri, X509Certificate2 certificate)
    {
        _server = new WebSocketServer(uri) { SslConfiguration = { ServerCertificate = certificate } };
    }

    public void StartWebSocketServer(string path = "/rpc/hypervisor")
    {
        _server.AddWebSocketService<HypervisorServiceWebSharp>("/rpc/hypervisor");
        _server.Start();
    }

    public void Dispose()
    {
        _server.RemoveWebSocketService("/rpc/hypervisor");
        _server.Stop();
        Console.WriteLine("Disposing WebSharpServer");
    }
}

public class HypervisorServiceWebSharp : WebSocketBehavior, IHypervisor
{
    private readonly HypervisorFactory _factory = new();
    ConcurrentDictionary<string, HclWssJsonRpc> _clients = new();
    public HypervisorServiceWebSharp()
    {
    }

    protected override void OnOpen()
    {
        string seesionIdentifier = $"{Context.UserEndPoint.Address}:{Context.UserEndPoint.Port}";
        var rpcSession = new HclWssJsonRpc(new WebsharpWebSocketMessageHandler(Context.WebSocket), this);

        _clients.TryAdd(seesionIdentifier, rpcSession);
        rpcSession.StartListening();

        _ = HandleRpcSessionAsync(seesionIdentifier, rpcSession);
        Console.WriteLine($"Client {seesionIdentifier} disconnected");
    }

    private async Task HandleRpcSessionAsync(string sessionIdentifier, HclWssJsonRpc rpcSession)
    {
        try
        {
            await rpcSession.Completion;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client {sessionIdentifier} error: {ex.Message}");
        }
        finally
        {
            Console.WriteLine($"Client {sessionIdentifier} disconnected");
            _clients.TryRemove(sessionIdentifier, out _);
        }
    }


    protected override void OnClose(CloseEventArgs e)
    {
        // string seesionIdentifier = $"{Context.UserEndPoint.Address}:{Context.UserEndPoint.Port}";
        // if (_clients.TryRemove(seesionIdentifier, out var client))
        // {
        //     client.Dispose();
        // }
        Console.WriteLine($"Client disconnected");
       // Console.WriteLine($"Client {seesionIdentifier} disconnected");
    }
    public async Task<HelloResponse> SayHelloAsync(HelloRequest request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new HelloResponse { Message = $"Hello from {this.GetType().Name}" });
    }

    public async Task<IConnectionDetail?> GetConnectionDetailAsync(string factoryName,
        CancellationToken cancellationToken)
    {
        var connectionDetail = new AzureConnectionDetail();
        connectionDetail.FactoryName = factoryName; // factoryName is not used in AzureConnectionDetail Just for demo
        var hypervisor = await GetHypervisorAsync(connectionDetail, cancellationToken).ConfigureAwait(false);
        return await hypervisor.GetConnectionDetailAsync(factoryName, cancellationToken);
    }

    public async Task<VMDiskInfo> GetVMDiskInfoAsync(IConnectionDetail connectionDetail,
        CancellationToken cancellationToken)
    {
        var hypervisor = await GetHypervisorAsync(connectionDetail, cancellationToken).ConfigureAwait(false);
        return await hypervisor.GetVMDiskInfoAsync(connectionDetail, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IHypervisor> GetHypervisorAsync(IConnectionDetail connectionDetail,
        CancellationToken cancellationToken)
    {
        return await _factory.GetHypervisorAsync(connectionDetail, cancellationToken);
    }
}
