using System.ServiceModel;

namespace WcfHostApp.WcfDefinitions
{
    [ServiceContract]
    public interface ICalculatorService
    {
        [OperationContract]
        int Add(int a, int b);
    }
}
