using Castle.DynamicProxy;
using Castle.MicroKernel;
using System;
using System.Reflection;
using WindsorDemo.Interfaces;

namespace WindsorDemo.Services
{
    public class DynamicHclProxyInterceptor : IInterceptor, IDisposable
    {
        private readonly IKernel _kernel;
        private readonly IToggle _toggle;
        private readonly IHclOperationFactory _hclOperationFactory;
        private readonly string _pluginName;
        private readonly IConnectionDetail _connectionDetail;
        private readonly IWcfClientService _wcfClientService;
        private readonly IJsonRpcClientService _jsonRpcClientService;

        public DynamicHclProxyInterceptor(IConnectionDetail connectionDetail,  IToggle toggle, IKernel kernel,IHclOperationFactory factory,
            IWcfClientService wcfClientService,IJsonRpcClientService jsonRpcClientService)
        {
            _pluginName = connectionDetail.FactoryName;
            _connectionDetail = connectionDetail;
            Console.WriteLine("DynamicHclProxyInterceptor constructor called with pluginName: " + _pluginName);
            _toggle = toggle;
            _kernel = kernel;
            _hclOperationFactory = factory;
            this._wcfClientService = wcfClientService;
            this._jsonRpcClientService = jsonRpcClientService;
        }
        public void Intercept(IInvocation invocation)
        {
            // Implement your interception logic here
            Console.WriteLine($"{GetType().Name}.Intercepting method: {invocation.Method.Name}");

            var operation = _hclOperationFactory.Create(invocation.Method, _pluginName, invocation.Arguments, _connectionDetail);
            try
            {
                Console.WriteLine($"{GetType().Name}.Execute method: {operation.GetType().Name}");

                invocation.ReturnValue = operation.Execute();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{GetType().Name} Exception caught: " + ex.Message);
            }
            finally
            {
                _hclOperationFactory.Release(operation);
            }
        }

        public void Dispose()
        {
            _kernel?.Dispose();
        }
    }
}
