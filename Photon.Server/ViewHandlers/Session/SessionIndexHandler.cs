using Photon.Library;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Session
{
    [Secure]
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
