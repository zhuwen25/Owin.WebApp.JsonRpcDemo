using JsonRpcContract.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yz.AzureHypervisor;

namespace VmwareHypervisor;

public class VmwareHypervisor : IHypervisor
{
    public async Task<HelloResponse> SayHelloAsync(HelloRequest request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new HelloResponse { Message = "Hello from Vmware Hypervisor" });
    }

    public async Task<IConnectionDetail?> GetConnectionDetailAsync(string factoryName,
        CancellationToken cancellationToken)
    {
        return await Task.FromResult<IConnectionDetail?>(new VmwareConnectionDetail("vmUserName", "Vmware Password"));
    }

    public async Task<VMDiskInfo> GetVMDiskInfoAsync(IConnectionDetail connectionDetail,
        CancellationToken cancellationToken)
    {
        return await Task.FromResult(new VMDiskInfo
        {
            DiskSize = 200,
            DiskType = "HDD",
            DiskName = "VMWareDisk",
            DiskPath = $"storage.storage/Cluster.cluster/{Guid.NewGuid()}"
        });
    }
}
