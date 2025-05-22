using Castle.DynamicProxy;
using Castle.MicroKernel;
using System;
using WindsorDemo.Interfaces;
using WindsorDemo.Services;

namespace WindsorDemo
{
    public class PluginHypervisorFactory : IRemoteHypervisorProxyFactory
    {
        private readonly IKernel _kernel;

        public PluginHypervisorFactory(IKernel kernel)
        {
            this._kernel = kernel;
        }
        public IRemoteHypervisor CreateRemoteHypervisor(IConnectionDetail connectionDetail)
        {

            if (connectionDetail != null)
            {
                Console.WriteLine($"Factory.GetHypervisor: ConnectionDetail type: {connectionDetail.GetType().FullName}");
            }

            var kernelArguments = new Arguments
            {
                { "connectionDetail", connectionDetail },
            };

            return _kernel.Resolve<IRemoteHypervisor>(kernelArguments);
        }
    }
}
