using System;
using WindsorDemo.Interfaces;

namespace WindsorDemo.Services
{
    public class HclJsonRpcAdapter: IHclJsonRpcAdapter
    {
        public object ToHclObject(object jsonRpcObject, Type targetType)
        {
            return HclWcfAdapter.ConvertObjectUsingConverterMethods(jsonRpcObject, targetType);
        }

        public object ToJsonRpcObject(object hclObject, Type targetType)
        {
            return HclWcfAdapter.ConvertObjectUsingConverterMethods(hclObject, targetType);
        }
    }
}
