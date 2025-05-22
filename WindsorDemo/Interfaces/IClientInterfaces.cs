using System;
using System.Threading;
using System.Threading.Tasks;
using WindsorDemo.JsonRpcRequest;

namespace WindsorDemo.Interfaces
{
    public enum HypervisorCapability
    {
        CreateVmWithNetworkDisconnected = 1,
        CreateVmWithNetworkConnected = 2,
    }

    public interface IHypervisor
    {
        string SayHello(IConnectionDetail connectionDetail, string name);
        int GetStatus(IConnectionDetail connectionDetail, int status);
        string GetVersion(IConnectionDetail connectionDetail, string inputVersion);

        HypervisorCapability[] GetCapabilities(string pluginName );

        int MaxOperationInProgressDefault { get; }

    }

    public interface IMachineManager
    {
        bool IsConnected(IConnectionDetail connectionDetail);
    }


//Below is the interface for the HypervisorCommunicationsLibraryInterface
//It is used to define for IHypervisor

    public interface IHypervisorCommunicationsLibraryInterface
    {
        string SayHello(string hypervisorPlugin,string name);
        int GetStatus(string hypervisorPlugin, int id);
        string GetVersion(string hypervisorPlugin, string versionIn);

        int MaxOperationInProgressDefault { get; }

        HypervisorCapability[] GetCapabilities(string hypervisorPlugin);
    }


    public class HclIHypervisorObjectSettings
    {
        public int MaxOperationInProgressDefault { get; set; }
    }



    public class JsonRpcResponse<T>
    {
        private T _response;

        public JsonRpcResponse(T response)
        {
            _response = response;
        }
        public T Response
        {
            get
            {
                return _response;
            }
            set
            {
                _response = value;
            }
        }
    }

    public interface IRemoteHcl
    {
        Task<string> SayHelloAsync( HclGreetRequest request, CancellationToken cancellationToken = default);
        Task<int> GetStatusAsync(HclStatusRequest request, CancellationToken cancellationToken = default);

        Task<JsonRpcResponse<string>> GetVersionAsync( HclVersionRequest request, CancellationToken cancellationToken = default);

        Task<JsonRpcResponse<HclIHypervisorObjectSettings>> GetHypervisorObjectSettingsAsync(HclBasePluginRequest req, CancellationToken cancellationToken = default);

        Task<JsonRpcResponse<HypervisorCapability[]>> GetCapabilitiesAsync(HclBasePluginRequest req, CancellationToken cancellationToken = default);

    }


    public interface IWcfClientService
    {
        T Call<T>(Func<IHypervisorCommunicationsLibraryInterface, T> operation);
    }

    public interface IJsonRpcClientService
    {
        Task<T> CallAsync<T>(Func<IRemoteHcl, CancellationToken,Task<T>> operation,CancellationToken cancellationToken = default);
    }
}
