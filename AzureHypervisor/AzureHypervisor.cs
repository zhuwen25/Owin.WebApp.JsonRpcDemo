using JsonRpcContract.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yz.AzureHypervisor;

namespace AzureHypervisor;

public class AzureHypervisor : IHypervisor
{
    public async Task<HelloResponse> SayHelloAsync(HelloRequest request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new HelloResponse { Message = "Hello from Azure Hypervisor" });
    }

    public async Task<IConnectionDetail?> GetConnectionDetailAsync(string factoryName,
        CancellationToken cancellationToken)
    {
        return await Task.FromResult<IConnectionDetail?>(new AzureConnectionDetail());
    }

    public async Task<VMDiskInfo> GetVMDiskInfoAsync(IConnectionDetail connectionDetail,
        CancellationToken cancellationToken)
    {
        return await Task.FromResult(new VMDiskInfo
        {
            DiskSize = 100,
            DiskType = "SSD",
            DiskName = "AzureDisk",
            DiskPath = $"resourceGroup/azureDisk/{Guid.NewGuid()}"
        });
    }
}
