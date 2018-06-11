using Photon.Agent.ViewModels.Session;
using PiServerLite.Http.Handlers;
using System;

namespace Photon.Agent.ViewHandlers.Session
{
    [HttpHandler("session/details")]
    internal class SessionDetailsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var sessionId = GetQuery("id");

            var vm = new SessionDetailsVM {
                PageTitle = "Photon Agent Session Details",
                SessionId = sessionId,
            };

            try {
                vm.Build();
            }
            catch (Exception error) {
                vm.Errors.Add(error);
            }

            return Response.View("Session\\Details.html", vm);
        }
    }
}
