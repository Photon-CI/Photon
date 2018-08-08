using Photon.Agent.Internal;
using Photon.Agent.Internal.Security;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Agent.ViewHandlers.Session
{
    [Secure]
    [RequiresRoles(GroupRole.SessionView)]
    [HttpHandler("/sessions")]
    [HttpHandler("/session/index")]
    internal class SessionsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new AgentViewModel(this) {
                PageTitle = "Photon Agent Sessions",
            };

            vm.Build();

            return Response.View("Session\\Index.html", vm);
        }
    }
}
