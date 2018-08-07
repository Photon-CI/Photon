using Photon.Server.Internal.Security;
using Photon.Server.ViewModels.Session;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Session
{
    [Secure]
    [RequiresRoles(GroupRole.BuildView, GroupRole.DeployView)]
    [HttpHandler("session/details")]
    internal class SessionDetailsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var sessionId = GetQuery("id");

            var vm = new SessionDetailsVM(this) {
                PageTitle = "Photon Server Session Details",
                SessionId = sessionId,
            };

            vm.Build();

            return Response.View("Session\\Details.html", vm);
        }
    }
}
