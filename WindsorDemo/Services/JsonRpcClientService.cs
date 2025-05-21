using System;
using System.Threading;
using System.Threading.Tasks;
using WindsorDemo.Interfaces;

namespace WindsorDemo.Services
{
    public class JsonRpcClientService: IJsonRpcClientService
    {
        private readonly IRemoteHcl _remoteHcl;

        public JsonRpcClientService(IRemoteHcl remoteHcl) {
            _remoteHcl = remoteHcl;
        }

        public async Task<T> CallAsync<T>(Func<IRemoteHcl, CancellationToken,Task<T>> operation, CancellationToken cancellationToken = default)
        {
            if (operation == null) {
                throw new ArgumentNullException(nameof(operation));
            }
            return await operation(_remoteHcl,cancellationToken);
        }
    }
}
