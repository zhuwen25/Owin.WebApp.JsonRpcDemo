using HypervisorCreator;
using JsonRpcContract;
using JsonRpcContract.Contracts;
using JsonRpcServer;
using Yz.AzureHypervisor;

namespace JsonRpcConsole;

public class NetWebSocket_JsonRpcHypervisorService : IHypervisor
{
    private readonly HypervisorFactory _factory = new();
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
