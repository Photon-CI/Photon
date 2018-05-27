using Photon.Library;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.HttpHandlers.Session
{
    [HttpHandler("/sessions")]
    [HttpHandler("/session/index")]
    internal class SessionsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase();

            return Response.View("Session\\Index.html", vm);
        }
    }
}
