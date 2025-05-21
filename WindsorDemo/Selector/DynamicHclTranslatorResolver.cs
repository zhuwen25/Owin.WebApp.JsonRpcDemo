using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using System;
using WindsorDemo.Interfaces;
using WindsorDemo.Services;

namespace WindsorDemo.Selector
{
    public class DynamicHclTranslatorResolver: ISubDependencyResolver
    {
        private IToggle _toggleService ;
        private IKernel _kernel ;

        public DynamicHclTranslatorResolver( IKernel kernel)
        {
            this._toggleService = kernel.Resolve<IToggle>();
            this._kernel = kernel;
        }
        public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
            DependencyModel dependency)
        {
            //We want to provide IHclmethodTranslator for all IHclMethod
            //bool wantsTranslator = dependency.TargetType == typeof(IHclMethodTranslation);

            //Only for SynchronousOperations(or a list of configured operations)
            bool wantsTranslator = dependency.TargetType == typeof(IHclMethodTranslation);
            bool forHclOperation = typeof(IHclOperation).IsAssignableFrom(model.Implementation);

            Console.WriteLine($"wantsTranslator: {wantsTranslator} , forHclOperation: {forHclOperation}");

            //bool forSynchronousOperations = dependency.TargetType == typeof(IHclOperation);
            return  wantsTranslator & forHclOperation ;

        }

        public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
            DependencyModel dependency)
        {
            Type concreteTranslatorType  = _toggleService.IsToggleEnable("EnableJsonRpc") ?
                 typeof(HclMethodJsonRpcTranslation) : typeof(HclMethodToWcfTranslation);
            return _kernel.Resolve(concreteTranslatorType);
        }
    }
}
