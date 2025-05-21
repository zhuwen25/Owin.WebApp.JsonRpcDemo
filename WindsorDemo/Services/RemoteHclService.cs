using System;
using System.Threading;
using System.Threading.Tasks;
using WindsorDemo.Interfaces;
using WindsorDemo.JsonRpcRequest;

namespace WindsorDemo
{
    public class RemoteHclService : IRemoteHcl
    {
        private readonly Lazy<IHypervisor> _lazyHypervisor;
        public IHypervisor Hypervisor => _lazyHypervisor.Value;


        public RemoteHclService()
        {
            _lazyHypervisor = new Lazy<IHypervisor>(() => HypervisorFactory.CreateHypervisor("Azure",new AzureConnectionDetail()));
        }

        public async Task<string> SayHelloAsync(HclGreetRequest request,  CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"{GetType().Name}: SayHelloAsync called with name: {request.FromUser}");
            return await Task.FromResult( Hypervisor.SayHello(request.ConnectionDetails,request.FromUser));
        }

        public async Task<int> GetStatusAsync(HclStatusRequest request, CancellationToken cancellationToken = default)
        {
            var hostConnection = request.ConnectionDetails;
            Console.WriteLine($"{GetType().Name}: GetStatusAsync ");
            return await Task.FromResult(Hypervisor.GetStatus(hostConnection,request.Status));
        }

        public async Task<JsonRpcResponse<string>> GetVersionAsync(HclVersionRequest request , CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"{GetType().Name}: GetVersionAsync ");
            var res = Hypervisor.GetVersion(request.ConnectionDetails, request.InVersion);
            return await Task.FromResult(new JsonRpcResponse<string>(res));
        }

        public async Task<JsonRpcResponse<HclIHypervisorObjectSettings>> GetHypervisorObjectSettingsAsync(HclBasePluginRequest req, CancellationToken cancellationToken = default)
        {
            var val1 = Hypervisor.MaxOperationInProgressDefault;
            var response = new HclIHypervisorObjectSettings
            {
                MaxOperationInProgressDefault = val1,
            };
            return await Task.FromResult(new JsonRpcResponse<HclIHypervisorObjectSettings>(response));
        }

        public async Task<JsonRpcResponse<HypervisorCapability[]>> GetCapabilitiesAsync(HclBasePluginRequest req, CancellationToken cancellationToken = default)
        {
            var capabilities = Hypervisor.GetCapabilities(req.PluginName);

            return await Task.FromResult(new JsonRpcResponse<HypervisorCapability[]>(capabilities));
        }
    }
}
