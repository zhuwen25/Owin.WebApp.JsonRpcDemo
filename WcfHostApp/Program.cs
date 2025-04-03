using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using WcfHostApp.HttpRest;
using WcfHostApp.WcfDefinitions;
using WcfHostApp.WsDefines;

namespace WcfHostApp
{
    internal class Program
    {
        private static X509Certificate2 GetSelfSignedCertificate()
        {
            // In a real scenario, you'd load this from a certificate store
            // For this example, we'll create a self-signed cert (requires admin rights)
            // Note: In production, use a proper certificate from a CA

            // You can create a self-signed cert using makecert.exe or PowerShell:
            // New-SelfSignedCertificate -DnsName "localhost" -CertStoreLocation "cert:\LocalMachine\My"

            // For this example, assuming certificate is in store:
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            var certCollection = store.Certificates.Find(X509FindType.FindBySubjectName, "localhost", false);

            if (certCollection.Count > 0)
                return certCollection[0];

            throw new Exception("Certificate not found");
        }




        public static void Main(string[] args)
        {
            //string addressHttp = String.Format("http://{0}:80/Calculator", System.Net.Dns.GetHostEntry("").HostName);
            string addressHttp = String.Format("https://localhost:8080");
            var binding = new WSHttpBinding();
            //binding.Security.Mode =
            //binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
            binding.Security.Mode = SecurityMode.Transport;
           binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;

            //Create an array of URI objects to have base address
            Uri a = new Uri(addressHttp+"/wcf");
            Uri[] baseAddress = new Uri[] { a };

            // Create the ServiceHost. The service type (Calculator) is not shown here.
            ServiceHost sh = new ServiceHost(typeof(CalculatorService), baseAddress);
            //ServiceHost sh = new ServiceHost(typeof(ICalculatorService), baseAddress);
            Type c = typeof(ICalculatorService);
            sh.AddServiceEndpoint(c, binding,"");

            //Add websocket  endpoint
           // ServiceHost webSocketHost = new ServiceHost(typeof(MyWebSocketService), baseAddress);
            // var wsBinding = new NetHttpBinding()
            // {
            //    WebSocketSettings = { TransportUsage = WebSocketTransportUsage.Always },
            //     Security =
            //     {
            //         Mode = BasicHttpSecurityMode.Transport,
            //         Transport = { ClientCredentialType = HttpClientCredentialType.None }
            //     }
            // };

//            sh.AddServiceEndpoint(typeof(IMyWebSocketService), wsBinding, "wss");

            sh.Credentials.ServiceCertificate.Certificate = GetSelfSignedCertificate();

            // This next line is optional. It specifies that the client's certificate
            // does not have to be issued by a trusted authority, but can be issued
            // by a peer if it is in the Trusted People store. Do not use this setting
            // for production code. The default is PeerTrust, which specifies that
            // the certificate must originate from a trusted certificate authority.

            // sh.Credentials.ClientCertificate.Authentication.CertificateValidationMode =
            //  X509CertificateValidationMode.PeerOrChainTrust;


            // ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
            // smb.HttpsGetEnabled = true;
            // smb.HttpsGetUrl = new Uri(addressHttp+"/metadata");
            // webSocketHost.Description.Behaviors.Add(smb);
            try
            {
                sh.Open();
                //webSocketHost.Open();
                for (var ennpoint = 0; ennpoint < sh.Description.Endpoints.Count; ennpoint++)
                {
                    Console.WriteLine($"Endpoint {ennpoint} is {sh.Description.Endpoints[ennpoint].Address}");
                }

                //Console.WriteLine($"WebSocket endpoint is {webSocketHost.Description.Endpoints[0].Address}");
                // string address = sh.Description.Endpoints[0].ListenUri.AbsoluteUri;
                ///Console.WriteLine($"Listening @{address}");
                //Console.WriteLine("Press enter to close the service");
                Console.WriteLine($"Service host state: {sh.State}");

                //Start WebsocketRestApi

                var restApi = new WebSocketServer(addressHttp);
                restApi.Start();
                Console.WriteLine($"WebSocket Rest API started at {addressHttp}");


                Console.ReadLine();
                sh.Close();
                //webSocketHost.Close();
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine($"A communication error occurred: {ce.Message}");
                Console.WriteLine();
            }
            catch (System.Exception exc)
            {
                Console.WriteLine($"An unforeseen error occurred: {exc.Message}");
                Console.ReadLine();
            }
        }
    }
}
