
using Yz.AzureHypervisor;
using JsonRpcContract.Contracts;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HypervisorCreator
{
    public class HypervisorFactory : IHypervisorFactory
    {
        public static readonly ConcurrentDictionary<string, IHypervisor> Hypervisors = new()
        {
            //[IConnectionDetail.FactoryNameAzure] = new AzureHypervisor.AzureHypervisor(),
            //[IConnectionDetail.FactoryNameVmware] = new VmwareHypervisor.VmwareHypervisor()
        };

        public HypervisorFactory()
        {
            //_hypervisors = new ConcurrentDictionary<string, IHypervisor>();
            Hypervisors.TryAdd(IConnectionDetail.FactoryNameAzure, new AzureHypervisor.AzureHypervisor());
            Hypervisors.TryAdd(IConnectionDetail.FactoryNameVmware, new VmwareHypervisor.VmwareHypervisor());
        }

        public async Task<IHypervisor> GetHypervisorAsync(IConnectionDetail connectionDetail,
            CancellationToken cancellationToken)
        {
            return await Task.FromResult(GetOrCreateHypervisor(connectionDetail));
        }

        private static IHypervisor GetOrCreateHypervisor(IConnectionDetail connectionDetail)
        {
            lock (Hypervisors)
            {
                if (Hypervisors.TryGetValue(connectionDetail.FactoryName, out var hypervisor))
                {
                    return hypervisor;
                }

                IHypervisor createHypervisor = connectionDetail.FactoryName switch
                {
                    IConnectionDetail.FactoryNameAzure => new AzureHypervisor.AzureHypervisor(),
                    IConnectionDetail.FactoryNameVmware => new VmwareHypervisor.VmwareHypervisor(),
                    _ => throw new Exception("Invalid hypervisor name")
                };
                Hypervisors.TryAdd(connectionDetail.FactoryName, createHypervisor);
                return createHypervisor;
            }
        }
    }
}
