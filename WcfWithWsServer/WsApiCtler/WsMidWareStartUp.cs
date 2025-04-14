#if NETFRAMEWORK
using Microsoft.Owin;
using Owin;
using System;
using System.Web.Http;

[assembly: OwinStartup(typeof(WcfWithWsServer.WsApiCtler.WsMidWareStartUp))]
namespace WcfWithWsServer.WsApiCtler
{
    public class WsMidWareStartUp
    {
        public void Configuration(IAppBuilder app)
        {
            Console.WriteLine("win Startup: Configuration method started.");
            // -- Web API configuration
            HttpConfiguration config = new HttpConfiguration();
            //Enable Attribute Routing(Recommend for defining routes on controllers)
            config.MapHttpAttributeRoutes();

            //Optional: Enable convention-based routing(if you want routes like /api/controller/id)
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //Use Web API Middleware
            app.UseWebApi(config);
           // app.UseWebSockets();
            Console.WriteLine("Web API configured and added to OWIN pipeline.");
            Console.WriteLine("UseWebApi Startup: HttpConfiguration method finished.");

            // Add WebSocket Middleware AFTER Web API (or based on desired path handling)
            // This middleware will only handle requests matching the "/ws" path
            //app.Use<WebSocketMiddleware>("/api/jsonrpc");
        }
    }
}
#endif
