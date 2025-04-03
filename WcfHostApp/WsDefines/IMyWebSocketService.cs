using System.ServiceModel;

namespace WcfHostApp.WsDefines
{
    [ServiceContract(CallbackContract = typeof(IMyWebSocketCallback))]
    public interface IMyWebSocketService
    {
        [OperationContract(IsOneWay = true)]
        void SendMessage(string message);

        [OperationContract(IsOneWay = true)]
        void Connect();
    }

    public interface IMyWebSocketCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnMessageReceived(string message);
    }
}
