using Photon.Server.Internal.Security;
using Photon.Server.ViewModels.Agent;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Agent
{
    [Secure]
    [RequiresRoles(GroupRole.AgentEdit)]
    [HttpHandler("/agent/edit/json")]
    internal class AgentEditJsonHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var id = GetQuery("id");

            var vm = new AgentEditJsonVM(this) {
                PageTitle = "Photon Server Edit Agent JSON",
                AgentId = id,
            };

            vm.Build();

            return Response.View("Agent\\EditJson.html", vm);
        }
    }
}
