using Photon.Server.ViewModels.Session;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers.Session
{
    [HttpHandler("session/details")]
    internal class SessionDetailsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var sessionId = GetQuery("id");

            var vm = new SessionDetailsVM {
                PageTitle = "Photon Server Session Details",
                SessionId = sessionId,
            };

            //try {
            //    vm.
            //}

            return Response.View("Session\\Details.html", vm);
        }
    }
}
