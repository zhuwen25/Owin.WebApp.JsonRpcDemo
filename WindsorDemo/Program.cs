// See https://aka.ms/new-console-template for more information

using Castle.DynamicProxy;
using System;
using WindsorDemo;
using WindsorDemo.Interfaces;

public class Program
{
    public static void Main(string[] args)
    {
        // See https://aka.ms/new-console-template for more information
        Console.WriteLine("Hello, World!");

        bool useWcf = true;
        string preferWcf = "WCF.IMPL1";

        var container = WindsorBootstrapper.Bootstrap(useWcf, preferWcf);

        //var hypervisor = HypervisorFactory.CreateHypervisor("Azure", "connectionString");
        //var hypervisor = PluginHypervisorFactory.Create<IHypervisor>("Azure", "connectionString");
        var hypervisor = container.Resolve<IRemoteHypervisorProxyFactory>().CreateRemoteHypervisor( new AzureConnectionDetail());

        var isProxy = ProxyUtil.IsProxy(hypervisor);
        Console.WriteLine($"Hypervisor IsProxy: {isProxy}");
        var connectionDetail = new AzureConnectionDetail();
        //Call
        var result = hypervisor.GetStatus(connectionDetail, 100);


        var hypervisor2 = container.Resolve<IRemoteHypervisorProxyFactory>().CreateRemoteHypervisor( new VmwareConnectionDetail());
        var connectionDetail2 = new VmwareConnectionDetail();
        var result2 = hypervisor2.GetStatus(connectionDetail2, 888);

        Console.WriteLine($"Hypervisor Status: {result}");
    }
}
