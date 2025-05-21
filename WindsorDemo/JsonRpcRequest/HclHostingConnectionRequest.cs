using WindsorDemo.Interfaces;

namespace WindsorDemo.JsonRpcRequest
{
    public class HclHostingConnectionRequest: HclBasePluginRequest
    {
        public IConnectionDetail ConnectionDetails { get; set; }
    }
}
