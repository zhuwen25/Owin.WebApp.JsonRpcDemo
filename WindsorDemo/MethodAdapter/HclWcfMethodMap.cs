using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WindsorDemo.Interfaces;

namespace WindsorDemo.Services
{
    public class HclWcfMethodMap : IHclWcfMethodMap
    {
        /// This dictionary maps the HCL method name to the WCF method name
        /// Type is different interface, such as IHypervisor, or IMachineManager, there can be all implemented in One WCF service
        private static Dictionary<Tuple<Type,string>,string> HclToWcfName = new Dictionary<Tuple<Type,string>,string>()
        {
            { Tuple.Create(typeof(IHypervisor),nameof(IHypervisor.GetVersion)),nameof(IHypervisorCommunicationsLibraryInterface.GetVersion) },
            { Tuple.Create(typeof(IHypervisor),nameof(IHypervisor.GetStatus)),nameof(IHypervisorCommunicationsLibraryInterface.GetStatus) },
            { Tuple.Create(typeof(IHypervisor),nameof(IHypervisor.SayHello)),nameof(IHypervisorCommunicationsLibraryInterface.SayHello) },
        };

        private readonly Dictionary<String, MethodInfo> wcfNameToWcfMethodMap = new Dictionary<String, MethodInfo>();


        public HclWcfMethodMap()
        {
            var wcfMethods = typeof(IHypervisorCommunicationsLibraryInterface).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var wcfMethod in wcfMethods)
            {
                wcfNameToWcfMethodMap.Add(wcfMethod.Name, wcfMethod);
            }
            //Validate all mapped wcfMethods are valid
            foreach (var entry in HclToWcfName.Where(entry => !wcfNameToWcfMethodMap.ContainsKey(entry.Value)))
            {
                throw new ArgumentException($"WCF method {entry.Value} not found in WCF interface");
            }
        }

        public MethodInfo GetWcfMethod(MethodInfo hclMethod)
        {
            HclToWcfName.TryGetValue(Tuple.Create(hclMethod.DeclaringType, hclMethod.Name), out string wcfMethodName);
            if (wcfMethodName == null)
            {
                throw new InvalidOperationException($"Could not find hcl method: {hclMethod.Name} in cache for declaring type: {hclMethod.DeclaringType}");
            }
            wcfNameToWcfMethodMap.TryGetValue(wcfMethodName, out MethodInfo wcfMethod);

            if (wcfMethod == null)
            {
                throw new InvalidOperationException($"Could not find wcf method associated with wcf method name: {wcfMethodName}, for hcl method lookup: {hclMethod.Name}");
            }
            return wcfMethod;
        }
    }
}
