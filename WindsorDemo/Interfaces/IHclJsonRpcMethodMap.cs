using System.Reflection;

namespace WindsorDemo.Interfaces
{
    public interface IHclJsonRpcMethodMap
    {
        MethodInfo GetJsonRpcMethod(MethodInfo hclMethod);
    }
}
