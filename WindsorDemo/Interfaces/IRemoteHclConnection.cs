using System;
using System.Threading;
using System.Threading.Tasks;

namespace WindsorDemo.Interfaces
{
    public interface IRemoteHclConnection
    {
        void CallAction(Action<IHypervisorCommunicationsLibraryInterface> action, Guid? opId);
        T CallFunction<T>(Func<IHypervisorCommunicationsLibraryInterface, T> function, Guid? opId);

        void CallAction(Action<IRemoteHcl> action, CancellationToken? cancellationToken, Guid? opId);

        T CallFunction<T>(Func<IRemoteHcl, CancellationToken, Task<T>> function, CancellationToken? cancellationToken, Guid? opId);
    }
}
