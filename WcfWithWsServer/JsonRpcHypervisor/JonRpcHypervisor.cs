using HypervisorCreator;
using JsonRpcContract.Contracts;
using Nerdbank.Streams;
using Newtonsoft.Json;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;
using StreamJsonRpc.Reflection;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        private WebSocket _webSocket;

        public JsonRpcHypervisor(WebSocket ws , IHypervisorFactory factory , string identifier)
        {
            _factory = factory;
            _jsonRpcHandler = new WsJsonRpcHandler(ws, this);
            _sessionIdentifier = identifier;
            _webSocket = ws;
        }

        protected virtual T Execute<T>(Func<T> func,[CallerMemberName] string methodName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Console.WriteLine($"Executing {methodName} at line {lineNumber}");
            try
            {
                return func();
            }
            finally
            {
                Console.WriteLine($"Executed {methodName} at line {lineNumber}");
            }
        }

        protected static Func<T> WrapExceptions<T>(Func<T> func)
        {
            return () =>
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    throw;
                }
            };
        }

        public async Task<HelloResponse> SayHelloAsync(HelloRequest request, CancellationToken cancellationToken)
        {

            var response = Execute(WrapExceptions(() =>
            {
                if (request.IsException)
                {
                    throw new Exception("This is a test exception");
                }

                return new HelloResponse { Message = $"Hello {request?.Name} from {this.GetType().Name}" };
            }));
            return await Task.FromResult( response);
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
            var hypervisor=  await _factory.GetHypervisorAsync(connectionDetail, cancellationToken)
                .ConfigureAwait(false);
            return await hypervisor.GetVMDiskInfoAsync(connectionDetail,cancellationToken);
        }


        public void Dispose()
        {
            _jsonRpcHandler?.Dispose();
        }
    }
}
