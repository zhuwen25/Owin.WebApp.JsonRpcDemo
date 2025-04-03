using System;
using System.Net;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Text;
using System.Threading;
using WcfWithWsServer;

namespace WcfHostAppClient
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //Call HTTPS
            // Bypass certificate validation (for testing only)
            ServicePointManager.ServerCertificateValidationCallback =
                (sender, cert, chain, sslPolicyErrors) => true;

            // CallHttpEndpoint();
            var wsClient = new ClientWebSocket();
            CallWebSocketEndpointAsync(wsClient);
            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
            wsClient.Dispose();
        }

        public static void CallHttpEndpoint()
        {
            try
            {
                var httpBinding = new WSHttpBinding();
                httpBinding.Security.Mode = SecurityMode.Transport;
                httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;

                var httpEndpoint = new EndpointAddress("https://localhost:8080/wcf");
                var factory = new ChannelFactory<IMyWcfService>(httpBinding, httpEndpoint);
                factory.Credentials.ServiceCertificate.SslCertificateAuthentication =
                    new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode =
                            X509CertificateValidationMode.None // For self-signed cert testing only
                        // In production, use ChainTrust or PeerTrust
                    };
                var client = factory.CreateChannel();
                var response = client.GetData(199);
                Console.WriteLine("HTTP Response: " + response);
                ((IClientChannel)client)?.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("HTTP Error: " + ex.Message);
            }
        }

        static async void CallWebSocketEndpointAsync(ClientWebSocket wsClient)
        {
            try
            {
                var uri = new Uri("wss://localhost:8080/wss/api/jsonrpc/websocket");
                await wsClient.ConnectAsync(uri, CancellationToken.None);
                Console.WriteLine("WebSocket connected to " + uri);
                var messaege = "Hello from WCF client!";
                var buffer = System.Text.Encoding.UTF8.GetBytes(messaege);
                await wsClient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
                    CancellationToken.None);

                // Receive a message from the server
                //var buffer = new byte[1024];
                var result = await wsClient.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received: {receivedMessage}");

                // Close the connection
                // await wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                // Console.WriteLine("WebSocket connection closed");
            }
            catch (Exception ex)
            {
                Console.WriteLine("WebSocket Error: " + ex.ToString());
            }
        }
    }
}
