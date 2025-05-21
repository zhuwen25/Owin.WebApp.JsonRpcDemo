using System;
using System.Threading;
using System.Threading.Tasks;

namespace WindsorDemo.Interfaces
{
    public interface IConnectionDetail
    {
        string FactoryName { get; set; }
        string ConnectionString { get; set; }
    }

    public class AzureConnectionDetail : IConnectionDetail
    {
        public string FactoryName { get; set; } = "AzureRm";
        public string ConnectionString { get; set; } = "DefaultConnection";
    }
}
