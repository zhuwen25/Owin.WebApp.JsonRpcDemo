using HypervisorCreator;
using JsonRpcContract;
using JsonRpcContract.Contracts;
using Yz.AzureHypervisor;

namespace JsonRpcServer;

public class HypervisorService(ILogger<HypervisorService> logger) : IHypervisor
{
    private readonly HypervisorFactory _factory = new();
    private ILogger<HypervisorService> _logger = logger;

    public async Task<HelloResponse> SayHelloAsync(HelloRequest request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new HelloResponse { Message = "Hello from HypervisorService" });
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
