using JsonRpcContract.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Yz.AzureHypervisor
{
    public interface IHypervisor
    {
        Task<HelloResponse> SayHelloAsync(HelloRequest request, CancellationToken cancellationToken);
        Task<IConnectionDetail> GetConnectionDetailAsync(string factoryName, CancellationToken cancellationToken);
        Task<VMDiskInfo> GetVMDiskInfoAsync(IConnectionDetail connectionDetail, CancellationToken cancellationToken);

    }
}
