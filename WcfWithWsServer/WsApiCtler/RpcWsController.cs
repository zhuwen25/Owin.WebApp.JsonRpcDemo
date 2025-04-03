using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.WebSockets;

namespace WcfWithWsServer.WsApiCtler
{
    [RoutePrefix("api/jsonrpc")]
    public class RpcWsController: ApiController
    {
        // Helper to identify common closure/abort exception types
        private bool IsWebSocketClosureError(WebSocketException ex) {
            return ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely ||
                   ex.WebSocketErrorCode == WebSocketError.InvalidState || // Can occur if trying to use after close
                   ex.InnerException is HttpListenerException || // Often happens on abrupt client disconnect with System.Net listener
                   ex.InnerException is System.Net.Sockets.SocketException;
        }

        [HttpGet]
        [Route("websocket")]
        public async Task<HttpResponseMessage>  GetWebSocketConnection()
        {

            IOwinContext owinContext = Request.GetOwinContext();
            if (owinContext.Request.Headers["Upgrade"]?.Equals("websocket", StringComparison.OrdinalIgnoreCase) == true &&
                owinContext.Request.Headers["Connection"]?.ToLower().Contains("upgrade") == true)
            {
                try
                {
                    // Accept WebSocket request
                    WebSocket websocket = await AcceptWebSocketAsync(owinContext);
                    await ProcessWebSocketCommunication(websocket);

                    // Return Switching Protocols response
                    return new HttpResponseMessage(System.Net.HttpStatusCode.SwitchingProtocols);
                }
                catch (Exception ex)
                {
                    return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                    {
                        Content = new StringContent($"Error: {ex.Message}")
                    };
                }
            }

            return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
            {
                Content = new StringContent("WebSocket request expected")
            };

        }

        private async Task<WebSocket> AcceptWebSocketAsync(IOwinContext context)
        {
            var acceptToken = new TaskCompletionSource<WebSocket>();

            // Get the WebSocket accept function from OWIN environment
            var accept = context.Get<Action<IDictionary<string, object>, Func<IDictionary<string, object>, Task>>>("websocket.Accept");

            if (accept == null)
            {
                throw new NotSupportedException("WebSocket support not available");
            }

            // Accept the WebSocket connection
            accept(null, async env =>
            {
                var ws = env["websocket.WebSocket"] as WebSocket;
                acceptToken.SetResult(ws);
            });

            return await acceptToken.Task;
        }

        //This method handles the ongoing websocket communication
        //It is called by AcceptWebSocketRequest after the handshake succeeds.
        //Note: This can be privately static or an instance method. Static avoids holding controller state.
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
                    Console.WriteLine($"WebSocket ReceiveAsync Result: Type={result.MessageType}, Count={result.Count}, Status={result.CloseStatus}");

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

                        await webSocket.SendAsync(responseBuffer, WebSocketMessageType.Text, result.EndOfMessage, cancellationToken);
                        Console.WriteLine($"Sent (API): {responseMessage}");
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        // Client initiated close
                        Console.WriteLine($"Close received (API): {result.CloseStatus?.ToString() ?? "N/A"} - {result.CloseStatusDescription ?? "N/A"}");
                        // Acknowledge the close request
                        await webSocket.CloseAsync(result.CloseStatus ?? WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription ?? string.Empty, cancellationToken);
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
                Console.WriteLine($"WebSocket connection closed abruptly (API): {ex.Message} (ErrorCode: {ex.WebSocketErrorCode})");
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
                        await webSocket.CloseOutputAsync(WebSocketCloseStatus.InternalServerError, "Server error during processing", cancellationToken);
                        Console.WriteLine($"WebSocket Sent CloseOutput due to Server Error (API).");
                    } catch (Exception closeEx){
                        Console.WriteLine($"WebSocket Exception during error cleanup CloseOutputAsync (API): {closeEx}");
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
