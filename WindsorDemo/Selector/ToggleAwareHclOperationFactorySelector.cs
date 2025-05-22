using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using System;
using System.Reflection;
using WindsorDemo.Interfaces;

namespace WindsorDemo.Selector
{
    public class ToggleAwareHclOperationFactorySelector:ITypedFactoryComponentSelector
    {
        private readonly IToggle _toggle;
        private readonly HclOperationFactorySelector _hclOperationFactorySelector;
        private readonly HclJsonRpcOperationFactorySelector _hclJsonRpcOperationFactorySelector;
        public ToggleAwareHclOperationFactorySelector(IToggle toggle,
            HclOperationFactorySelector hclOperationFactorySelector, HclJsonRpcOperationFactorySelector hclJsonRpcOperationFactorySelector)
        {
            this._toggle = toggle;
            this._hclOperationFactorySelector = hclOperationFactorySelector;
            this._hclJsonRpcOperationFactorySelector = hclJsonRpcOperationFactorySelector;
        }

        public  Func<IKernelInternal, IReleasePolicy, object> SelectComponent(MethodInfo method, Type type, object[] arguments)
        {
           // Console.WriteLine($"[MySelector] Attempting to select for method: {method.Name}, type: {type.Name}");
            return _toggle.IsToggleEnable("EnableJsonRpc") ?  _hclJsonRpcOperationFactorySelector.SelectComponent(method, type, arguments)
                : _hclOperationFactorySelector.SelectComponent(method, type, arguments);
        }

        // protected override Type GetComponentType(MethodInfo method, object[] arguments)
        // {
        //     // if (_toggle.IsToggleEnable("EnableJsonRpc"))
        //     // {
        //     //     return null;
        //     // }
        //     return base.GetComponentType(method, arguments);
        // }
    }
}
