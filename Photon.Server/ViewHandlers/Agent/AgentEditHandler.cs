using System;
using Photon.Server.ViewModels.Agent;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Agent
{
    [Secure]
    [HttpHandler("/agent/edit")]
    internal class AgentEditHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new AgentEditVM {
                AgentId = GetQuery("id"),
            };

            vm.PageTitle = $"Photon Server {(vm.IsNew?"New":"Edit")} Agent";

            vm.Build();

            return Response.View("Agent\\Edit.html", vm);
        }

        public override HttpHandlerResult Post()
        {
            var vm = new AgentEditVM();

            try {
                vm.Restore(Request.FormData());
                vm.Save();

                return Response.Redirect("Agents");
            }
            catch (Exception error) {
                vm.Errors.Add(error);
                return Response.View("Agent\\Edit.html", vm);
            }
        }
    }
}
