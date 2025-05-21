using System;
using WindsorDemo.Interfaces;

namespace WindsorDemo
{
    public class HypervisorCommunicationsLibraryService:IHypervisorCommunicationsLibraryInterface
    {
        private readonly Lazy<IHypervisor> _lazyHypervisor;
        public IHypervisor Hypervisor => _lazyHypervisor.Value;

        public HypervisorCommunicationsLibraryService()
        {
            _lazyHypervisor = new Lazy<IHypervisor>(() => HypervisorFactory.CreateHypervisor("Azure",new AzureConnectionDetail()));
        }
        private IConnectionDetail GetConnectionDetail(string hypervisorPlugin)
        {
            return new AzureConnectionDetail();
        }

        public string SayHello(string hypervisorPlugin, string name)
        {
            return Hypervisor.SayHello(GetConnectionDetail(hypervisorPlugin),$"{GetType().Name} says hello to {name}");
        }

        public int GetStatus(string hypervisorPlugin, int id)
        {
            Console.WriteLine($"{GetType().Name} GetStatus called with id: {id}");
            return Hypervisor.GetStatus(GetConnectionDetail(hypervisorPlugin), id);
        }

        public string GetVersion(string hypervisorPlugin, string versionIn)
        {
            Console.WriteLine($"{GetType().Name} GetVersion called with id: {versionIn}");
            return Hypervisor.GetVersion(GetConnectionDetail(hypervisorPlugin), versionIn);
        }

        public int MaxOperationInProgressDefault { get { return Hypervisor.MaxOperationInProgressDefault; } }
        public HypervisorCapability[] GetCapabilities(string hypervisorPlugin)
        {
            return Hypervisor.GetCapabilities(hypervisorPlugin);
        }
    }
}
