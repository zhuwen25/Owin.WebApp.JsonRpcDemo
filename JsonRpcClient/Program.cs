// See https://aka.ms/new-console-template for more information

using JsonRpcContract;
using JsonRpcContract.Contracts;
using StreamJsonRpc;
using System.Net.WebSockets;
using Newtonsoft.Json;
using Yz.AzureHypervisor;

namespace JsonRpcClient;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            Console.WriteLine("Canceling...");
            cancellationTokenSource.Cancel();
            e.Cancel = true;
        };

        using var webSocket = new ClientWebSocket();
        webSocket.Options.AddSubProtocol("jsonrpc");
        webSocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
        try
        {
            while (cancellationTokenSource.IsCancellationRequested == false)
            {
                Console.WriteLine("Starting server, Press Ctrl+C to end...");
                await MainAsync(cancellationTokenSource.Token, webSocket);
                cancellationTokenSource.Token.WaitHandle.WaitOne();
                Thread.Sleep(1000*50);
            }
        }
        catch (OperationCanceledException e)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
            Console.WriteLine("The operation was canceled.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static async Task MainAsync(CancellationToken cancellationToken , ClientWebSocket webSocket )
    {
        try
        {
            //Allow self-signed certificate
            //using var handler = new HttpClientHandler();
            //handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            //await webSocket.ConnectAsync(new Uri("wss://localhost:5000/rpc/hypervisor"), cancellationToken);

            //Connect to the server for console app
            await webSocket.ConnectAsync(new Uri("wss://localhost:8080/wss/rpc/hypervisor/connect"), cancellationToken);

            var messageHandler = new WebSocketMessageHandler(webSocket, new JsonMessageFormatter
            {
                JsonSerializer = {
                    TypeNameHandling = TypeNameHandling.All,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
                    // Converters = { new () } // Add custom converters
                }
            });

            if (webSocket.State == WebSocketState.Open)
            {
                var wsJsonRpcClient = new WsJsonRpcClient(messageHandler);
                wsJsonRpcClient.StartListening();

                var vmwareConnection = new VmwareConnectionDetail("vmUserName", "Vmware Password");
                var rpcHypervisor = wsJsonRpcClient.Attach<IHypervisor>();

                //Say Hello first
                var helloResponse =
                    await rpcHypervisor.SayHelloAsync(new HelloRequest { Name = "JsonRpc Client" }, cancellationToken);
                Console.WriteLine("Hello response: " + helloResponse?.Message);

                var connectionDetail =
                    await rpcHypervisor.GetConnectionDetailAsync(vmwareConnection.FactoryName, cancellationToken);
                Console.WriteLine("Connection detail: " + connectionDetail?.ToString());

                var azureConnectionDetail = new AzureConnectionDetail();
                var resp = await rpcHypervisor.GetVMDiskInfoAsync(azureConnectionDetail, cancellationToken);

                Console.WriteLine("VMDiskInfo detail: " + resp);
            }

            // if (webSocket.State == WebSocketState.Open)
            // {
            //     await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Finished in client", cancellationToken);
            // }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
