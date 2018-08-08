using Photon.Server.Internal.Security;
using Photon.Server.ViewModels.Agent;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Agent
{
    [Secure]
    [HttpHandler("/agents")]
    [HttpHandler("/agent/index")]
    [RequiresRoles(GroupRole.AgentView)]
    internal class AgentIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new AgentIndexVM(this) {
                PageTitle = "Photon Server Agents",
            };

            vm.Build();

            return Response.View("Agent\\Index.html", vm);
        }
    }
}
