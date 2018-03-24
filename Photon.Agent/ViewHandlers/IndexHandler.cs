using PiServerLite.Http.Handlers;

namespace Photon.Agent.Handlers
{
    [HttpHandler("/")]
    [HttpHandler("/index")]
    internal class IndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            return View("Index.html");
        }
    }
}
