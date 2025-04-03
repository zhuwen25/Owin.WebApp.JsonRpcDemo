using Owin;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WcfWithWsServer
{
    // [assembly: OwinStartup(typeof(MyWebSocketStartup))]
    public class MyWebSocketStartup
    {
        public void Configuration(IAppBuilder app)
        {
            Console.WriteLine("WebSocket server started configuration");
            // Add middleware to handle WebSocket requests specifically on the "/ws" path
            app.Use(async (context, next) =>
            {
                Console.WriteLine($"Request received: {context.Request.Path.Value}");

                // Log entry for debugging
                Console.WriteLine($"Middleware Entered. Request URI: {context.Request.Uri}");
                Console.WriteLine($"Middleware Entered. Request Path.Value: '{context.Request.Path.Value}'"); // Log original value too
                Console.WriteLine($"Middleware Entered. Request Uri.AbsolutePath: '{context.Request.Uri.AbsolutePath}'");

                // Check if the request path matches our WebSocket endpoint
                if (context.Request.Uri.AbsolutePath == "/wss")
                {
                    Console.WriteLine($"Request received for {context.Request.Path.Value}");

                    // Check if the request is a WebSocket upgrade request
                    // The OWIN environment dictionary contains details about the request.
                    // The "websocket.Accept" key holds a delegate if the request is upgradable.
                    object acceptObj;
                    if (context.Environment.TryGetValue("websocket.Accept", out acceptObj))
                    {
                        if (acceptObj is Action<IDictionary<string, object>, Func<IDictionary<string, object>, Task>> acceptWebSocket)
                        {
                            Console.WriteLine("WebSocket upgrade request detected. Accepting...");

                            // Accept the WebSocket request and pass our handler function
                            acceptWebSocket(null, HandleWebSocketConnection);
                            return; // WebSocket connection handled, don't call next middleware
                        }
                        // if (acceptObj is Func<IDictionary<string, object>, Func<IDictionary<string, object>>, Task> acceptWebSocket)
                        // {
                        //     Console.WriteLine("WebSocket upgrade request detected. Accepting...");
                        //
                        //     // Accept the WebSocket request and pass our handler function
                        //     await acceptWebSocket(null, HandleWebSocketConnection); // Pass null for default options
                        //     return; // WebSocket connection handled, don't call next middleware
                        // }
                        else
                        {
                            Console.WriteLine("websocket.Accept was not the expected delegate type.");
                            context.Response.StatusCode = 500; // Internal server error
                            return;
                        }
                    }
                    else
                    {
                        // Not a WebSocket request for this path, maybe return 400 Bad Request
                        Console.WriteLine("Request to /ws was not a WebSocket upgrade request.");
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("This endpoint requires a WebSocket connection.");
                        return;
                    }
                }

                // If the path doesn't match "/ws" or it wasn't a WebSocket request handled above,
                // pass the request to the next middleware in the pipeline (if any).
                await next.Invoke();
            });

            // You could add other middleware here (e.g., for static files, other API endpoints)
            // app.UseWelcomePage(); // Example: Add a default welcome page for other paths

            Console.WriteLine("OWIN pipeline configured.");
        }

        // --- WebSocket Handling Logic ---
        private async Task HandleWebSocketConnection(IDictionary<string, object> wsEnv)
        {
            // The OWIN environment dictionary for a WebSocket connection contains the WebSocket object.
            var ws = wsEnv["System.Net.WebSockets.WebSocketContext"] as WebSocketContext; // Or directly as WebSocket depending on host

            if (ws?.WebSocket == null) // Check if WebSocketContext exists and has a WebSocket
            {
                Console.WriteLine("Failed to get WebSocket object from environment.");
                // Potentially try getting it directly if WebSocketContext isn't populated by the host
                object wsObj;
                if (wsEnv.TryGetValue("System.Net.WebSockets.WebSocket", out wsObj) && wsObj is WebSocket directWs)
                {
                    await ProcessWebSocket(directWs);
                }
                return; // Cannot proceed
            }

            await ProcessWebSocket(ws.WebSocket);
        }

        private async Task ProcessWebSocket(WebSocket webSocket)
        {
              Console.WriteLine($"WebSocket connection established: {webSocket.SubProtocol ?? "N/A"}");

              var buffer = new byte[1024 * 4]; // 4KB buffer
              var cancellationToken = CancellationToken.None; // You might want a real CancellationToken

              try
              {
                  // Keep listening as long as the client keeps the connection open
                  while (webSocket.State == WebSocketState.Open)
                  {
                      WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                      if (result.MessageType == WebSocketMessageType.Text)
                      {
                          string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                          Console.WriteLine($"Received: {receivedMessage}");

                          // Echo the message back
                          string responseMessage = $"Server received: {receivedMessage}";
                          byte[] responseBytes = Encoding.UTF8.GetBytes(responseMessage);
                          await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, result.EndOfMessage, cancellationToken);
                          Console.WriteLine($"Sent: {responseMessage}");
                      }
                      else if (result.MessageType == WebSocketMessageType.Binary)
                      {
                          Console.WriteLine($"Received binary data: {result.Count} bytes");
                          // Handle binary data if needed
                          await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), WebSocketMessageType.Binary, result.EndOfMessage, cancellationToken);
                      }
                      else if (result.MessageType == WebSocketMessageType.Close)
                      {
                          Console.WriteLine($"Close message received: {result.CloseStatus?.ToString() ?? "N/A"} - {result.CloseStatusDescription ?? "N/A"}");
                          await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing normally", cancellationToken);
                          break; // Exit loop after receiving close
                      }
                  }
              }
              catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
              {
                   Console.WriteLine($"WebSocket connection closed prematurely: {ex.Message}");
              }
              catch (Exception ex)
              {
                  Console.WriteLine($"WebSocket error: {ex.GetType().Name} - {ex.Message}");
                  // Attempt to close gracefully if still possible
                  if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                  {
                      await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Server error", cancellationToken);
                  }
              }
              finally
              {
                  // Clean up the WebSocket object if necessary
                  webSocket?.Dispose();
                  Console.WriteLine("WebSocket connection closed.");
              }
        }
    }
}
