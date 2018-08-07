using Photon.Server.Internal.Security;
using Photon.Server.ViewModels.Agent;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;

namespace Photon.Server.ViewHandlers.Agent
{
    [Secure]
    [RequiresRoles(GroupRole.AgentEdit)]
    [HttpHandler("/agent/edit")]
    internal class AgentEditHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new AgentEditVM(this) {
                AgentId = GetQuery("id"),
            };

            vm.PageTitle = $"Photon Server {(vm.IsNew?"New":"Edit")} Agent";

            vm.Build();

            return Response.View("Agent\\Edit.html", vm);
        }

        public override HttpHandlerResult Post()
        {
            var vm = new AgentEditVM(this);

            try {
                vm.Restore(Request.FormData());
                vm.Save();

                return Response.Redirect("Agents");
            }
            catch (Exception error) {
                vm.Errors.Add(error);

                vm.Build();

                return Response.View("Agent\\Edit.html", vm);
            }
        }
    }
}
