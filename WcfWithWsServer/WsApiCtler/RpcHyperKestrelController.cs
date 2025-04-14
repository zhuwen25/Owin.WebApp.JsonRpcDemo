#if NET
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using WcfWithWsServer.JsonRpcHypervisor;

namespace WcfWithWsServer.WsApiCtler;

[ApiController]
public class RpcHyperKestrelController: ControllerBase
{
    [HttpGet]
    public IActionResult TestingHttp()
    {
        return Ok("Hello from Kestrel");
    }

    [HttpGet("rpc/hypervisor2")]
    public async Task GetHypervisor()
    {
        // Simulate some processing
        var hypervisor = new { Name = "HyperKestrel", Version = "1.0" };

        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            string remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            int remotePort = HttpContext.Connection.RemotePort;
            var socket = await HttpContext.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
            // Start WebSocket session (handle messages)
            await JsonRcpHypervisorService.Instance.WebsocketOpenedAsync(socket, remoteIp, remotePort)
                .ConfigureAwait(false);
        }
        else
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
    }

}
#endif
