using System;
using WindsorDemo.Interfaces;

namespace WindsorDemo.Services
{
    public class PluginHypervisor: IRemoteHypervisor
    {
        private readonly IWcfClientService _wcfClientService;
        private readonly IJsonRpcClientService _jsonRpcClientService;
        private readonly IConnectionDetail _connectionDetail;

        public PluginHypervisor(IWcfClientService wcfClientService, IJsonRpcClientService jsonRpcClientService,IConnectionDetail connectionDetail)
        {
            this._wcfClientService = wcfClientService;
            this._jsonRpcClientService = jsonRpcClientService;
            this._connectionDetail = connectionDetail;
        }

        public string SayHello(IConnectionDetail connectionDetail, string name)
        {
            throw new System.NotImplementedException();
        }

        public int GetStatus(IConnectionDetail connectionDetail, int status)
        {
            throw new System.NotImplementedException();
        }

        public string GetVersion(IConnectionDetail connectionDetail, string inputVersion)
        {
            throw new System.NotImplementedException();
        }

        public HypervisorCapability[] GetCapabilities(string pluginName )
        {
            throw new System.NotImplementedException();
        }

        public int MaxOperationInProgressDefault { get { throw new NotImplementedException(); } }
    }
}
