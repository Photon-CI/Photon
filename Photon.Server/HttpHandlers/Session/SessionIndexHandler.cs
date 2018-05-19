using Photon.Library;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers.Session
{
    [HttpHandler("/sessions")]
    [HttpHandler("/session/index")]
    internal class SessionIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase {
                PageTitle = "Photon Server Sessions",
            };

            return Response.View("Session\\Index.html", vm);
        }
    }
}
