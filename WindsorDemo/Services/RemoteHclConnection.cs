using System;
using System.Threading;
using System.Threading.Tasks;
using WindsorDemo.Interfaces;

namespace WindsorDemo.Services
{
    public class RemoteHclConnection : IRemoteHclConnection
    {
        private IWcfClientService WcfClientService { get; set; }
        private IJsonRpcClientService JsonRpcClientService { get; set; }

        public RemoteHclConnection(IWcfClientService wcfClientService, IJsonRpcClientService jsonRpcClientService)
        {
            this.WcfClientService = wcfClientService;
            this.JsonRpcClientService = jsonRpcClientService;
        }

        private T InvokeWithErrorHandling<T>(Func<IHypervisorCommunicationsLibraryInterface, T> operation, Guid opId)
        {
            try
            {
                Console.WriteLine($"{GetType().Name} InvokeWithErrorHandling called with opId: {opId}");
                var result = WcfClientService.Call<T>(operation);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private T InvokeWithErrorHandling<T>(Func<IRemoteHcl, CancellationToken, Task<T>> operation, CancellationToken token,  Guid opId)
        {
            try
            {
                Console.WriteLine($"{GetType().Name} InvokeWithErrorHandling for IRemoteHcl called with opId: {opId}");
                var result = JsonRpcClientService.CallAsync<T>(operation,token).GetAwaiter().GetResult();
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void CallAction(Action<IHypervisorCommunicationsLibraryInterface> action, Guid? opId)
        {
            opId = opId ?? Guid.NewGuid();
            InvokeWithErrorHandling(
                (hcl) =>
                {
                    action(hcl);
                    return string.Empty;
                }, opId.Value);
        }

        public T CallFunction<T>(Func<IHypervisorCommunicationsLibraryInterface, T> function, Guid? opId)
        {
            opId = opId ?? Guid.NewGuid();
            return InvokeWithErrorHandling(function, opId.Value);
        }

        public void CallAction(Action<IRemoteHcl> action, CancellationToken? cancellationToken, Guid? opId)
        {
            opId = opId ?? Guid.NewGuid();
            cancellationToken = cancellationToken ?? CancellationToken.None;
            InvokeWithErrorHandling<string>(
                (hcl, token) =>
                {
                    action(hcl);
                    return null;
                }, cancellationToken.Value, opId.Value);

        }

        public T CallFunction<T>(Func<IRemoteHcl, CancellationToken, Task<T>> function, CancellationToken? cancellationToken, Guid? opId)
        {
            opId = opId ?? Guid.NewGuid();
            cancellationToken = cancellationToken ?? CancellationToken.None;
            return InvokeWithErrorHandling<T>(function, cancellationToken.Value, opId.Value);
        }
    }
}
