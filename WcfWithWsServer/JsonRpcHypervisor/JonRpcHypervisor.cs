using HypervisorCreator;
using JsonRpcContract.Contracts;
using Newtonsoft.Json;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;
using StreamJsonRpc.Reflection;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Yz.AzureHypervisor;

namespace WcfWithWsServer.JsonRpcHypervisor
{
    public class WsJsonRpcHandler : JsonRpc, IJsonRpcTracingCallbacks
    {
        public WsJsonRpcHandler(WebSocket ws, object? target = null) : base(
            new WebSocketMessageHandler(ws, new JsonMessageFormatter
            {
                JsonSerializer =
                {
                    TypeNameHandling = TypeNameHandling.All,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
                    // Converters = { new () } // Add custom converters
                }
            }), target)
        {
        }

        void IJsonRpcTracingCallbacks.OnMessageDeserialized(JsonRpcMessage message, object encodedMessage)
        {
            Console.WriteLine($"{this.GetType().FullName}: Server: OnMessageDeserialized message:{encodedMessage}");
        }

        void IJsonRpcTracingCallbacks.OnMessageSerialized(JsonRpcMessage message, object encodedMessage)
        {
            Console.WriteLine($"{this.GetType().FullName}: Server OnMessageSerialized message:{message}");
        }


        protected override Type? GetErrorDetailsDataType(JsonRpcError error)
        {
            var ret = base.GetErrorDetailsDataType(error);
            if (ret != typeof(Exception))
            {
                return ret;
            }
            // if (error.Error?.Data is not null && error.Error?.Data?.ToString().IndexOf(Exception., StringComparison.Ordinal) > -1)
            // {
            //     return typeof(FaultException);
            // }

            return ret;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of any resources here
            }
          //  base.Dispose(disposing);
        }
    }

    public class JsonRpcHypervisor : IHypervisor , IDisposable
    {
        private readonly WsJsonRpcHandler _jsonRpcHandler;
        private readonly IHypervisorFactory _factory;
        private readonly string _sessionIdentifier;
        //private WebSocket _webSocket;

        public JsonRpcHypervisor(WebSocket ws , IHypervisorFactory factory , string identifier)
        {
            _factory = factory;
            _jsonRpcHandler = new WsJsonRpcHandler(ws, this);
            _sessionIdentifier = identifier;
          //  _webSocket = ws;
        }

        public async Task<HelloResponse> SayHelloAsync(HelloRequest request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(new HelloResponse { Message = $"Hello from {this.GetType().Name}" });
        }

        public async Task HandleRpcSessionAsync(Action<string> onSessionClosed)
        {
            try
            {
                _jsonRpcHandler.StartListening();
                await _jsonRpcHandler.Completion;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client {_sessionIdentifier} error: {ex.Message}");
            }
            finally
            {
                onSessionClosed?.Invoke(_sessionIdentifier);
                // _jsonRpcHandler.Dispose();
                // if (_webSocket.State == WebSocketState.Open && _webSocket.State
                //     != WebSocketState.Aborted)
                // {
                //     await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                // }

             //   Console.WriteLine($"Client {_sessionIdentifier} disconnected");
            }
        }


        public async Task<IConnectionDetail> GetConnectionDetailAsync(string factoryName,
            CancellationToken cancellationToken)
        {
            var connectionDetail = new AzureConnectionDetail();
            connectionDetail.FactoryName = factoryName; // factoryName is not used in AzureConnectionDetail Just for demo
            var hypervisor = await GetHypervisorAsync(connectionDetail, cancellationToken).ConfigureAwait(false);
            return await hypervisor.GetConnectionDetailAsync(factoryName, cancellationToken);
        }

        private async Task<IHypervisor> GetHypervisorAsync(IConnectionDetail connectionDetail,
            CancellationToken cancellationToken)
        {
            return await _factory.GetHypervisorAsync(connectionDetail, cancellationToken);
        }

        public async Task<VMDiskInfo> GetVMDiskInfoAsync(IConnectionDetail connectionDetail,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _jsonRpcHandler?.Dispose();
        }
    }
}
