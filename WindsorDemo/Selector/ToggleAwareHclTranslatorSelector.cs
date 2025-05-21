using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using System;
using System.Reflection;
using WindsorDemo.Interfaces;
using WindsorDemo.Services;

namespace WindsorDemo.Selector
{
    public class ToggleAwareHclTranslatorSelector : DefaultTypedFactoryComponentSelector
    {
        private readonly IToggle _toggle;
        private readonly IKernel _kernel;

        public ToggleAwareHclTranslatorSelector(IToggle toggle, IKernel kernel)
        {
            this._toggle = toggle;
            this._kernel = kernel;
        }
        // This method determines WHICH CONCRETE TYPE the factory should create

        // If you want to use the component type instead of the name, you can uncomment the following method
        // Then you register should be like this:
        // container.Register(Component.For<HclMethodJsonRpcTranslation>(),Component.For<HclMethodToWcfTranslation>());
        protected override Type GetComponentType(MethodInfo factoryMethod, object[] arguments)
        {
            if (factoryMethod.Name == nameof(IHclMethodTranslationFactory.Create) && factoryMethod.ReturnType == typeof(IHclMethodTranslation))
            {
                var targetType = _toggle.IsToggleEnable("EnableJsonRpc")
                    ? typeof(HclMethodJsonRpcTranslation)
                    : typeof(HclMethodToWcfTranslation);
                Console.WriteLine($"{GetType().Name}.GetComponentType: {targetType.Name}");

                return targetType;
            }
            return base.GetComponentType(factoryMethod, arguments);
        }

        // This method determines WHICH CONCRETE TYPE the factory should create
        // container.Register(Component.For<IHclMethodJsonRpcTranslation>().ImplementBy<HclMethodJsonRpcTranslation>.Named(${nameof{HclMethodJsonRpcTranslation}}).....;
        // protected override string GetComponentName(MethodInfo factoryMethod, object[] arguments)
        // {
        //     if (factoryMethod.Name == nameof(IHclMethodTranslationFactory.Create) &&
        //         factoryMethod.ReturnType == typeof(IHclMethodTranslation))
        //     {
        //         var targetType = _toggle.IsToggleEnable("EnableJsonRpc")
        //             ? nameof(HclMethodJsonRpcTranslation)
        //             : nameof(HclMethodToWcfTranslation);
        //         Console.WriteLine($"{GetType().Name}.GetComponentName: {targetType}");
        //
        //         return targetType;
        //     }
        //
        //     return base.GetComponentName(factoryMethod, arguments);
        // }
    }
}
