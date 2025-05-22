using System;
using WindsorDemo.Interfaces;
using static WindsorDemo.Interfaces.HypervisorCapability;

namespace WindsorDemo
{
    public class AzureRmHypervisor: IHypervisor
    {
        public string SayHello(IConnectionDetail connectionDetail, string name)
        {
            return $"Hello { name} from {GetType().Name}";
        }

        public int GetStatus(IConnectionDetail connectionDetail, int status)
        {
            Console.WriteLine($"{GetType().Name}: GetStatusAsync called with id: {status}");
            return status;
        }

        public string GetVersion(IConnectionDetail connectionDetail, string inputVersion)
        {
            return $"{GetType().Name} GetVersionAsync called with inputVersion: {inputVersion}";
        }

        public HypervisorCapability[] GetCapabilities(string  pluinName)
        {
            Console.WriteLine($"{GetType().Name} with {pluinName}: GetCapabilitiesAsync called");

            return  new HypervisorCapability[]
            {
                CreateVmWithNetworkDisconnected,
                CreateVmWithNetworkConnected
            };
        }

        public int MaxOperationInProgressDefault { get { return 10; } }
    }
}
