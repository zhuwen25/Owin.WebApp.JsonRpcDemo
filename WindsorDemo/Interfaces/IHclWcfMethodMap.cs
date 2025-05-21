using System.Reflection;

namespace WindsorDemo.Interfaces
{
    public interface IHclWcfMethodMap
    {
        MethodInfo GetWcfMethod(MethodInfo hclMethod);
    }
}
