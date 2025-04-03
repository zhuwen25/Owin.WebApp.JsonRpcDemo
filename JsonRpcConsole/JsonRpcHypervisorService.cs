using Fleck;
using HypervisorCreator;
using JsonRpcContract;
using JsonRpcContract.Contracts;
using JsonRpcServer;
using System.Security.Cryptography.X509Certificates;
using Yz.AzureHypervisor;

namespace JsonRpcConsole;

public class JsonRpcHypervisorService : WebSocketServer, IHypervisor
{
    private readonly HypervisorFactory _factory = new();

    public JsonRpcHypervisorService(string uri, X509Certificate2? certificate2 = null, bool supportDualStack = true) :
        base(uri, supportDualStack)
    {
        Certificate = certificate2;

        Console.WriteLine("JsonRpcHypervisorService: Started");
    }

    public void Start()
    {
        Start(s =>
        {
            s.OnOpen = () => _ = OnClientConnected(s);
            s.OnClose = () => OnClientDisconnected(s);
            s.OnMessage = (data) => OnMessageReceived(s, data);
        });
    }

    private readonly IDictionary<string, HclWssJsonRpc> _clients = new Dictionary<string, HclWssJsonRpc>();

    private async Task OnClientConnected(IWebSocketConnection socket)
    {
        Console.WriteLine($"Client: OnClientConnected");
        var sessionIdentifier = $"{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}";
        var duplexPipe = new WebSocketDuplexPipeAdapter(socket);
        var rpc = new HclWssJsonRpc(duplexPipe);
        rpc.AddLocalRpcTarget(this);
        rpc.StartListening();
        _clients.Add(sessionIdentifier, rpc);
        // await duplexPipe.ProcessAsync();
        var rpcTask = rpc.Completion;
        var pipeProcessingTask = duplexPipe.ProcessAsync(); // Message processing task
        await Task.WhenAny(rpcTask, pipeProcessingTask);
        // Ensure both tasks finish completely before closing the connection
        await Task.WhenAll(rpcTask, pipeProcessingTask);

        Console.WriteLine($"Client: OnClientConnected: {sessionIdentifier}");
    }

    private void OnClientDisconnected(IWebSocketConnection socket)
    {
        Console.WriteLine($"Client: OnClientDisconnected");
        var sessionIdentifier = $"{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}";
        _clients.Remove(sessionIdentifier);
        Console.WriteLine($"Client: OnClientDisconnected removed: {sessionIdentifier}");
    }

    private void OnMessageReceived(IWebSocketConnection socket, string data)
    {
        Console.WriteLine($"Client: OnMessageReceived: {data}");
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
