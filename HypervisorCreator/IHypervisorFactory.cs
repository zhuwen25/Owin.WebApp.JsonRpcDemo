using JsonRpcContract.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Yz.AzureHypervisor;

namespace HypervisorCreator
{
    public interface IHypervisorFactory
    {
        Task<IHypervisor> GetHypervisorAsync(IConnectionDetail connectionDetail, CancellationToken cancellationToken);
    }
}
