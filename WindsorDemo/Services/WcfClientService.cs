using System;
using WindsorDemo.Interfaces;

namespace WindsorDemo
{
    public class WcfClientService: IWcfClientService
    {
        private readonly IHypervisorCommunicationsLibraryInterface _hypervisorCommunicationsLibraryInterface;

        public WcfClientService(IHypervisorCommunicationsLibraryInterface hypervisorCommunicationsLibraryInterface)
        {
            _hypervisorCommunicationsLibraryInterface = hypervisorCommunicationsLibraryInterface;
        }

        public T Call<T>(Func<IHypervisorCommunicationsLibraryInterface, T> operation)
        {
            if (operation == null) {
                throw new ArgumentNullException(nameof(operation));
            }
            return  operation(_hypervisorCommunicationsLibraryInterface);
        }
    }
}
