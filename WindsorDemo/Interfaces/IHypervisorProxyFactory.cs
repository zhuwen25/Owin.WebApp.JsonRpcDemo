using Castle.MicroKernel;
using WindsorDemo.Services;

namespace WindsorDemo.Interfaces
{
    public interface IRemoteHypervisor:IHypervisor
    {
    }
    public interface IRemoteHypervisorProxyFactory
    {
        IRemoteHypervisor CreateRemoteHypervisor(IConnectionDetail connectionDetail);
    }

}
