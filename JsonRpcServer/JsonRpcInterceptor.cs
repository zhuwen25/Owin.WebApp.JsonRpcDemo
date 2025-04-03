using Castle.DynamicProxy;

namespace JsonRpcServer;

public class JsonRpcInterceptor : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        throw new NotImplementedException();
    }
}
