using System;
using System.ServiceModel;

namespace WcfHostApp.WsDefines
{
    public class MyWebSocketService : IMyWebSocketService
    {
        public void SendMessage(string message)
        {
            // Get the callback channel to send a message back to the client
            var callback = OperationContext.Current.GetCallbackChannel<IMyWebSocketCallback>();
            callback?.OnMessageReceived($"Server received: {message}");
            Console.WriteLine($"Client sent: {message}");
        }

        public void Connect()
        {
            var callback = OperationContext.Current.GetCallbackChannel<IMyWebSocketCallback>();
            callback?.OnMessageReceived("Connected to the server!");
            Console.WriteLine("A client has connected.");
        }
    }
}
