using Castle.DynamicProxy;
using System;
using System.Linq;

namespace WindsorDemo
{
    public class LoggingInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            var typeName = invocation.Method?.DeclaringType?.Name;

            Console.WriteLine($"[Proxy Log] Calling: {typeName}.{invocation.Method.Name}" +
            $"with args: {string.Join(", ", invocation.Arguments.Select(a => a?.ToString() ?? "null"))}");
            try
            {
                invocation.Proceed();
                if (invocation.Method.ReturnType != typeof(void))
                {
                    Console.WriteLine($"[Proxy Log] Finished:{invocation.Method.Name}, Return value: {invocation.ReturnValue??"null"}");
                }
                else
                {
                    Console.WriteLine($"[Proxy Log] Finished:{invocation.Method.Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Proxy Log] Exception:{ex.Message} in {typeName}.{invocation.Method.Name}");
            }
        }
    }
}
