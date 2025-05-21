using System.Collections.Generic;

namespace WindsorDemo.JsonRpcRequest
{
    public class HclBaseRequest
    {
        public IDictionary<string, object> ExtensionData { get; set; } = new Dictionary<string, object>();
    }
}
