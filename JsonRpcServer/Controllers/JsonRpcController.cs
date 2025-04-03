using JsonRpcContract;
using JsonRpcServer.CommonJsonRpc;
using Microsoft.AspNetCore.Mvc;
using Yz.AzureHypervisor;

namespace JsonRpcServer.Controllers;

[ApiController]
[Route("[controller]")]
public class JsonRpcController : ControllerBase
{
    private readonly ILogger<JsonRpcController> _logger;
    private readonly HypervisorService HypervisorService;

    public JsonRpcController(HypervisorService service, ILogger<JsonRpcController> logger)
    {
        HypervisorService = service;
        _logger = logger;
    }

    [Route("/rpc/hypervisor")]
    public async Task<IActionResult> InvokeHypervisor()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            return new BadRequestResult();
        }

        var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        var rpc = new WsJsonRpc(_logger);

        //var services = HypervisorFactory.Hypervisors.Select(object (x) => x.Value).ToList();
        //services.Add(new HypervisorFactory());

        var services = new List<object> { HypervisorService };
        await rpc.AttachService<IHypervisor>(socket, services);
        //Attach factory

        await rpc.Completion;
        return new EmptyResult();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        throw new NotSupportedException();
    }
}
