using Photon.Server.Internal;
using Photon.Server.Internal.Security;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Session
{
    [Secure]
    [RequiresRoles(GroupRole.BuildView, GroupRole.DeployView)]
    [HttpHandler("/sessions")]
    [HttpHandler("/session/index")]
    internal class SessionIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ServerViewModel(this) {
                PageTitle = "Photon Server Sessions",
            };

            vm.Build();

            return Response.View("Session\\Index.html", vm);
        }
    }
}
