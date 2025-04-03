﻿using System;
using System.ServiceModel;
using Microsoft.Owin.Hosting;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using WcfWithWsServer.WsApiCtler;

namespace WcfWithWsServer
{
    [ServiceContract]
    public interface IMyWcfService
    {
        [OperationContract]
        string GetData(int value);
    }

    // WCF Service Implementation
    public class MyWcfService : IMyWcfService
    {
        public string GetData(int value)
        {
            return $"You entered: {value}";
        }
    }



    internal class Program
    {
        private static void StartOwnServiceHost(ServiceHost serviceHost )
        {
            serviceHost.AddServiceEndpoint(typeof(IMyWcfService), new BasicHttpsBinding(), "");
            serviceHost.Open();
            Console.WriteLine($"WCF Service is running on Uri: {serviceHost.BaseAddresses}");
        }

        private static IDisposable StartRestApiNoController(string baseAddress)
        {
            try
            {
                var webApp = WebApp.Start<MyWebSocketStartup>(baseAddress);
                Console.WriteLine("WebSocket server running on Uri: " + baseAddress);
                return webApp;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        private static IDisposable StartControllerRestApi(string baseAddress)
        {
            try
            {
                var webApp = WebApp.Start<WsMidWareStartUp>(baseAddress);
                Console.WriteLine($"Web API is running on Uri: {baseAddress}");
                return webApp;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start Web API: {ex.Message}");
                return null;
            }

        }

        private static async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            // Your WebSocket handling logic
            byte[] buffer = new byte[1024];
            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                    break;
                }

            }

        }
        private static async void RawHttpListener(string baseAddress)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(baseAddress);
            listener.Start();

            Console.WriteLine("Listening on http://localhost:8080/");

            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();

                if (context.Request.IsWebSocketRequest)
                {
                    try
                    {
                        context.Response.StatusCode = 101;
                        context.Response.Headers.Add("Upgrade", "websocket");
                        context.Response.Headers.Add("Connection", "Upgrade");
                        context.Response.Close();
                        WebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
                        _ = Task.Run(async () =>
                        {
                            await HandleWebSocketConnection(webSocketContext.WebSocket);
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        public static async Task Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("Canceling...");
                cancellationTokenSource.Cancel();
                e.Cancel = true;
            };

            try
            {
                Uri baseAddress = new Uri("https://localhost:8080/wcf");

                ServiceHost host = new ServiceHost(typeof(MyWcfService), baseAddress);
                //StartOwnServiceHost(host);

                string url = "https://localhost:8080/wss/";
                //StartRestApiNoController(url);
                var webApi =  StartControllerRestApi(url);
                //RawHttpListener(url);

                await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);


                Console.WriteLine("Press Ctrl+C to exit.");
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine("The operation was canceled.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
