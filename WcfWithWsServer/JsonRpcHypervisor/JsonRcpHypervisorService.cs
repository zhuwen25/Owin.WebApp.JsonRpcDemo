using HypervisorCreator;
using Owin.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace WcfWithWsServer.JsonRpcHypervisor;

public class JsonRcpHypervisorService
{
    private static readonly Lazy<JsonRcpHypervisorService> LazyInstance = new Lazy<JsonRcpHypervisorService>(() => new JsonRcpHypervisorService());
    private readonly IHypervisorFactory _hypervisorFactory = new HypervisorFactory();
    private object lockObject = new object();
    protected ConcurrentDictionary<string, JsonRpcHypervisor> _hypervisors = new ConcurrentDictionary<string, JsonRpcHypervisor>();

    public static JsonRcpHypervisorService Instance { get { return LazyInstance.Value; } }

    private JsonRcpHypervisorService()
    {
    }
    public async Task WebsocketOpenedAsync(WebSocket webSocket, string remoteIp, int? remotePort)
    {
        Console.WriteLine($"WebSocket opened: {remoteIp}:{remotePort}");
        var identitier =  $"{remoteIp}:{remotePort}";
        JsonRpcHypervisor rpcHypervisor = new JsonRpcHypervisor(webSocket, _hypervisorFactory,identitier);
        lock (lockObject)
        {

            //rpcHypervisor = _hypervisors.GetOrAdd(identitier, new JsonRpcHypervisor(webSocket, _hypervisorFactory,identitier));
        }


        await (rpcHypervisor?.HandleRpcSessionAsync(s =>
        {
            Console.WriteLine($"Session closed: {s}");
            lock (lockObject)
            {
                //_hypervisors.TryRemove(s, out _);
            }
        })).ConfigureAwait(false);
    }

    public void WebsocketClosed(WebSocketConnection WebSocket, string remoteIp, int? remotePort)
    {
        lock (lockObject)
        {
            var identitier =  $"{remoteIp}:{remotePort}";
            _hypervisors.TryRemove(identitier, out _);
        }
    }
}
