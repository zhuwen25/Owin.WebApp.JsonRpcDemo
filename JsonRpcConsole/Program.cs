using Fleck;
using System;
using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;

namespace JsonRpcConsole
{
    class Program
    {
        static async Task Main(string[] args)
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
                // while (cancellationTokenSource.IsCancellationRequested == false)
                {
                    Console.WriteLine("Starting server, Press Ctrl+C to end...");
                    await StartWssJsonRpcServerAsync(cancellationTokenSource.Token);
                }
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

        private static async Task StartWssJsonRpcServerAsync(CancellationToken cancellationToken)
        {

            Console.WriteLine("Starting server, Press Ctrl+C to end...");
            //JsonRpcHypervisorService wssServer = null;
            WebSharpServer wssWebSharpServer = null;
            try
            {
                var hostIp = IPAddress.Loopback.ToString();
                var uri = new UriBuilder("wss", "localhost", 80);
                SecureString securePassword = new SecureString();

                foreach (char c in "123456") // Replace with secure input
                {
                    securePassword.AppendChar(c);
                }
                securePassword.MakeReadOnly();
                var certificate2 = new X509Certificate2("D:\\code\\certificate.pfx", securePassword,
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.UserKeySet);

                wssWebSharpServer = new WebSharpServer("wss://localhost:80",certificate2);
                wssWebSharpServer.StartWebSocketServer(path:"/rpc/hypervisor");

                // wssServer.Start();

                await Task.Delay(Timeout.Infinite, cancellationToken);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                //wssServer?.Stop();
            }
        }
    }
}
