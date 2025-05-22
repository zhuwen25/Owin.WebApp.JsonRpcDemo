using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using System;
using System.Collections.Generic;
using System.Reflection;
using WindsorDemo.Interfaces;
using WindsorDemo.RemoteHclOperation;

namespace WindsorDemo.Selector
{
    public class HclJsonRpcOperationFactorySelector : DefaultTypedFactoryComponentSelector
    {
        private readonly Dictionary<string, Type> _specialCaseOperations = new Dictionary<string, Type>()
            { };
        private readonly IKernel _kernel;

        public HclJsonRpcOperationFactorySelector(IToggle toggle,IKernel kernel) {

            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        }

        protected override Type GetComponentType(MethodInfo method, object[] arguments)
        {
            var hclMethod = arguments[0] as MethodInfo;
            if (hclMethod == null)
            {
                throw new ArgumentException("The first argument must be a MethodInfo object.");
            }

            if (_specialCaseOperations.TryGetValue(hclMethod.Name, out var opType))
            {
                return opType;
            }
            if (hclMethod != null && hclMethod.Name.StartsWith("op_"))
            {
                throw new NotImplementedException();
            }

            return typeof(SyncToAsyncOperations);
        }
    }
}
