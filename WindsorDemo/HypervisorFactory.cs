using WindsorDemo.Interfaces;
using WindsorDemo.Services;

namespace WindsorDemo
{
    public class HypervisorFactory
    {
        public static IHypervisor CreateHypervisor(string factoryName, IConnectionDetail connection)
        {
            return new AzureRmHypervisor();
        }
    }
}
