using System.Collections.Generic;
using System.Reflection;

namespace WindsorDemo.Interfaces
{
    public interface IHclOperationFactory
    {
        IHclOperation Create(MethodInfo hclMethod, string hypervisorPlugin, IEnumerable<object> hclArguments,IConnectionDetail connectionDetail);
        void Release(IHclOperation operation);
    }

    public interface IHclOperation
    {
        object Execute();
    }
}
