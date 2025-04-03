
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Owin;
using Owin.WebSocket;
using Owin.WebSocket.Extensions;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace WcfHostApp.HttpRest
{

    public class MyWsStartup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapWebSocketRoute<MyWebSocketConnection>("/wss");
        }
    }

    public class MyWebSocketConnection : WebSocketConnection
    {
        public override Task OnOpenAsync()
        {
            Console.WriteLine("WebSocket connection opened.");
            return SendText(Encoding.UTF8.GetBytes("Hello from the server!"), true);
        }

        public override Task OnMessageReceived(ArraySegment<byte> message, WebSocketMessageType type)
        {
            string receivedMessage = Encoding.UTF8.GetString(message.Array, message.Offset, message.Count);
            Console.WriteLine($"Received: {receivedMessage}");
            // Echo the message back to the client
            return SendText(Encoding.UTF8.GetBytes($"Echo: {receivedMessage}"), true);
        }

        public override void OnClose(WebSocketCloseStatus? closeStatus, string closeStatusDescription)
        {
            Console.WriteLine($"WebSocket closed: {closeStatus} - {closeStatusDescription}");
        }
    }



    public class WebSocketServer : IDisposable
    {
        private string _baseAddress;
        private IDisposable _server;
        public WebSocketServer(string baseAddress)
        {
            _baseAddress = baseAddress;

        }

        public void Start()
        {
            try
            {
                StartOptions options = new StartOptions(_baseAddress)
                {
                    ServerFactory = "Microsoft.Owin.Host.HttpListener"
                };
                _server = Microsoft.Owin.Hosting.WebApp.Start<MyWsStartup>(options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start WebSocket server: {ex.Message}");
                throw;
            }
        }


        public void Dispose()
        {
            _server?.Dispose();
        }
    }
}
