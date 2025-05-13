using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WcfWithWsServer.JsonRpcHypervisor;

namespace WcfWithWsServer.WsApiCtler
{
    [RoutePrefix("rpc/hypervisor")]
    public class RpcHypervisorController : ApiController
    {
        private bool IsWebSocketClosureError(WebSocketException ex)
        {
            return ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely ||
                   ex.WebSocketErrorCode == WebSocketError.InvalidState || // Can occur if trying to use after close
                   ex.InnerException is
                       HttpListenerException || // Often happens on abrupt client disconnect with System.Net listener
                   ex.InnerException is System.Net.Sockets.SocketException;
        }

        private Task AcceptWebSocketAsync(IOwinContext context, Func<WebSocket,Task> onAccepted = null)
        {
            var tcs = new TaskCompletionSource<WebSocket>();
            // Get the WebSocket accept function from OWIN environment
            var accept = context.Get<Action<IDictionary<string, object>, Func<IDictionary<string, object>, Task>>>("websocket.Accept");
            if (accept == null)
            {
                var ex =  new NotSupportedException("WebSocket support not available");
                tcs.TrySetException(ex);
                return tcs.Task;
            }

            IDictionary<string, object> acceptOptions = null; // Options for the accept call
            string selectedSubProtocol = null; // The protocol we select
            const string SupportedSubProtocol = "jsonrpc";
            // 1. Check client's requested protocols
            var clientRequestedProtocolsHeader = context.Request.Headers.Get("Sec-WebSocket-Protocol");
            if (!string.IsNullOrEmpty(clientRequestedProtocolsHeader))
            {
                // 2. Parse the comma-separated list, trim whitespace
                var clientProtocols = clientRequestedProtocolsHeader
                    .Split(',')
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrEmpty(p));

                // 3. Check if the client requested the protocol we support (case-insensitive check is safer)
                if (clientProtocols.Contains(SupportedSubProtocol, StringComparer.OrdinalIgnoreCase))
                {
                    Console.WriteLine(
                        $"WebSocketMiddleware: Client requested '{SupportedSubProtocol}'. Accepting with this subprotocol.");
                    selectedSubProtocol = SupportedSubProtocol; // Select it

                    // 4. Prepare the options dictionary for the accept call
                    acceptOptions = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "websocket.SubProtocol", selectedSubProtocol }
                    };
                }
                else
                {
                    Console.WriteLine($"WebSocketMiddleware: Client requested protocols ({clientRequestedProtocolsHeader}), but not '{SupportedSubProtocol}'. Proceeding without subprotocol.");
                    // We don't select a subprotocol if the client didn't request the one we support.
                }
            }
            else
            {
                Console.WriteLine("WebSocketMiddleware: Client did not request any subprotocol.");
                // Proceed without subprotocol
            }

            // Accept the WebSocket connection
            accept(acceptOptions, env =>
            {
                //var ws = env["websocket.WebSocket"] as WebSocket;
                var wsContext = env["System.Net.WebSockets.WebSocketContext"] as WebSocketContext;
                if (wsContext?.WebSocket != null && onAccepted != null ) {
                    // Call the provided action with the accepted WebSocket
                    tcs.SetResult(wsContext.WebSocket);
                    onAccepted(wsContext.WebSocket);
                }
                else
                {
                    tcs.SetResult(null);
                    Console.WriteLine("WebSocketMiddleware: WebSocket context is null or action not provided.");
                }
                return Task.CompletedTask;
            });
            return tcs.Task;
        }


        [HttpGet]
        [Route("connect2")]
        public async Task Connect2()
        {
            HttpContext.Current.AcceptWebSocketRequest((contex) =>
            {

                var wsContext = contex.WebSocket;
                if (wsContext != null)
                {
                    // Handle the WebSocket connection
                    return ProcessWebSocketCommunication(wsContext);
                }
                else
                {
                    throw new InvalidOperationException("WebSocket context is null.");
                }

            });



        }


        [HttpGet]
        [Route("connect")]
        public async Task HypervisorConnect()
        {
            IOwinContext owinContext = Request.GetOwinContext();
            // Validate WebSocket request
            if (!string.Equals(owinContext.Request.Headers["Upgrade"], "websocket", StringComparison.OrdinalIgnoreCase) ||
                owinContext.Request.Headers["Connection"]?.IndexOf("upgrade", StringComparison.OrdinalIgnoreCase) < 0)
            {
                owinContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            try
            {
                // Below is good
                var  acceptOptions = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    { "websocket.SubProtocol", "jsonrpc" }
                };
                await OwinWebSocketHelper.AcceptWebSocketAsync(owinContext, async socket =>
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        string remoteIp = owinContext.Request.RemoteIpAddress;
                        int remotePort = owinContext.Request.RemotePort ?? 0;
                        // Start WebSocket session (handle messages)
                        await JsonRcpHypervisorService.Instance.WebsocketOpenedAsync(socket, remoteIp, remotePort)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        throw new InvalidOperationException("WebSocket is not in an open state.");
                    }
                }, acceptOptions);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket error: {ex.Message}");
            }
        }

        private async Task ProcessWebSocketCommunication(WebSocket webSocket)
        {
            string subProtocol = webSocket.SubProtocol;
            Console.WriteLine($"WebSocket connection established with subprotocol: {subProtocol}");

            // Use ArraySegment for better buffer management
            var buffer = new ArraySegment<byte>(new byte[1024 * 4]);
            var cancellationToken = CancellationToken.None;

            try
            {
                //Loop while the connection is open
                while (webSocket.State == WebSocketState.Open)
                {
                    // Wait for a message from the client
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, cancellationToken);
                    Console.WriteLine(
                        $"WebSocket ReceiveAsync Result: Type={result.MessageType}, Count={result.Count}, Status={result.CloseStatus}");

                    // Handle different message types
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        if (buffer.Array == null) continue; // Defensive check
                        string receivedMessage = System.Text.Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                        Console.WriteLine($"Received message: {receivedMessage}");
                        // Echo the message back to the client
                        string responseMessage = $"Server (API) received: {receivedMessage} at {DateTime.Now}";
                        byte[] responseBytes = Encoding.UTF8.GetBytes(responseMessage);
                        var responseBuffer = new ArraySegment<byte>(responseBytes);

                        await webSocket.SendAsync(responseBuffer, WebSocketMessageType.Text, result.EndOfMessage,
                            cancellationToken);
                        Console.WriteLine($"Sent (API): {responseMessage}");
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        // Client initiated close
                        Console.WriteLine(
                            $"Close received (API): {result.CloseStatus?.ToString() ?? "N/A"} - {result.CloseStatusDescription ?? "N/A"}");
                        // Acknowledge the close request
                        await webSocket.CloseAsync(result.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                            result.CloseStatusDescription ?? string.Empty, cancellationToken);
                        break; // Exit the loop
                    }
                    else //Other message types
                    {
                        Console.WriteLine($"Unsupported message type: {result.MessageType}");
                        // Handle other message types if needed
                    }
                }
            }
            catch (WebSocketException ex) when (IsWebSocketClosureError(ex))
            {
                // Common error when client disconnects without proper close handshake
                Console.WriteLine(
                    $"WebSocket connection closed abruptly (API): {ex.Message} (ErrorCode: {ex.WebSocketErrorCode})");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Console.WriteLine($"WebSocket error (API): {ex.GetType().Name} - {ex.Message}");
                // Attempt to close gracefully if the connection is still in a state where closing is possible
                if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                {
                    try
                    {
                        await webSocket.CloseOutputAsync(WebSocketCloseStatus.InternalServerError,
                            "Server error during processing", cancellationToken);
                        Console.WriteLine($"WebSocket Sent CloseOutput due to Server Error (API).");
                    }
                    catch (Exception closeEx)
                    {
                        Console.WriteLine(
                            $"WebSocket Exception during error cleanup CloseOutputAsync (API): {closeEx}");
                    }
                }
            }
            finally
            {
                // Ensure the socket is disposed
                webSocket?.Dispose();
                Console.WriteLine("WebSocket connection closed (API Controller).");
                Console.WriteLine($"ProcessWebSocketCommunication finished. Final State={webSocket?.State}");
            }
        }
    }
}
