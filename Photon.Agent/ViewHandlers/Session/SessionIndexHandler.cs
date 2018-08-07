using Photon.Agent.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.ViewHandlers.Session
{
    [HttpHandler("/sessions")]
    [HttpHandler("/session/index")]
    internal class SessionsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new AgentViewModel {
                PageTitle = "Photon Agent Sessions",
            };

            return Response.View("Session\\Index.html", vm);
        }
    }
}
