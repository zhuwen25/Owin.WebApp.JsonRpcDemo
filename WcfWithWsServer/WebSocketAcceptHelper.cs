using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace WcfWithWsServer;

/// <summary>
/// Contains OWIN helper methods, including WebSocket acceptance.
/// </summary>
public static class OwinWebSocketHelper
{
    /// <summary>
    /// Accepts an OWIN WebSocket request using the "websocket.Accept" delegate.
    /// It ensures the returned Task completes only after the provided userCallback finishes.
    /// </summary>
    /// <param name="context">The IOwinContext for the current request.</param>
    /// <param name="userCallback">An asynchronous function that takes the established WebSocket
    /// and handles the communication session. This callback is awaited internally.</param>
    /// <param name="options">Optional dictionary containing options for the WebSocket handshake
    /// (e.g., subprotocol using key "websocket.SubProtocol").</param>
    /// <returns>A Task that completes when the userCallback finishes, or faults if an error occurs
    /// during handshake, retrieving the socket, or within the userCallback.</returns>
    /// <exception cref="ArgumentNullException">Thrown if context or userCallback is null.</exception>
    /// <exception cref="NotSupportedException">Thrown if the OWIN host does not provide the "websocket.Accept" delegate.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the WebSocket object cannot be retrieved after handshake.</exception>
    public static Task AcceptWebSocketAsync(
        IOwinContext context,
        Func<WebSocket, Task> userCallback,
        IDictionary<string, object> options = null)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (userCallback == null) throw new ArgumentNullException(nameof(userCallback));

        // This TaskCompletionSource is the key. Its Task will be returned to the caller.
        // We will signal it (SetResult or SetException) only *after* the userCallback completes.
        var tcs = new TaskCompletionSource<object>(); // Using object type, result value doesn't matter

        // 1. Get the standard OWIN WebSocket accept delegate from the environment.
        var acceptDelegate = context.Get<Action<IDictionary<string, object>, Func<IDictionary<string, object>, Task>>>("websocket.Accept");

        if (acceptDelegate == null)
        {
            var ex = new NotSupportedException("OWIN WebSocket Accept delegate ('websocket.Accept') was not found in the environment. Ensure WebSocket support is enabled in the host.");
            tcs.TrySetException(ex); // Signal failure immediately
            return tcs.Task; // Return the faulted task
            // Alternatively, just throw:
            // throw ex;
        }

        // 2. Call the OWIN accept delegate.
        //    Pass the options dictionary (e.g., for subprotocol negotiation).
        //    Pass our internal wrapper callback that will execute the user's callback.
        acceptDelegate(options, async owinEnvironment => // This is the OWIN callback executed AFTER successful handshake
        {
            WebSocket webSocket = null;
            try
            {
                // 3. Retrieve the WebSocket object from the OWIN environment dictionary.
                //    The key might differ based on the OWIN server. Check common keys.
                if (owinEnvironment.TryGetValue("System.Net.WebSockets.WebSocketContext", out object ctxObj) && ctxObj is WebSocketContext wsContext)
                {
                    webSocket = wsContext.WebSocket;
                    // You could also check wsContext.SecWebSocketProtocol here if needed
                }
                else if (owinEnvironment.TryGetValue("websocket.WebSocket", out object wsObj) && wsObj is WebSocket ws) // Fallback
                {
                    webSocket = ws;
                }

                if (webSocket == null)
                {
                    // This would be unusual if the handshake succeeded, but defensively check.
                    throw new InvalidOperationException("Failed to retrieve WebSocket instance from OWIN environment after handshake.");
                }

                // --- This is the crucial part ---
                // 4. Execute the user's provided callback and AWAIT its completion.
                //    This ensures we don't signal success until the user's async work is done.
                Console.WriteLine($"AcceptWebSocketAsync: Invoking user callback for socket {webSocket.GetHashCode()}...");
                await userCallback(webSocket);
                Console.WriteLine($"AcceptWebSocketAsync: User callback completed for socket {webSocket.GetHashCode()}.");
                // ---------------------------------

                // 5. If the userCallback completed without exception, signal success.
                tcs.TrySetResult(null); // Result value doesn't matter, just signal completion
            }
            catch (Exception ex)
            {
                // 6. If any exception occurred (getting socket or during userCallback), signal failure.
                Console.WriteLine($"AcceptWebSocketAsync: Exception occurred in OWIN callback or user callback: {ex}");
                tcs.TrySetException(ex);
            }
            finally
            {
                 // IMPORTANT: Socket Disposal Responsibility
                 // It's generally best practice for the code that handles the
                 // communication loop (i.e., your userCallback or the service it calls)
                 // to be responsible for Disposing the WebSocket when done or on error.
                 // Avoid disposing the socket here if it was successfully passed to userCallback.
                 // If an error occurred BEFORE calling userCallback (e.g., getting the socket),
                 // then 'webSocket' might be null or should be disposed here if not null.
            }
        });

        // 7. Return the Task from the TaskCompletionSource.
        // The caller will await this Task, and it will only complete when
        // tcs.TrySetResult or tcs.TrySetException is called inside the OWIN callback.
        Console.WriteLine("AcceptWebSocketAsync: Returning TaskCompletionSource task to caller.");
        return tcs.Task;
    }
}
