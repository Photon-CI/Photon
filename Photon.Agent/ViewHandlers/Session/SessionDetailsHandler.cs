using Photon.Agent.Internal.Security;
using Photon.Agent.ViewModels.Session;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Agent.ViewHandlers.Session
{
    [Secure]
    [RequiresRoles(GroupRole.SessionView)]
    [HttpHandler("session/details")]
    internal class SessionDetailsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var sessionId = GetQuery("id");

            var vm = new SessionDetailsVM(this) {
                PageTitle = "Photon Agent Session Details",
                SessionId = sessionId,
            };

            vm.Build();

            return Response.View("Session\\Details.html", vm);
        }
    }
}
