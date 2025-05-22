using Castle.Core;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using Castle.Windsor;
using System;
using WindsorDemo.Interfaces;
using WindsorDemo.Selector;
using WindsorDemo.Services;

namespace WindsorDemo
{


    public class WindsorInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            throw new NotImplementedException();
        }
    }

    public class WindsorBootstrapper
    {
        public static IWindsorContainer Bootstrap(bool useWcf = true, string preferWcf = "WCF.IMPL1" )
        {
            var container = new WindsorContainer();
            container.AddFacility<TypedFactoryFacility>();
           // container.Install(FromAssembly.This());
            //return container;
            //Registering the components for LoggingInterceptor

            // container.Register(Component.For<HclOperationFactorySelector>().LifestyleSingleton(),
            //     Component.For<IHclOperationFactory>().AsFactory(c=>c.SelectedWith<HclOperationFactorySelector>()).LifestyleSingleton(),
            //     Classes.FromThisAssembly().IncludeNonPublicTypes().BasedOn<IHclOperation>().LifestyleTransient());
            container.Register(Component.For<IToggle>().
                ImplementedBy<Toggle>().LifestyleSingleton());

            container.Register(
                // 1. Register your feature toggle service

                // 2. Register the individual selectors that the toggle-aware selector will use
                Component.For<HclOperationFactorySelector>().LifestyleSingleton(),
                Component.For<HclJsonRpcOperationFactorySelector>().LifestyleSingleton(),

                // 3. Register the ToggleAwareHclOperationFactorySelector itself.
                //    Windsor will inject the IFeatureToggleService, OriginalHclOperationFactorySelector,
                //    and AlternativeHclOperationFactorySelector into its constructor.
                Component.For<ToggleAwareHclOperationFactorySelector>().LifestyleSingleton(),

                // 4. Register your factory to use the ToggleAwareHclOperationFactorySelector
                Component.For<IHclOperationFactory>()
                    .AsFactory(c => c.SelectedWith<ToggleAwareHclOperationFactorySelector>())
                    .LifestyleSingleton(),

                // 5. Register all your IHclOperation implementations
                //    Make sure their naming or selection logic aligns with what
                //    OriginalHclOperationFactorySelector and AlternativeHclOperationFactorySelector expect.
                Classes.FromThisAssembly().IncludeNonPublicTypes().BasedOn<IHclOperation>().LifestyleTransient()
            );


            container.Register(Component.For<ToggleAwareHclTranslatorSelector>().LifestyleSingleton());
            container.Register(Component.For<IHclMethodTranslationFactory>().AsFactory(config =>
                config.SelectedWith<ToggleAwareHclTranslatorSelector>()).LifestyleSingleton());


            //Optional1:
            container.Register(Component.For<HclMethodToWcfTranslation>().LifestyleTransient(),
                Component.For<HclMethodJsonRpcTranslation>().LifestyleTransient());

            //Optional2:
            // var hclToWcfTranslator = nameof(HclMethodToWcfTranslation);
            // var hclToJsonRpcTranslator = nameof(HclMethodJsonRpcTranslation);
            // container.Register(
            //     Component.For<IHclMethodTranslation>().ImplementedBy<HclMethodToWcfTranslation>().Named(hclToWcfTranslator).LifestyleTransient(),
            //     Component.For<IHclMethodTranslation>().ImplementedBy<HclMethodJsonRpcTranslation>().Named(hclToJsonRpcTranslator).LifestyleTransient()
            // );

            container.Register(Component.For<IHclWcfAdapter>().ImplementedBy<HclWcfAdapter>().LifestyleSingleton(),
                Component.For<IHclJsonRpcAdapter>().ImplementedBy<HclJsonRpcAdapter>().LifestyleSingleton(),
                Component.For<IHclWcfMethodMap>().ImplementedBy<HclWcfMethodMap>().LifestyleSingleton(),
                Component.For<IHclJsonRpcMethodMap>().ImplementedBy<HclJsonRpcMethodMap>().LifestyleSingleton());


            //Add the sub-dependency Resolver ---
            //This resolver will inject the correct IHclMethodTranslation into IHclOperation instance
            //container.Kernel.Resolver.AddSubResolver(new DynamicHclTranslatorResolver(container.Kernel));


            //Registering the WCF
            container.Register(Component.For<IHypervisorCommunicationsLibraryInterface>()
                .ImplementedBy(typeof(HypervisorCommunicationsLibraryService))
                .LifestyleTransient().Named("HypervisorCommunicationsLibraryService"));

            container.Register(Component.For<IRemoteHcl>()
                .ImplementedBy<RemoteHclService>().LifestyleTransient().Named("RemoteHclService"));


            container.Register(
                // Dependencies for PluginHypervisor and PluginHypervisorFactory
                Component.For<IWcfClientService>().ImplementedBy<WcfClientService>().LifestyleTransient(),
                Component.For<IJsonRpcClientService>().ImplementedBy<JsonRpcClientService>().LifestyleTransient(),
                Component.For<IKernel>().Instance(container.Kernel),

                //Interceptors
                Component.For<LoggingInterceptor>().LifestyleTransient(), // Ensure LoggingInterceptor is registered
                Component.For<DynamicHclProxyInterceptor>().LifestyleTransient().DynamicParameters(((kernel, arguments) =>
                {
                    // arguments here are those passed to _kernel.Resolve<DynamicHclProxyInterceptor>(arguments)
                    // or from the component it's intercepting if resolved as part of that.
                    // The arguments from factory's _kernel.Resolve<IRemoteHypervisor> are for IRemoteHypervisor's resolution context.
                    Console.WriteLine("DynamicHclProxyInterceptor created with arguments: " + string.Join(", ", arguments));
                })),

                // *** This is the key registration for your proxied hypervisor ***
                Component.For<IRemoteHypervisor>().ImplementedBy<PluginHypervisor>().Interceptors(typeof(LoggingInterceptor))
                    .Interceptors(InterceptorReference.ForType<DynamicHclProxyInterceptor>()).Last
                    .LifestyleTransient()
                    .DependsOn(Dependency.OnValue("connectionDetail", null)),
                Component.For<IRemoteHypervisorProxyFactory>().ImplementedBy<PluginHypervisorFactory>().LifestyleTransient());

            return container;
        }
    }
}
