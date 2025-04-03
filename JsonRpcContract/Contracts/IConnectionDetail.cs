using System;

namespace JsonRpcContract.Contracts
{
    //[JsonDerivedType(typeof(AzureConnectionDetail),"AzureConnectionDetail")]
//[JsonDerivedType(typeof(VmwareConnectionDetail),"VMWareConnectionDetail")]

public abstract class IConnectionDetail : IDisposable
{
    public const string FactoryNameVmware = "Vmware";
    public const string FactoryNameAzure = "Azure";
    public abstract string FactoryName { get; set; }
    public abstract string Name { get; set; }
    public abstract string Address { get; set; }
    public abstract Guid ConnectionId { get; set; }
    public abstract override string ToString();
    public abstract void Dispose();
}
}
