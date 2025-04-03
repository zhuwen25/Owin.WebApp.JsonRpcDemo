using System;
using System.Diagnostics;
using System.Web;
using System.Web.Http;

namespace WcfWithWsServer.WsApiCtler
{
    [RoutePrefix("api/test")]
    public class TestController:ApiController
    {
        [HttpGet]
        [Route("context")]
        public IHttpActionResult GetContextStatus()
        {
            bool isContextAvailable = HttpContext.Current != null;
            string message = $"[TestController] HttpContext.Current is null? {!isContextAvailable}";
            Debug.WriteLine(message);
            Console.WriteLine(message);
            return Ok(message);
        }
    }
}
