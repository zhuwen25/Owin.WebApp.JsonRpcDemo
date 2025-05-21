using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WindsorDemo.Interfaces;

namespace WindsorDemo.Services
{
    public class HclJsonRpcMethodMap : IHclJsonRpcMethodMap
    {
        /// This dictionary maps the HCL method name to the WCF method name
        /// Type is different interface, such as IHypervisor, or IMachineManager, there can be all implemented in One WCF service
        private static Dictionary<Tuple<Type,string>,string> HclToJsonRpcName = new Dictionary<Tuple<Type,string>,string>()
        {
            { Tuple.Create(typeof(IHypervisor),nameof(IHypervisor.GetVersion)),nameof(IRemoteHcl.GetVersionAsync) },
            { Tuple.Create(typeof(IHypervisor),nameof(IHypervisor.GetStatus)),nameof(IRemoteHcl.GetStatusAsync) },
            { Tuple.Create(typeof(IHypervisor),nameof(IHypervisor.SayHello)),nameof(IRemoteHcl.SayHelloAsync) },
            { Tuple.Create(typeof(IHypervisor),nameof(IHypervisor.MaxOperationInProgressDefault)),nameof(IRemoteHcl.GetHypervisorObjectSettingsAsync) },
            { Tuple.Create(typeof(IHypervisor),nameof(IHypervisor.GetCapabilities)),nameof(IRemoteHcl.GetCapabilitiesAsync) },

        };

        private readonly Dictionary<String, MethodInfo> jsonRpcNameToWcfMethodMap = new Dictionary<String, MethodInfo>();

        public HclJsonRpcMethodMap()
        {
            var wcfMethods = typeof(IRemoteHcl).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var wcfMethod in wcfMethods)
            {
                jsonRpcNameToWcfMethodMap.Add(wcfMethod.Name, wcfMethod);
            }
            //Validate all mapped wcfMethods are valid
            foreach (var entry in HclToJsonRpcName.Where(entry => !jsonRpcNameToWcfMethodMap.ContainsKey(entry.Value)))
            {
                throw new ArgumentException($"JsonRpc Remote method {entry.Value} not found in JsonRpcRemoteHcl interface");
            }
        }

        public MethodInfo GetJsonRpcMethod(MethodInfo hclMethod)
        {
            HclToJsonRpcName.TryGetValue(Tuple.Create(hclMethod.DeclaringType, hclMethod.Name), out string remoteHclMethod);
            if (remoteHclMethod == null)
            {
                throw new InvalidOperationException($"Could not find hcl method: {hclMethod.Name} in cache for declaring type: {hclMethod.DeclaringType}");
            }
            jsonRpcNameToWcfMethodMap.TryGetValue(remoteHclMethod, out MethodInfo jsonRpcMethod);

            if (jsonRpcMethod == null)
            {
                throw new InvalidOperationException($"Could not find jsonrpc method associated with jsonrpcRemote method name: {remoteHclMethod}, for hcl method lookup: {hclMethod.Name}");
            }
            return jsonRpcMethod;
        }
    }
}
